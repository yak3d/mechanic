namespace Mechanic.Core.Services;

using Contracts;
using Errors;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using LanguageExt;
using static LanguageExt.Prelude;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Models.Steam;

public class WindowsSteamService(ILogger<WindowsSteamService> logger, IRegistryService registryService) : ISteamService(logger)
{
    private const string SteamappsDirectory = "steamapps";
    private const string SteamRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam";
    private const string SteamRegistryKey64Bit = @"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam";
    private const string SteamRegistryInstallPathKey = "InstallPath";
    private const string DefaultSteamDirectory = @"C:\Program Files (x86)\Steam";

    public override Either<SteamManifestError, List<SteamGame>> GetInstalledGames()
    {
        var allGames = new List<SteamGame>();
        var libraryPaths = this.GetLibraryPaths();

        return libraryPaths.Match(
            Right: paths =>
            {
                foreach (var libraryPath in paths)
                {
                    var steamAppsPath = Path.Combine(libraryPath, SteamappsDirectory);
                    if (!Directory.Exists(steamAppsPath)) continue;

                    var manifestFilesResult = Try(() => Directory.GetFiles(steamAppsPath, "appmanifest_*.acf"))
                        .ToEither(ex =>
                            (SteamManifestError)new SteamManifestError.IoError("ManifestEnumeration", ex.Message));

                    if (manifestFilesResult.IsLeft)
                        return manifestFilesResult.Map(_ => new List<SteamGame>());

                    var manifestFiles = manifestFilesResult.RightAsEnumerable().First();

                    foreach (var manifestFile in manifestFiles)
                    {
                        var gameResult = this.ParseGameManifest(manifestFile, steamAppsPath);

                        if (gameResult.IsLeft)
                            return gameResult.Map(_ => new List<SteamGame>());

                        allGames.Add(gameResult.RightAsEnumerable().First());
                    }
                }

                return allGames.OrderBy(g => g.Name).ToList();
            },
            Left: error => error
        );
    }

    public override string GetSteamInstallPath()
    {
        try
        {
            return (registryService.GetValue(SteamRegistryKey, SteamRegistryInstallPathKey, null) as string) ??
                   (registryService.GetValue(SteamRegistryKey64Bit, SteamRegistryInstallPathKey, null) as string) ??
                   DefaultSteamDirectory;
        }
        catch
        {
            return DefaultSteamDirectory;
        }
    }

    protected override Either<SteamManifestError, List<string>> GetLibraryPaths()
    {
        var libraryPaths = new List<string>();
        var steamPath = this.GetSteamInstallPath();

        if (string.IsNullOrEmpty(steamPath))
        {
            return libraryPaths;
        }

        libraryPaths.Add(steamPath);

        try
        {
            var libraryFoldersPath = Path.Combine(steamPath, SteamappsDirectory, "libraryfolders.vdf");

            if (File.Exists(libraryFoldersPath))
            {
                var vdfContent = File.ReadAllText(libraryFoldersPath);
                var libraryData = VdfConvert.Deserialize(vdfContent);

                var libraryFolders = libraryData.Value as VToken;

                libraryFolders.Children().ToList().ForEach(folder =>
                {
                    if (folder is not VProperty property || !int.TryParse(property.Key, out _))
                    {
                        return;
                    }

                    var folderData = property.Value as VToken;
                    var pathProperty =
                        folderData.Children()
                            .FirstOrDefault(p => p is VProperty { Key: "path" }) as VProperty;

                    if (pathProperty?.Value is not VValue pathValue)
                    {
                        return;
                    }

                    var libraryPath = pathProperty.Value<string>();
                    if (!string.IsNullOrEmpty(libraryPath) && Directory.Exists(libraryPath))
                    {
                        libraryPaths.Add(libraryPath);
                    }
                });
            }
        }
        catch (Exception)
        {
            return Left<SteamManifestError, List<string>>(new SteamManifestError.LibraryFoldersNotFound(steamPath));
        }

        return libraryPaths.Distinct().ToList();
    }
}

namespace Mechanic.Core.Services;

using Contracts;
using Errors;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Infrastructure.Logging;
using LanguageExt;
using static LanguageExt.Prelude;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Models.Steam;

public class WindowsSteamService(
    ILogger<WindowsSteamService> logger,
    IRegistryService registryService,
    IFileService fileService)
    : ISteamService(logger, fileService)
{
    private readonly IFileService fileService = fileService;

    private const string SteamappsDirectory = "steamapps";
    private const string SteamRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam";
    private const string SteamRegistryKey64Bit = @"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam";
    private const string SteamRegistryInstallPathKey = "InstallPath";
    private const string DefaultSteamDirectory = @"C:\Program Files (x86)\Steam";

    public override async Task<Either<SteamManifestError, List<SteamGame>>> GetInstalledGamesAsync()
    {
        var allGames = new List<SteamGame>();
        var libraryPaths = await this.GetLibraryPathsAsync();

        return await libraryPaths.Match(
            Right: async paths =>
            {
                logger.FoundSteamLibraries(paths.ToArray());
                foreach (var libraryPath in paths)
                {
                    var steamAppsPath = Path.Combine(libraryPath, SteamappsDirectory);
                    if (!await this.fileService.DirectoryExists(steamAppsPath))
                    {
                        continue;
                    }

                    try
                    {
                        var manifestFiles =
                            await this.fileService.GetFilesFromDirectoryAsync(steamAppsPath, "appmanifest_*.acf");

                        foreach (var manifestFile in manifestFiles)
                        {
                            var gameResult = await this.ParseGameManifestAsync(manifestFile, steamAppsPath);

                            if (gameResult.IsLeft)
                                return gameResult.Map(_ => new List<SteamGame>());

                            allGames.Add(gameResult.RightAsEnumerable().First());
                        }
                    }
                    catch (Exception ex)
                    {
                        return Left<SteamManifestError, List<SteamGame>>(
                            new SteamManifestError.IoError("ManifestEnumeration", ex.Message));
                    }
                }

                return Right<SteamManifestError, List<SteamGame>>(allGames.OrderBy(g => g.Name).ToList());
            },
            Left: error => Task.FromResult(Left<SteamManifestError, List<SteamGame>>(error))
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

    protected override async Task<Either<SteamManifestError, List<string>>> GetLibraryPathsAsync()
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
                var vdfContent = await this.fileService.ReadAllText(libraryFoldersPath);
                var libraryData = VdfConvert.Deserialize(vdfContent);

                var libraryFolders = libraryData.Value as VToken;

                libraryFolders.Children().ToList().ForEach(async folder =>
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

                    var libraryPath = pathProperty.Value.ToString();
                    var libraryExists = await this.fileService.DirectoryExists(libraryPath);
                    if (!string.IsNullOrEmpty(libraryPath) && libraryExists)
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

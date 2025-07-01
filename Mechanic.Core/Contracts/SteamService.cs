namespace Mechanic.Core.Contracts;

using Constants;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Infrastructure.Logging;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Models.Steam;
using Services.Errors;
using static LanguageExt.Prelude;

public abstract class SteamService(ILogger<SteamService> logger, IFileService fileService)
{
    protected static readonly string[] ValidAppIds =
    [
        "22330",  // Oblivion GOTY
        "900883", // Oblivion GOTY Deluxe
        "72850",  // Skyrim
        "489830", // Skyrim Special Edition
        "22300",  // Fallout 3
        "22370",  // Fallout 3 GOTY
        "377160", // Fallout 4
        "1716740" // Starfield
    ];
    public abstract Task<Either<SteamManifestError, List<SteamGame>>> GetInstalledGamesAsync();
    public abstract string GetSteamInstallPath();
    protected abstract Task<Either<SteamManifestError, List<string>>> GetLibraryPathsAsync();

    protected async Task<Either<SteamManifestError, SteamGame>> ParseGameManifestAsync(string manifestPath, string steamAppsPath)
    {
        try
        {
            var vdfContent = await fileService.ReadAllText(manifestPath);
            var manifestData = VdfConvert.Deserialize(vdfContent);

            var appState = manifestData.Value;

            var appId = GetVdfValue(appState, SteamVdfConstants.AppIdKey);
            var name = GetVdfValue(appState, SteamVdfConstants.AppNameKey);
            var installDir = GetVdfValue(appState, SteamVdfConstants.AppInstallDirKey);
            var lastUpdated = GetVdfValue(appState, SteamVdfConstants.AppLastUpdatedKey);

            if (string.IsNullOrEmpty(installDir))
            {
                return Left<SteamManifestError, SteamGame>(
                    new SteamManifestError.VdfParseError(manifestPath, "Missing install directory"));
            }
            var fullPath = Path.Combine(steamAppsPath, "common", installDir);

            return new SteamGame(
                AppId: appId ?? string.Empty,
                Name: name ?? string.Empty,
                InstallDir: installDir,
                FullPath: fullPath,
                LastUpdated: long.TryParse(lastUpdated, out var timestamp)
                    ? DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime
                    : DateTime.MinValue
            );
        }
        catch (Exception e)
        {
            logger.SteamManifestDeserializingError(e);
            return Left<SteamManifestError, SteamGame>(new SteamManifestError.VdfParseError(manifestPath, e.Message));
        }
    }

    private static string? GetVdfValue(VToken token, string key)
    {
        var property = token?.Children()?.FirstOrDefault(prop => prop is VProperty vProp && vProp.Key == key) as VProperty;

        return property?.Value is VValue value ? value.Value<string>() : null;
    }
}

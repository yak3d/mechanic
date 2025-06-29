namespace Mechanic.Core.Contracts;

using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Infrastructure.Logging;
using LanguageExt;
using static LanguageExt.Prelude;
using Microsoft.Extensions.Logging;
using Models.Steam;
using Services.Errors;

public abstract class ISteamService(ILogger<ISteamService> logger)
{
    public abstract Either<SteamManifestError, List<SteamGame>> GetInstalledGames();
    public abstract string GetSteamInstallPath();
    protected abstract Either<SteamManifestError, List<string>> GetLibraryPaths();

    protected Either<SteamManifestError, SteamGame> ParseGameManifest(string manifestPath, string steamAppsPath)
    {
        try
        {
            var vdfContent = File.ReadAllText(manifestPath);
            var manifestData = VdfConvert.Deserialize(vdfContent);

            var appState = manifestData.Value;

            var appId = GetVdfValue(appState, "appId");
            var name = GetVdfValue(appState, "name");
            var installDir = GetVdfValue(appState, "installdir");
            var lastUpdated = GetVdfValue(appState, "LastUpdated");

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
    }}

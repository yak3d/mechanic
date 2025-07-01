namespace Mechanic.Core.Services.Errors;

public abstract record SteamManifestError
{
    public record SteamNotInstalled : SteamManifestError;
    public record RegistryAccessDenied(string Details) : SteamManifestError;
    public record LibraryFoldersNotFound(string ExpectedPath) : SteamManifestError;
    public record VdfParseError(string FilePath, string Error) : SteamManifestError;
    public record ManifestCorrupted(string ManifestPath, string Details) : SteamManifestError;
    public record IoError(string Operation, string Details) : SteamManifestError;
    public record NoGamesFound : SteamManifestError;
}

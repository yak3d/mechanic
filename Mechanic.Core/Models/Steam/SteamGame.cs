namespace Mechanic.Core.Models.Steam;

public record SteamGame(string AppId, string Name, string InstallDir, string FullPath, DateTime LastUpdated);

namespace Mechanic.CLI.Models;

public enum GameFileType
{
    Other = 0,
    Material = 1,
    DirectDrawSurface = 2,
    WwiseEncodedMedia = 3,
    PapyrusExecutable = 4,
    PapyrusProject = 5
}

public static class GameFileTypeExtensions
{
    public static Core.Models.GameFileType ToDomain(this GameFileType gameFileType) => gameFileType switch
    {
        GameFileType.Other => Core.Models.GameFileType.Other,
        GameFileType.Material => Core.Models.GameFileType.Material,
        GameFileType.DirectDrawSurface => Core.Models.GameFileType.DirectDrawSurface,
        GameFileType.WwiseEncodedMedia => Core.Models.GameFileType.WwiseEncodedMedia,
        GameFileType.PapyrusExecutable => Core.Models.GameFileType.PapyrusExecutable,
        GameFileType.PapyrusProject => Core.Models.GameFileType.PapyrusProject,
        _ => throw new ArgumentOutOfRangeException(nameof(gameFileType), gameFileType, null)
    };

    public static GameFileType FromDomain(this Core.Models.GameFileType domainGameFileType) => domainGameFileType switch
    {
        Core.Models.GameFileType.Other => GameFileType.Other,
        Core.Models.GameFileType.Material => GameFileType.Material,
        Core.Models.GameFileType.DirectDrawSurface => GameFileType.DirectDrawSurface,
        Core.Models.GameFileType.WwiseEncodedMedia => GameFileType.WwiseEncodedMedia,
        Core.Models.GameFileType.PapyrusExecutable => GameFileType.PapyrusExecutable,
        Core.Models.GameFileType.PapyrusProject => GameFileType.PapyrusProject,
        _ => throw new ArgumentOutOfRangeException(nameof(domainGameFileType), domainGameFileType, null)
    };
}

namespace Mechanic.CLI.Models;

public enum GameFileType
{
    Material = 0,
    DirectDrawSurface = 1,
    WwiseEncodedMedia = 2,
    PapyrusExecutable = 3
}

public static class GameFileTypeExtensions
{
    public static Core.Models.GameFileType ToDomain(this GameFileType gameFileType) => gameFileType switch
    {
        GameFileType.Material => Core.Models.GameFileType.Material,
        GameFileType.DirectDrawSurface => Core.Models.GameFileType.DirectDrawSurface,
        GameFileType.WwiseEncodedMedia => Core.Models.GameFileType.WwiseEncodedMedia,
        GameFileType.PapyrusExecutable => Core.Models.GameFileType.PapyrusExecutable,
        _ => throw new ArgumentOutOfRangeException(nameof(gameFileType), gameFileType, null)
    };
    
    public static GameFileType FromDomain(this Core.Models.GameFileType domainGameFileType) => domainGameFileType switch
    {
        Core.Models.GameFileType.Material => GameFileType.Material,
        Core.Models.GameFileType.DirectDrawSurface => GameFileType.DirectDrawSurface,
        Core.Models.GameFileType.WwiseEncodedMedia => GameFileType.WwiseEncodedMedia,
        Core.Models.GameFileType.PapyrusExecutable => GameFileType.PapyrusExecutable,
        _ => throw new ArgumentOutOfRangeException(nameof(domainGameFileType), domainGameFileType, null)
    };
}

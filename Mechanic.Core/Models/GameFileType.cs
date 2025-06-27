namespace Mechanic.Core.Models;
using Mechanic.Core.Project.Models.Json;

public enum GameFileType
{
    Other = 0,
    Material = 1,
    DirectDrawSurface = 2,
    WwiseEncodedMedia = 3,
    PapyrusExecutable = 4
}

public static class GameFileTypeExtensions
{
    public static GameFilesFileType ToJson(this GameFileType gameFileType) => gameFileType switch
    {
        GameFileType.Other => GameFilesFileType.OTHER,
        GameFileType.Material => GameFilesFileType.MAT,
        GameFileType.DirectDrawSurface => GameFilesFileType.DDS,
        GameFileType.WwiseEncodedMedia => GameFilesFileType.WEM,
        GameFileType.PapyrusExecutable => GameFilesFileType.PEX,
        _ => throw new ArgumentOutOfRangeException(nameof(gameFileType), gameFileType, null)
    };
}

public static class GameFilesFileTypeExtensions
{
    public static GameFileType ToGameFileType(this GameFilesFileType jsonGameFileType) =>
        jsonGameFileType switch
        {
            GameFilesFileType.OTHER => GameFileType.Other,
            GameFilesFileType.MAT => GameFileType.Material,
            GameFilesFileType.DDS => GameFileType.DirectDrawSurface,
            GameFilesFileType.WEM => GameFileType.WwiseEncodedMedia,
            GameFilesFileType.PEX => GameFileType.PapyrusExecutable,
            _ => throw new ArgumentOutOfRangeException(nameof(jsonGameFileType), jsonGameFileType, null)
        };
}

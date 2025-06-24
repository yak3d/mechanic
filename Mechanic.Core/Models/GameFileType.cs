using Mechanic.Core.Project.Models.Json;

namespace Mechanic.Core.Models;

public enum GameFileType
{
    Material = 0,
    DirectDrawSurface = 1,
    WwiseEncodedMedia = 2,
    PapyrusExecutable = 3
}

public static class GameFileTypeExtensions
{
    public static GameFilesFileType ToJson(this GameFileType gameFileType) => gameFileType switch
    {
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
            GameFilesFileType.MAT => GameFileType.Material,
            GameFilesFileType.DDS => GameFileType.DirectDrawSurface,
            GameFilesFileType.WEM => GameFileType.WwiseEncodedMedia,
            GameFilesFileType.PEX => GameFileType.PapyrusExecutable,
            _ => throw new ArgumentOutOfRangeException(nameof(jsonGameFileType), jsonGameFileType, null)
        };
}

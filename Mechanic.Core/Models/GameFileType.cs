using Mechanic.Core.Project.Models.Json;

namespace Mechanic.Core.Models;

public enum GameFileType
{
    Mat = 0,
    Dds = 1,
    Wem = 2,
    Pex = 3
}

public static class GameFileTypeExtensions
{
    public static GameFileType ToGameFileType(this GameFilesFileType jsonGameFileType) =>
        jsonGameFileType switch
        {
            GameFilesFileType.MAT => GameFileType.Mat,
            GameFilesFileType.DDS => GameFileType.Dds,
            GameFilesFileType.WEM => GameFileType.Wem,
            GameFilesFileType.PEX => GameFileType.Pex,
            _ => throw new ArgumentOutOfRangeException(nameof(jsonGameFileType), jsonGameFileType, null)
        };
}
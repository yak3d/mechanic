namespace Mechanic.Core.Models;
using Mechanic.Core.Project.Models.Json;

public enum GameFileType
{
    Other = 0,
    Material = 1,
    DirectDrawSurface = 2,
    WwiseEncodedMedia = 3,
    PapyrusExecutable = 4,
    PapyrusProject = 5,
    NetImmerseFile = 6,
    Mesh = 7
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
        GameFileType.PapyrusProject => GameFilesFileType.PPJ,
        GameFileType.NetImmerseFile => GameFilesFileType.NIF,
        GameFileType.Mesh => GameFilesFileType.MESH,
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
            GameFilesFileType.PPJ => GameFileType.PapyrusProject,
            GameFilesFileType.NIF => GameFileType.NetImmerseFile,
            GameFilesFileType.MESH => GameFileType.Mesh,
            _ => throw new ArgumentOutOfRangeException(nameof(jsonGameFileType), jsonGameFileType, null)
        };
}

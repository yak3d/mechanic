namespace Mechanic.Core.Models;
using Mechanic.Core.Project.Models.Json;

public class GameFile : ProjectFile
{
    public GameFileType GameFileType { get; set; }

    public GameFiles ToJson() => new()
    {
        Id = this.Id.ToString(),
        Path = this.Path,
        FileType = this.GameFileType.ToJson()
    };

    public static GameFile FromJson(GameFiles gameFile) => new()
    {
        Id = Guid.Parse(gameFile.Id),
        Path = gameFile.Path,
        GameFileType = gameFile.FileType.ToGameFileType(),
    };
}

using Mechanic.Core.Project.Models.Json;

namespace Mechanic.Core.Models;

public class GameFile : ProjectFile
{
    public GameFileType GameFileType { get; set; }

    public Mechanic.Core.Project.Models.Json.GameFiles ToJson() => new()
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

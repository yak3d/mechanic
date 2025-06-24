namespace Mechanic.Core.Models;

using Newtonsoft.Json;

public class MechanicProject
{
    [JsonProperty("$schema")]
    public string Schema { get; } = "Mechanic.Core/ProjectFileSchema.json";
    public required string Id { get; init; }
    public required Game Game { get; init; }
    public List<SourceFile> SourceFiles { get; set; } = [];
    public List<GameFile> DestinationFiles { get; set; } = [];

    public Project.Models.Json.MechanicProject ToJson() => new()
    {
        Id = this.Id,
        Game = this.Game.ToJson(),
        SourceFiles = this.SourceFiles.Select(file => file.ToJson()).ToList(),
        GameFiles = this.DestinationFiles.Select(file => file.ToJson()).ToList()
    };

    public static MechanicProject FromJsonObject(Core.Project.Models.Json.MechanicProject jsonObject)
    {
        return new MechanicProject
        {
            Id = jsonObject.Id,
            Game = jsonObject.Game.FromJsonGame(),
            SourceFiles = jsonObject.SourceFiles.Select(SourceFile.FromJsonProject).ToList(),
            DestinationFiles = jsonObject.GameFiles.Select(GameFile.FromJson).ToList()
        };
    }
}

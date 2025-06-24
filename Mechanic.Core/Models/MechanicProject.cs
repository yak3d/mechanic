namespace Mechanic.Core.Models;

public class MechanicProject
{
    public required string Id { get; init; }
    public List<SourceFile> SourceFiles { get; set; } = [];
    public List<GameFile> DestinationFiles { get; set; } = [];

    public static MechanicProject FromJsonObject(Core.Project.Models.Json.MechanicProject jsonObject)
    {
        return new MechanicProject
        {
            Id = jsonObject.Id,
            SourceFiles = jsonObject.SourceFiles.Select(SourceFile.FromJsonProject).ToList(),
            DestinationFiles = jsonObject.GameFiles.Select(GameFile.FromJson).ToList()
        };
    }
}
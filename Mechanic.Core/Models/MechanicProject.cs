namespace Mechanic.Core.Models;

using Newtonsoft.Json;

public class MechanicProject
{
    private Game game;

    [JsonProperty("$schema")]
    public string Schema { get; } = "Mechanic.Core/ProjectFileSchema.json";
    public required string Id { get; init; }

    public required Game Game
    {
        get => this.game;
        init => this.game = value;
    }

    public List<SourceFile> SourceFiles { get; private init; } = [];
    public List<GameFile> DestinationFiles { get; init; } = [];

    public SourceFile AddSourceFile(string path, SourceFileType fileType)
    {
        if (this.SourceFiles.Any(sourceFile => sourceFile.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"Source file already exists with path: {path}");
        }

        var sourceFile = new SourceFile
        {
            Id = Guid.NewGuid(),
            Path = path,
            FileType = fileType,
            DestinationPaths = []
        };

        this.SourceFiles.Add(sourceFile);

        return sourceFile;
    }

    public void ChangeGame(Game newGame) => this.game = newGame;

    public Project.Models.Json.MechanicProject ToJson() => new()
    {
        Id = this.Id,
        Game = this.Game.ToJson(),
        SourceFiles = [.. this.SourceFiles.Select(file => file.ToJson())],
        GameFiles = [.. this.DestinationFiles.Select(file => file.ToJson())]
    };

    public static MechanicProject FromJsonObject(Project.Models.Json.MechanicProject jsonObject) => new MechanicProject
    {
        Id = jsonObject.Id,
        Game = jsonObject.Game.FromJsonGame(),
        SourceFiles = [.. jsonObject.SourceFiles.Select(SourceFile.FromJsonProject)],
        DestinationFiles = [.. jsonObject.GameFiles.Select(GameFile.FromJson)]
    };
}

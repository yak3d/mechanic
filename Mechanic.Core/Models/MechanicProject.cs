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

    public List<SourceFile> SourceFiles { get; init; } = [];
    public List<GameFile> GameFiles { get; init; } = [];

    public SourceFile AddSourceFile(string path, SourceFileType fileType) => this.AddSourceFile(path, fileType, null);

    public SourceFile AddSourceFile(string path, SourceFileType fileType, Guid? gameFileGuid)
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
            GameFileLinks = gameFileGuid != null ? [gameFileGuid.Value] : []
        };

        this.SourceFiles.Add(sourceFile);

        return sourceFile;
    }

    public GameFile AddGameFile(string path, GameFileType fileType)
    {
        if (this.GameFiles.Any(gf => gf.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"Game file already exists with path: {path}");
        }

        var gameFile = new GameFile { Id = Guid.NewGuid(), Path = path, GameFileType = fileType };

        this.GameFiles.Add(gameFile);

        return gameFile;
    }

    public void ChangeGame(Game newGame) => this.game = newGame;

    public Project.Models.Json.MechanicProject ToJson() => new()
    {
        Id = this.Id,
        Game = this.Game.ToJson(),
        SourceFiles = [.. this.SourceFiles.Select(file => file.ToJson())],
        GameFiles = [.. this.GameFiles.Select(file => file.ToJson())]
    };

    public static MechanicProject FromJsonObject(Project.Models.Json.MechanicProject jsonObject) => new MechanicProject
    {
        Id = jsonObject.Id,
        Game = jsonObject.Game.FromJsonGame(),
        SourceFiles = [.. jsonObject.SourceFiles.Select(SourceFile.FromJsonProject)],
        GameFiles = [.. jsonObject.GameFiles.Select(GameFile.FromJson)]
    };
}

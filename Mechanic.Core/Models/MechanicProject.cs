namespace Mechanic.Core.Models;

using Newtonsoft.Json;
using Project.Models.Json;

public class MechanicProject
{
    private GameName gameName;

    [JsonProperty("$schema")]
    public string Schema { get; } = "Mechanic.Core/ProjectFileSchema.json";
    public required string Id { get; init; }
    public required string Namespace { get; init; }

    public required GameName GameName
    {
        get => this.gameName;
        init => this.gameName = value;
    }

    public ProjectSettings? ProjectSettings { get; init; }

    public required string GamePath { get; init; }

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

    public GameFile AddGameFile(string path, GameFileType fileType, Guid sourceFileId)
    {
        var gameFile = this.AddGameFile(path, fileType);
        this.SourceFiles.Where(sf => sf.Id == sourceFileId).ToList().ForEach(sf => sf.GameFileLinks.Add(gameFile.Id));

        return gameFile;
    }

    public void ChangeGame(GameName newGameName) => this.gameName = newGameName;

    public Mechanic.Core.Project.Models.Json.MechanicProject ToJson() => new()
    {
        Id = this.Id,
        Namespace = this.Namespace,
        Game = new Game
        {
            Name = this.GameName.ToJson(),
            Path = this.GamePath
        },
        ProjectSettings = this.ProjectSettings?.ToJsonModel(),
        SourceFiles = [.. this.SourceFiles.Select(file => file.ToJson())],
        GameFiles = [.. this.GameFiles.Select(file => file.ToJson())]
    };

    public static MechanicProject FromJsonObject(Core.Project.Models.Json.MechanicProject jsonObject) => new()
    {
        Id = jsonObject.Id,
        Namespace = jsonObject.Namespace,
        GameName = jsonObject.Game.Name.FromJsonGame(),
        GamePath = jsonObject.Game.Path,
        ProjectSettings = jsonObject.ProjectSettings?.ToDomain(),
        SourceFiles = [.. jsonObject.SourceFiles.Select(SourceFile.FromJsonProject)],
        GameFiles = [.. jsonObject.GameFiles.Select(GameFile.FromJson)]
    };
}

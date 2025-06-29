
using Mechanic.Core.Models;

namespace Mechanic.CLI.Models;

public class MechanicProject
{
    private Game game;

    public required string Id { get; init; }

    public required CLI.Models.Game Game
    {
        get => this.game;
        init => this.game = value;
    }

    public List<SourceFile> SourceFiles { get; private init; } = [];
    public List<CLI.Models.GameFile> GameFiles { get; init; } = [];

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
    public Core.Models.MechanicProject ToDomain()
    {
        return new Core.Models.MechanicProject
        {
            Id = Id,
            GameName = Game.ToDomain(),
            SourceFiles = [.. SourceFiles.Select(sf => sf.ToDomain())],
            GameFiles = [.. GameFiles.Select(df => df.ToDomain())]
        };
    }
    
    public static MechanicProject FromDomain(Core.Models.MechanicProject domainProject)
    {
        return new MechanicProject
        {
            Id = domainProject.Id,
            Game = domainProject.GameName.FromDomain(),
            SourceFiles = [.. domainProject.SourceFiles.Select(SourceFile.FromDomain)],
            GameFiles = [.. domainProject.GameFiles.Select(GameFile.FromDomain)]
        };
    }
}

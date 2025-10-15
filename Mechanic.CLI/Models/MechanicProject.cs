
namespace Mechanic.CLI.Models;

public class MechanicProject
{
    private readonly GameName gameName;

    public required string Id { get; init; }
    public required string Namespace { get; init; }

    public required GameName GameName
    {
        get => this.gameName;
        init => this.gameName = value;
    }

    public required ProjectSettings? ProjectSettings { get; init; }

    public required string GamePath { get; init; }

    public List<SourceFile> SourceFiles { get; private init; } = [];
    public List<GameFile> GameFiles { get; init; } = [];

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
    public Core.Models.MechanicProject ToDomain() =>
        new()
        {
            Id = this.Id,
            Namespace = this.Namespace,
            GameName = this.GameName.ToDomain(),
            GamePath = this.GamePath,
            ProjectSettings = this.ProjectSettings?.ToDomain(),
            SourceFiles = [.. this.SourceFiles.Select(sf => sf.ToDomain())],
            GameFiles = [.. this.GameFiles.Select(df => df.ToDomain())]
        };

    public static MechanicProject FromDomain(Core.Models.MechanicProject domainProject) =>
        new()
        {
            Id = domainProject.Id,
            Namespace = domainProject.Namespace,
            GameName = domainProject.GameName.FromDomain(),
            GamePath = domainProject.GamePath,
            ProjectSettings = domainProject.ProjectSettings?.ToCliModel(),
            SourceFiles = [.. domainProject.SourceFiles.Select(SourceFile.FromDomain)],
            GameFiles = [.. domainProject.GameFiles.Select(GameFile.FromDomain)]
        };
}

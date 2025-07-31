
namespace Mechanic.CLI.Models;

public class MechanicProject
{
    private GameName _gameName;

    public required string Id { get; init; }
    public required string Namespace { get; init; }

    public required CLI.Models.GameName GameName
    {
        get => this._gameName;
        init => this._gameName = value;
    }
    
    public required ProjectSettings ProjectSettings { get; init; }

    public required string GamePath { get; init; }

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
            Namespace = Namespace,
            GameName = GameName.ToDomain(),
            GamePath = GamePath,
            ProjectSettings = ProjectSettings.ToDomain(),
            SourceFiles = [.. SourceFiles.Select(sf => sf.ToDomain())],
            GameFiles = [.. GameFiles.Select(df => df.ToDomain())]
        };
    }

    public static MechanicProject FromDomain(Core.Models.MechanicProject domainProject)
    {
        return new MechanicProject
        {
            Id = domainProject.Id,
            Namespace = domainProject.Namespace,
            GameName = domainProject.GameName.FromDomain(),
            GamePath = domainProject.GamePath,
            ProjectSettings = domainProject.ProjectSettings.ToCliModel(),
            SourceFiles = [.. domainProject.SourceFiles.Select(SourceFile.FromDomain)],
            GameFiles = [.. domainProject.GameFiles.Select(GameFile.FromDomain)]
        };
    }
}

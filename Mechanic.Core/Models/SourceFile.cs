namespace Mechanic.Core.Models;
using Mechanic.Core.Project.Models.Json;

public class SourceFile : ProjectFile
{
    public required SourceFileType FileType { get; init; }
    public List<Guid> GameFileLinks { get; init; } = [];

    public SourceFiles ToJson() => new()
    {
        Id = this.Id.ToString(),
        Path = this.Path,
        FileType = this.FileType.ToJson(),
        DestinationPaths = [.. this.GameFileLinks.Select(path => path.ToString())]
    };

    public static SourceFile FromJsonProject(SourceFiles sourceFile) => new()
    {
        Id = Guid.Parse(sourceFile.Id),
        Path = sourceFile.Path,
        FileType = sourceFile.FileType.ToSourceFileType(),
        GameFileLinks = [.. sourceFile.DestinationPaths.Select(Guid.Parse)]
    };
}

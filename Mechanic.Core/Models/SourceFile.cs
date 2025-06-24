using Mechanic.Core.Project.Models.Json;

namespace Mechanic.Core.Models;

public class SourceFile : ProjectFile
{
    public required SourceFileType FileType { get; init; }
    public List<Guid> DestinationPaths { get; init; } = [];

    public static SourceFile FromJsonProject(SourceFiles sourceFile) => new()
    {
        Id = Guid.Parse(sourceFile.Id),
        Path = sourceFile.Path,
        FileType = sourceFile.FileType.ToSourceFileType(),
        DestinationPaths = sourceFile.DestinationPaths.Select(Guid.Parse).ToList()
    };
}
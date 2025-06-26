using Mechanic.Core.Models;

namespace Mechanic.CLI.Models;

public class SourceFile : CLI.Models.ProjectFile
{
    public required SourceFileType FileType { get; init; }
    public List<Guid> DestinationPaths { get; init; } = [];
    
    public Core.Models.SourceFile ToDomain()
    {
        return new Core.Models.SourceFile
        {
            Id = Id,
            Path = Path,
            FileType = FileType.ToDomain(),
            DestinationPaths = [.. DestinationPaths]
        };
    }
    
    public static SourceFile FromDomain(Core.Models.SourceFile domainSourceFile)
    {
        return new SourceFile
        {
            Id = domainSourceFile.Id,
            Path = domainSourceFile.Path,
            FileType = domainSourceFile.FileType.FromDomain(),
            DestinationPaths = [.. domainSourceFile.DestinationPaths]
        };
    }
}

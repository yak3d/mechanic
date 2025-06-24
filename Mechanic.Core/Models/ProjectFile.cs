namespace Mechanic.Core.Models;

using Project.Models.Json;

public class ProjectFile
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Path { get; init; }
}

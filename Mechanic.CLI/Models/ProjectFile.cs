namespace Mechanic.CLI.Models;
public class ProjectFile
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Path { get; init; }
}

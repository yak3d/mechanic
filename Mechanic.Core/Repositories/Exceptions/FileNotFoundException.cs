namespace Mechanic.Core.Repositories.Exceptions;

public record FileIdentifier
{
    public static FileIdentifier FromPath(string path) => new PathIdentifier(path);
    public static FileIdentifier FromId(Guid id) => new IdIdentifier(id);
}

public record PathIdentifier(string Path) : FileIdentifier;
public record IdIdentifier(Guid Id) : FileIdentifier;

public abstract class FileNotFoundException(FileIdentifier identifier) : Exception(identifier switch
{
    PathIdentifier(var path) => $"File not found at path: {path}",
    IdIdentifier(var id) => $"File not found with ID: {id}",
    _ => "File not found"
})
{
    public FileIdentifier Identifier { get; } = identifier;
}

public class ProjectSourceFileNotFoundException(FileIdentifier identifier) : FileNotFoundException(identifier);
public class ProjectGameFileNotFoundException(FileIdentifier identifier) : FileNotFoundException(identifier);

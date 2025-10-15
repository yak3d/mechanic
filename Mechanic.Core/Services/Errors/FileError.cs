namespace Mechanic.Core.Services.Errors;

public record FileError(string message, string filePath) : ProjectError(message);
public record UnauthorizedError(string filePath) : FileError("Attempted to access a file you don't have access to.", filePath);
public record FileNotFoundByPathError(string filePath) : FileError("Failed to find file", filePath);
public record FileNotByIdFoundError(Guid fileId) :  FileError("Failed to find file", fileId.ToString());

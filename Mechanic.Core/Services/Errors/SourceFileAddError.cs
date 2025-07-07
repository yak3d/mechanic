namespace Mechanic.Core.Services.Errors;

public record SourceFileAddError() : ProjectError("Failed to create source file");

public record LinkedFileDoesNotExistError : SourceFileAddError
{
    public Guid LinkedFileId { get; init; }
}

public record SourceFileDoesNotExistAtPathError(string Path) : ProjectError("Source file does not exist at the path");
public record SourceFileDoesNotExistWithIdError(Guid Id) : ProjectError("Source file does not exist with ID");
public record GameFileDoesNotExistAtPathError(string Path) : ProjectError("Game file does not exist at the path");
public record GameFileDoesNotExistWithIdError(Guid Id) : ProjectError("Game file does not exist with ID");

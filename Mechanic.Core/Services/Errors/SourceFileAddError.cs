namespace Mechanic.Core.Services.Errors;

public record SourceFileAddError : ProjectError;

public record LinkedFileDoesNotExistError : SourceFileAddError
{
    public Guid LinkedFileId { get; init; }
}

public record SourceFileDoesNotExistAtPathError(string Path) : ProjectError;
public record SourceFileDoesNotExistWithIdError(Guid Id) : ProjectError;
public record GameFileDoesNotExistAtPathError(string Path) : ProjectError;
public record GameFileDoesNotExistWithIdError(Guid Id) : ProjectError;

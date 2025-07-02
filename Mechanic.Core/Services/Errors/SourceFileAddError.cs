namespace Mechanic.Core.Services.Errors;

public class SourceFileAddError : ProjectError;

public class LinkedFileDoesNotExistError : SourceFileAddError
{
    public Guid LinkedFileId { get; init; }
}

public class SourceFileDoesNotExistAtPathError(string path) : ProjectError;
public class SourceFileDoesNotExistWithIdError(Guid id) : ProjectError;

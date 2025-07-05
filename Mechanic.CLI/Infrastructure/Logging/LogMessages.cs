using Mechanic.CLI.Models;
using Mechanic.Core.Services.Errors;
using Microsoft.Extensions.Logging;

namespace Mechanic.CLI.Infrastructure.Logging;

public static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Assuming game file type is {GameFileType} based on the extension {Extension}")]
    public static partial void AssumingGameFileType(this ILogger logger, GameFileType gameFileType, string extension);
    
    [LoggerMessage(Level = LogLevel.Warning, Message = "File type was set to Other, this may cause issues. If you think this was done incorrectly, check the file extension or set the type with the --type argument")]
    public static partial void FileTypeSetToOther(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Added source file {SourceFilePath} with id {SourceFileId}")]
    public static partial void AddedSourceFile(this ILogger logger, string sourceFilePath, Guid sourceFileId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Added source file {SourceFilePath} with id {SourceFileId} and linked it to game file {gameFilePath} ({GameFileLinkId})")]
    public static partial void AddedSourceFileWithLink(this ILogger logger, string sourceFilePath, Guid sourceFileId, string gameFilePath, Guid gameFileLinkId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to add source file {SourceFilePath} because the game file with id {GameFileId} could not be found.")]
    public static partial void FailedToAddSourceFileWithBadLink(this ILogger logger, string sourceFilePath, Guid gameFileId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Failed to add source file with error: {ErrorType}")]
    public static partial void FailedToAddSourceFileWithError(this ILogger logger, Type errorType);

    [LoggerMessage(Level = LogLevel.Error, Message = "Source file with path {SourceFilePath} already exists.")]
    public static partial void SourceFileAlreadyExists(this ILogger logger, string sourceFilePath);
    
    [LoggerMessage(Level = LogLevel.Information, Message = "Source file at path {SourceFilePath} removed from the project successfully. Note: It was not removed from the file system.")]
    public static partial void SourceFileRemovedFromProjectByPath(this ILogger logger, string sourceFilePath);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Source file at path {SourceFilePath} could not be found in the project.")]
    public static partial void SourceFileNotFoundByPathWhenRemoving(this ILogger logger, string sourceFilePath);
    
    [LoggerMessage(Level = LogLevel.Information, Message = "Source file with id {Id} removed from the project successfully. [bold]Note:[/] It was not removed from the file system.")]
    public static partial void SourceFileRemovedFromProjectById(this ILogger logger, Guid id);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Source file with id {Id} could not be found in the project.")]
    public static partial void SourceFileNotFoundByIdWhenRemoving(this ILogger logger, Guid id);
    
    [LoggerMessage(Level = LogLevel.Information, Message = "Game file at path {GameFilePath} removed from the project successfully. Note: It was not removed from the file system.")]
    public static partial void GameFileRemovedFromProjectByPath(this ILogger logger, string gameFilePath);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Game file at path {GameFilePath} could not be found in the project.")]
    public static partial void GameFileNotFoundByPathWhenRemoving(this ILogger logger, string gameFilePath);
    
    [LoggerMessage(Level = LogLevel.Information, Message = "Game file with id {Id} removed from the project successfully. [bold]Note:[/] It was not removed from the file system.")]
    public static partial void GameFileRemovedFromProjectById(this ILogger logger, Guid id);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Game file with id {Id} could not be found in the project.")]
    public static partial void GameFileNotFoundByIdWhenRemoving(this ILogger logger, Guid id);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Game file with path {GameFilePath} already exists.")]
    public static partial void GameFileAlreadyExists(this ILogger logger, string gameFilePath);
    
    [LoggerMessage(Level = LogLevel.Information, Message = "Added game file with path {GameFilePath} with id {GameFileId}.")]
    public static partial void AddedGameFileWithId(this ILogger logger, string gameFilePath, Guid gameFileId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = "Added game file {GameFilePath} with id {GameFileId} and linked it to source file {SourceFilePath} ({SourceFileId})")]
    public static partial void AddedGameFileWithLink(this ILogger logger, string gameFilePath, Guid gameFileId, string sourceFilePath, Guid sourceFileId);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create directory for settings with exception")]
    public static partial void FailedToCreateDirectoryWithException(this ILogger logger, Exception exception);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to write to settings file at path {SettingsPath} with exception")]
    public static partial void FailedToWriteToSettingsFile(this ILogger logger, string settingsPath, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to determine if {Exe} is on PATH, so executable must be set manually. Error was {OsError}")]
    public static partial void ExecutableIsOnPath(this ILogger logger, string exe, OSError osError);
}
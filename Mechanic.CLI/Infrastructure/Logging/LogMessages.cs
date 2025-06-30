using Mechanic.CLI.Models;
using Microsoft.Extensions.Logging;

namespace Mechanic.CLI.Infrastructure.Logging;

public static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Assuming game file type is {GameFileType} based on the extension {Extension}")]
    public static partial void AssumingGameFileType(this ILogger logger, GameFileType gameFileType, string extension);

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
}
namespace Mechanic.Core.Infrastructure.Logging;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Services.Errors;

public static partial class LogMessages
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Serializing project {ProjectId} to {Destination}")]
    public static partial void ProjectSerializing(this ILogger logger, string projectId, string destination);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Initializing project {ProjectId} to {Destination}")]
    public static partial void ProjectInitializing(this ILogger logger, string projectId, string destination);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Debug,
        Message = "Loading project from {ProjectJsonPath}")]
    public static partial void ProjectLoading(this ILogger logger, string projectJsonPath);

    [LoggerMessage(EventId = 1004,
        Level = LogLevel.Information,
        Message = "Loaded project with id {ProjectId}")]
    public static partial void ProjectLoaded(this ILogger logger, string projectId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Saving project with id {ProjectId} to {Destination}")]
    public static partial void ProjectSaving(this ILogger logger, string projectId, string destination);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Saved project with id {ProjectId} to {Destination}")]
    public static partial void ProjectSaved(this ILogger logger, string projectId, string destination);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Adding source file {SourcePath} with type {SourceType} to project {ProjectId}")]
    public static partial void ProjectAddingSourceFile(this ILogger logger, string sourcePath, string sourceType, string projectId);
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to serialize project from {ProjectPath}")]
    public static partial void ProjectDeserializingError(this ILogger logger, string projectPath, JsonException exception);
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to parse steam manifest")]
    public static partial void SteamManifestDeserializingError(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Found steam libraries in {SteamLibraries}")]
    public static partial void FoundSteamLibraries(this ILogger logger, string[] steamLibraries);

    [LoggerMessage(Level = LogLevel.Debug, Message = "No matching Steam games found.")]
    public static partial void NoSteamGamesFound(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to retrieve steam games due to error: {Error}")]
    public static partial void SteamGamesDueToError(this ILogger logger, SteamManifestError error);

    [LoggerMessage(Level = LogLevel.Information, Message = "Unlinking game file {GameFilePath} ({GameFileId}) from source file {SourceFilePath} ({SourceFileId}) since the game file is being removed from tracking.")]
    public static partial void UnlinkingGameFile(this ILogger logger, string gameFilePath, Guid gameFileId, string sourceFilePath, Guid sourceFileId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Exception thrown while checking if executable {ExecutableName} exists in {Path}")]
    public static partial void ExceptionThrownWhenCheckingForExeInPath(this ILogger logger, string executableName, string path, Exception exception);

    [LoggerMessage(Level = LogLevel.Debug, Message = "PATH variable contents: {PathVar}")]
    public static partial void PathVariableContents(this ILogger logger, string pathVar);

    [LoggerMessage(Level = LogLevel.Warning, Message = "PATH variable was empty, so assuming executable is not in PATH")]
    public static partial void PathVariableIsEmpty(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to get last modified time for file {FilePath} with exception")]
    public static partial void FailedToFindLastModifiedTime(this ILogger logger, string filePath, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to serialize Pyro project to XML: ")]
    public static partial void FailedToSerializePyroProject(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Writing project file to {FilePath}")]
    public static partial void WritingProjectFile(this ILogger logger, string filePath);

    [LoggerMessage(Level = LogLevel.Debug, Message = "FileWatcher registered subscriber with class {clazz}: {subscriber}")]
    public static partial void FileWatcherRegisteredSubscriber(this ILogger logger, string clazz, object subscriber);

    [LoggerMessage(Level = LogLevel.Debug, Message = "FileEventHandler registered subscriber with class {clazz}: {subscriber}")]
    public static partial void FileEventHandlerRegisteredSubscriber(this ILogger logger, string clazz, object subscriber);

    [LoggerMessage(Level = LogLevel.Error, Message = "There was an error when watching a file")]
    public static partial void ErrorWatchingFile(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Watching source files in {Path}")]
    public static partial void WatchingSourceFile(this ILogger logger, string path);

    [LoggerMessage(Level = LogLevel.Information, Message = "Watching game files in {Path}")]
    public static partial void WatchingGameFile(this ILogger logger, string path);

    [LoggerMessage(Level = LogLevel.Information, Message = "File watcher detected {EventType} for file {FilePath}")]
    public static partial void FileWatcherDetected(this ILogger logger, string eventType, string filePath);

    [LoggerMessage(Level = LogLevel.Information, Message = "Received file event for [{EventType}] {FilePath}")]
    public static partial void ReceivedFileEvent(this ILogger logger, string eventType, string filePath);
}

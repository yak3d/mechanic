namespace Mechanic.Core.Infrastructure.Logging;

using System.Text.Json;
using Microsoft.Extensions.Logging;

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
}

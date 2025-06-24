namespace Mechanic.Core.Infrastructure.Logging;

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
}

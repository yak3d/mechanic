using Mechanic.CLI.Models;
using Microsoft.Extensions.Logging;

namespace Mechanic.CLI.Infrastructure.Logging;

public static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Assuming game file type is {GameFileType} based on the extension {Extension}")]
    public static partial void AssumingGameFileType(this ILogger logger, GameFileType gameFileType, string extension);
}
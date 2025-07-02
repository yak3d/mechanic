using Serilog;
using Serilog.Configuration;

namespace Mechanic.CLI.Infrastructure.Logging;

public static class SerilogExtensions
{
    public static LoggerConfiguration SpectreConsole(
        this LoggerSinkConfiguration sinkConfiguration,
        IFormatProvider? formatProvider = null,
        string outputTemplate = "[{Level:u3}] {Message:lj}{NewLine}")
    {
        return sinkConfiguration.Sink(new SpectreConsoleSink(formatProvider, outputTemplate));
    }
}
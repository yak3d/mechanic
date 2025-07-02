using Serilog.Core;
using Serilog.Events;
using Spectre.Console;

namespace Mechanic.CLI.Infrastructure.Logging;

public class SpectreConsoleSink : ILogEventSink
{
    private readonly IFormatProvider? _formatProvider;
    private readonly string _outputTemplate;

    public SpectreConsoleSink(IFormatProvider? formatProvider = null, 
        string outputTemplate = "<{Level:u3}> {Message:lj}{NewLine}")
    {
        _formatProvider = formatProvider;
        _outputTemplate = outputTemplate;
    }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(_formatProvider);
        var level = GetLevelMarkup(logEvent.Level);
        
        var formattedMessage = $"[{level}] {message}[/]";
        
        try
        {
            AnsiConsole.MarkupLine(formattedMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SINK ERROR] {ex.Message}");
            AnsiConsole.WriteLine($"[{logEvent.Level:u3}] {message}");
        }
    }

    private static string GetLevelMarkup(LogEventLevel level) => level switch
    {
        LogEventLevel.Verbose => "grey",
        LogEventLevel.Debug => "grey",
        LogEventLevel.Information => "blue",
        LogEventLevel.Warning => "yellow",
        LogEventLevel.Error => "red",
        LogEventLevel.Fatal => "maroon",
        _ => "white"
    };
}
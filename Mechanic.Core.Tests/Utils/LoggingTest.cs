using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Mechanic.Core.Tests.Utils;

public abstract class LoggingTest : IDisposable
{
    protected readonly ILoggerFactory LoggerFactory;

    protected LoggingTest(ITestOutputHelper output)
    {
        LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new XUnitLoggerProvider(output, new XUnitLoggerOptions()))
                .SetMinimumLevel(LogLevel.Debug);
        });
    }

    /// <summary>
    /// Creates a logger for the specified type
    /// </summary>
    protected ILogger<T> CreateLogger<T>()
    {
        return LoggerFactory.CreateLogger<T>();
    }

    /// <summary>
    /// Creates a logger with the specified category name
    /// </summary>
    protected ILogger CreateLogger(string categoryName)
    {
        return LoggerFactory.CreateLogger(categoryName);
    }

    /// <summary>
    /// Creates a mock of the specified interface
    /// </summary>
    // protected Mock<T> CreateMock<T>() where T : class
    // {
    //     return new Mock<T>();
    // }
    //
    public virtual void Dispose()
    {
        LoggerFactory?.Dispose();
    }
}

using Microsoft.Extensions.Logging;

namespace ragd;

public class RagLogger : ILogger
{
    public bool IsQuiet { get; set; }
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;
    public bool IsEnabled(LogLevel logLevel) => !IsQuiet;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        Console.WriteLine(formatter(state, exception));
    }
}
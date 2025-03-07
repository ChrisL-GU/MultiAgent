using Microsoft.Extensions.Logging;

using System;

namespace MultiAgent.Agents;

public class InMemoryLogger : ILogger
{
    private readonly List<string> logEntries = [];

    public IEnumerable<string> LogEntries => logEntries;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => null;
    
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        logEntries.Add(formatter(state, exception));
    }
}
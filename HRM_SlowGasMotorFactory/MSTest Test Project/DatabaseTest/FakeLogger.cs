using Microsoft.Extensions.Logging;

namespace HRM_SlowGasMotorFactory_TestsMSTest.DatabaseTest;

public class FakeLogger<T> : ILogger<T>
{
    public List<string> LogMessages { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        LogMessages.Add($"[{logLevel}] {message}");
    }
}
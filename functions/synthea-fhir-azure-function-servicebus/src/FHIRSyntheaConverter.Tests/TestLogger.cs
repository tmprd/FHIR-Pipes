using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Conceptual.FHIRSyntheaConverter.Tests
{
    public class TestLogger<T> : ILogger<T>
    {
        public List<string> LogMessages { get; } = new List<string>();

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            LogMessages.Add(message);
        }
    }
}
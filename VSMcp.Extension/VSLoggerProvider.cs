using System;
using Microsoft.Extensions.Logging;

namespace VSMcp.Extension
{
    /// <summary>
    /// Logger provider that logs to the Visual Studio output window.
    /// </summary>
    public class VSLoggerProvider : ILoggerProvider
    {
        private readonly VSOutputWindowPane _outputPane;
        
        public VSLoggerProvider(VSOutputWindowPane outputPane)
        {
            this._outputPane = outputPane;
        }
        
        public ILogger CreateLogger(string categoryName)
        {
            return new VSLogger(categoryName, _outputPane);
        }
        
        public void Dispose()
        {
            // Nothing to dispose
        }
        
        /// <summary>
        /// Logger implementation that logs to the Visual Studio output window.
        /// </summary>
        private class VSLogger : ILogger
        {
            private readonly string _categoryName;
            private readonly VSOutputWindowPane _outputPane;
            
            public VSLogger(string categoryName, VSOutputWindowPane outputPane)
            {
                this._categoryName = categoryName;
                this._outputPane = outputPane;
            }
            
            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
            
            public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;
            
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }
                
                var message = formatter(state, exception);
                
                var logLevelString = logLevel switch
                {
                    LogLevel.Trace => "TRACE",
                    LogLevel.Debug => "DEBUG",
                    LogLevel.Information => "INFO ",
                    LogLevel.Warning => "WARN ",
                    LogLevel.Error => "ERROR",
                    LogLevel.Critical => "CRIT ",
                    _ => "     "
                };
                
                _outputPane?.WriteLineAsync($"[{logLevelString}] [{_categoryName}] {message}").Wait();
                
                if (exception != null)
                {
                    _outputPane?.WriteLineAsync($"Exception: {exception}").Wait();
                }
            }
            
            private class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new NullScope();
                
                private NullScope() { }
                
                public void Dispose() { }
            }
        }
    }
}
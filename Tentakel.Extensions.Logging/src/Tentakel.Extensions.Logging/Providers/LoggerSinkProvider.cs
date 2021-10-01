using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Tentakel.Extensions.Logging.Loggers;

namespace Tentakel.Extensions.Logging.Providers
{
    public class LoggerSinkProvider : ILoggerProvider, ISupportExternalScope
    {
        private IExternalScopeProvider _scopeProvider;
        private readonly BackgroundWorker _backgroundWorker;

        public LoggerSinkProvider()
        {
            this._backgroundWorker = new BackgroundWorker(new Logger(this, "Tentakel.Logger"));
            this._backgroundWorker.Start();
        }

        #region Add/Update/Remove logger sink

        private readonly ConcurrentDictionary<string, ILoggerSink> _loggerSinks = new();

        public void AddOrUpdateLoggerSink(ILoggerSink loggerSink)
        {
            this._loggerSinks[loggerSink.Name] = loggerSink;
        }

        public void AddOrUpdateLoggerSinks(IEnumerable<ILoggerSink> loggerSinks)
        {
            foreach (var loggerSink in loggerSinks)
            {
                this.AddOrUpdateLoggerSink(loggerSink);
            }
        }

        public bool RemoveLoggerSink(string name)
        {
            return this._loggerSinks.TryRemove(name, out _);
        }

        public void ClearLoggerSinks() => this._loggerSinks.Clear();

        public IEnumerable<ILoggerSink> LoggerSinks => this._loggerSinks.Values;

        #endregion


        #region ISupportExternalScope

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            this._scopeProvider = scopeProvider;
        }

        #endregion

        #region ILoggerProvider

        public ILogger CreateLogger(string categoryName)
        {
            return new Logger(this, categoryName);
        }

        #endregion

        #region public member IsEnabled/Log

        public bool IsEnabled(LogLevel logLevel)
        {
            return this._loggerSinks.Any(x => x.Value.IsEnabled(logLevel));
        }

        public void Log(LogEntry logEntry)
        {
            logEntry.StackTrace ??= new StackTrace();

            this.ScopeProvider?.ForEachScope((value, loggingProps) =>
            {
                var scope = new LogScopeInfo();
                logEntry.Scopes ??= new List<LogScopeInfo>();
                logEntry.Scopes.Add(scope);

                switch (value)
                {
                    case string:
                        scope.Text = value.ToString();
                        break;
                    case IEnumerable<KeyValuePair<string, object>> props:
                        {
                            scope.Text = loggingProps?.ToString();
                            scope.Properties ??= new Dictionary<string, object>();

                            foreach (var (k, v) in props)
                            {
                                scope.Properties[k] = v;
                            }

                            break;
                        }
                }
            }, logEntry.State);


            this._backgroundWorker.AddBackgroundAction(() =>
            {
                var loggerSinks = this._loggerSinks.Values
                    .Where(x => x.Categories.Contains(logEntry.SourceCategory) &&
                                x.IsEnabled(logEntry.LogLevel)).ToList();

                foreach (var loggerSink in loggerSinks)
                {
                    if (string.IsNullOrEmpty(logEntry.Source) &&
                        SourceResolver.TryFindStackTraceSource(loggerSink.GetType(), logEntry.StackTrace, out var source))
                    {
                        logEntry.Source = source;
                    }

                    logEntry.LoggerSinkName = loggerSink.Name;
                    logEntry.LoggerSinkType = loggerSink.GetType();

                    loggerSink.Log(logEntry);
                }

                logEntry.LoggerSinkName = null;
                logEntry.LoggerSinkType = null;
            });
        }

        #endregion

        #region internal members

        internal IExternalScopeProvider ScopeProvider
        {
            get { return this._scopeProvider ??= new LoggerExternalScopeProvider(); }
        }

        #endregion

        #region IDisposable

        ~LoggerSinkProvider()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (this.IsDisposed) return;
            this.IsDisposed = true;

            try
            {
                this._backgroundWorker.Dispose();
            }
            catch 
            { 
                //
            }

            if (!disposing) return;
            GC.SuppressFinalize(this);
        }

        public bool IsDisposed { get; private set; }

        #endregion
    }
}
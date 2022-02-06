using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Tentakel.Extensions.Configuration;
using Tentakel.Extensions.Logging.Loggers;

namespace Tentakel.Extensions.Logging.Providers
{
    [ProviderAlias("Tentakel")]
    public class LoggerSinkProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly IConfiguredTypesOptionsMonitor<ILoggerSink> _options;
        private IExternalScopeProvider _scopeProvider;
        private readonly BackgroundWorker _backgroundWorker;
        private readonly Dictionary<string, ILoggerSink> _loggerSinks = new(StringComparer.Ordinal);


        public LoggerSinkProvider(IConfiguredTypesOptionsMonitor<ILoggerSink> options)
        {
            this._options = options ?? throw new ArgumentNullException(nameof(options));
            
            this._options.OnChange((sink, name, _) =>
            {
                if (this.ConfigurationName != name) return;
                this.AddOrUpdateLoggerSink(sink);
            });
           
            this.AddOrUpdateLoggerSinks(options.GetAll());

            this._backgroundWorker = new BackgroundWorker(new Logger(this, "Tentakel.Logger"));
            this._backgroundWorker.Start();
        }

        private string _configurationName = string.Empty;
        public string ConfigurationName
        {
            get => this._configurationName;
            set
            {
                this._configurationName = value;

                this.ClearLoggerSinks();
                this.AddOrUpdateLoggerSinks(this._options.GetAll(this._configurationName));
            }
        }

        #region Add/Update/Remove logger sink

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
            return this._loggerSinks.Remove(name);
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

        public bool WaitOne(TimeSpan timeout)
        {
            return this._backgroundWorker.WaitOne(timeout);
        }

        public bool WaitOne(int millisecondsTimeout)
        {
            return this._backgroundWorker.WaitOne(millisecondsTimeout);
        }

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

        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            if (this.IsDisposed) return;
            this.IsDisposed = true;

            try
            {
                this._backgroundWorker.Dispose();
            }
            catch 
            {
                // Swallow exceptions on dispose.
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Tentakel.Extensions.Configuration;
using Tentakel.Extensions.Logging.BackgroundWork;
using Tentakel.Extensions.Logging.Loggers;

namespace Tentakel.Extensions.Logging.Providers
{
    [ProviderAlias("Tentakel")]
    public class LoggerSinkProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly IConfiguredTypesOptionsMonitor<ILoggerSink>? _options;
        private IExternalScopeProvider? _scopeProvider;
        private readonly BackgroundWorker _backgroundWorker;
        private readonly Dictionary<string, ILoggerSink> _loggerSinks = new(StringComparer.Ordinal);
        private readonly AutoResetEvent _wait = new(false);

        public LoggerSinkProvider()
        {
            this._backgroundWorker = new(this);
            this._backgroundWorker.Start();
        }

        public LoggerSinkProvider(IConfiguredTypesOptionsMonitor<ILoggerSink> options)
        {
            this._options = options ?? throw new ArgumentNullException(nameof(options));

            this._options.OnChange((sink, name, _) =>
            {
                //var result = this.WaitOne(TimeSpan.FromSeconds(3));
                //if (result > 0)
                //{
                //    Log($"WaitOne: {result}");
                //}

                if (this.ConfigurationName != name) return;
                
                if (sink == null)
                {
                    this.RemoveLoggerSink(name);
                    return;
                }

                this.AddOrUpdateLoggerSink(sink);
            });

            this.AddOrUpdateLoggerSinks(options.GetAll());

            this._backgroundWorker = new(this);
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

                if (this._options == null) return;
                this.AddOrUpdateLoggerSinks(this._options.GetAll(this._configurationName));
            }
        }

        #region Add/Update/Remove logger sink

        public void AddOrUpdateLoggerSink(ILoggerSink loggerSink)
        {
            if (this._loggerSinks.TryGetValue(loggerSink.Name, out var item))
            {
                if (item == loggerSink) return;
                item.Dispose();
            }

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
            if (this._loggerSinks.TryGetValue(name, out var item))
            {
                item.Dispose();
            }

            return this._loggerSinks.Remove(name);
        }

        public void ClearLoggerSinks()
        {
            foreach (var loggerSink in this._loggerSinks.Values)
            {
                loggerSink.Dispose();
            }

            this._loggerSinks.Clear();
        }

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
            this.ScopeProvider.ForEachScope((value, loggingProps) =>
            {
                var scope = new LogScopeInfo();
                logEntry.Scopes.Add(scope);

                switch (value)
                {
                    case string:
                        scope.Text = value.ToString();
                        break;
                    case IEnumerable<KeyValuePair<string, object>> props:
                        {
                            scope.Text = loggingProps?.ToString();

                            foreach (var (k, v) in props)
                            {
                                scope.Properties[k] = v;
                            }

                            break;
                        }
                }
            }, logEntry.State);

            this._backgroundWorker.AddLogEntry(logEntry);
        }

        public int WaitOne(int millisecondsTimeout)
        {
            return this.WaitOne(TimeSpan.FromMilliseconds(millisecondsTimeout));
        }

        public int WaitOne(TimeSpan timeout)
        {
            lock (this._backgroundWorker)
            {
                if (this._backgroundWorker.UndoneLogs == 0) return 0;

                this._wait.Reset();
                return this._wait.WaitOne(timeout) ? 0 : this._backgroundWorker.UndoneLogs;
            }
        }

        public bool ResolveSource { get; set; } = true;


        #endregion

        #region internal members

        internal IExternalScopeProvider ScopeProvider
        {
            get { return this._scopeProvider ??= new LoggerExternalScopeProvider(); }
        }

        internal void InternalLog(LogEntry logEntry)
        {
            var loggerSinks = this._loggerSinks.Values
                .Where(x => (x.IsEnabled(logEntry.LogLevel) &&
                            (
                                 x.Categories.Any(c => c.EndsWith("*") && logEntry.LogCategory.StartsWith(c.TrimEnd('*'))) ||
                                 x.Categories.Any(c => c.StartsWith("*") && logEntry.LogCategory.EndsWith(c.TrimStart('*'))) ||
                                 x.Categories.Any(c => c.StartsWith("*") && c.EndsWith("*") && logEntry.LogCategory.Contains(c.Trim('*'))) ||
                                 x.Categories.Contains(logEntry.LogCategory) ||
                                 x.Categories.Contains("*")
                             )
                            )).ToList();

            foreach (var loggerSink in loggerSinks)
            {
                logEntry.LoggerSinkName = loggerSink.Name;
                logEntry.LoggerSinkType = loggerSink.GetType();

                try
                {
                    loggerSink.Log(logEntry);
                }
                catch (Exception ex)
                {
                    Log($"SinkType: {logEntry.LoggerSinkType}, SinkName: {logEntry.LoggerSinkName}, Message: {ex.Message}");
                }
            }

            logEntry.LoggerSinkName = null;
            logEntry.LoggerSinkType = null;
        }

        internal void BackgroundStackIsEmpty()
        {
            this._wait.Set();
        }

        #endregion


        private static void Log(string message)
        {
            File.AppendAllText(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "LoggerSinkProvider.log"), 
                $"{DateTime.Now:yyyy.MM.dd HH:mm:ss} {message}{Environment.NewLine}");
        }

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
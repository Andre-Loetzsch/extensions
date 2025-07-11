using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Oleander.Extensions.Logging.Providers;

namespace Oleander.Extensions.Logging.BackgroundWork
{
    internal class BackgroundWorker(LoggerSinkProvider provider) : IDisposable
    {
        private readonly ILogger _logger = provider.CreateLogger("Oleander.Logger");
        private Thread? _backgroundThread;
        private readonly ManualResetEvent _wait = new(false);
        private bool _logEntryBackgroundStackIsEmpty = true;
        private readonly LogEntryStackManager _logEntryStackManager = new();

        public bool IsRunning { get; private set; }

        public int UndoneLogs
        {
            get
            {
                lock (this._logEntryStackManager)
                {
                    return this._logEntryStackManager.AddStack.Length + this._logEntryStackManager.GetStack.Length;
                }
            }
        }

        public void AddLogEntry(LogEntry? logEntry)
        {
            if (this.IsDisposed) return;
            if (logEntry == null) return;

            lock (this._logEntryStackManager)
            {
                this._logEntryStackManager.AddLogEntry(logEntry);
                if (!this._logEntryBackgroundStackIsEmpty) return;
                this._wait.Set();
            }
        }

        public void Start()
        {
            if (this.IsDisposed) throw new ObjectDisposedException("BackgroundWorker", SR.BackgroundWorkerHasBeenDisposed);
            if (this.IsRunning) return;

            this._backgroundThread = new(this.Run)
            {
                Name = "TracingBW",
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            };

            this._backgroundThread.Start();
        }
        public void Stop()
        {
            if (!this.IsRunning) return;
            this.IsRunning = false;
            this._wait.Set();
        }

        private void Run()
        {
            if (this.IsRunning) return;
            this.IsRunning = true;

            this._wait.WaitOne();

            while (this.IsRunning)
            {
                var next = this.GetNextLogEntry();
                if (this.IsDisposed) break;

                if (next != null)
                {
                    try
                    {
                        provider.InternalLog(next);
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError(ex, "Error occurred during execute internal log: {next}!", next);
                    }
                }
                else
                {
                    lock (this._logEntryStackManager)
                    {
                        this._logEntryBackgroundStackIsEmpty = true;
                        provider.BackgroundStackIsEmpty();
                    }
                    
                    this._wait.Reset();
                    this._wait.WaitOne();

                    lock (this._logEntryStackManager)
                    {
                        this._logEntryBackgroundStackIsEmpty = false;
                    }
                }
            }
        }

        private LogEntry? GetNextLogEntry()
        {
            var next = this._logEntryStackManager.GetLogEntry();

            if (next != null) return next;

            lock (this._logEntryStackManager)
            {
                this._logEntryStackManager.ChangPointer();
                next = this._logEntryStackManager.GetLogEntry();
            }

            Thread.Sleep(150);
            return next;
        }

        #region IDisposable
        public bool IsDisposed { get; private set; }

        ~BackgroundWorker()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (this.IsDisposed) return;
            this.IsDisposed = true;

            this.Stop();

            this._logEntryStackManager.Dispose();

            if (!this._wait.SafeWaitHandle.IsClosed)
            {
                this._wait.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
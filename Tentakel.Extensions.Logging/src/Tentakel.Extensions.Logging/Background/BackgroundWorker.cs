using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Tentakel.Extensions.Logging.Providers;

namespace Tentakel.Extensions.Logging.Background
{
    internal class BackgroundWorker : IDisposable
    {
        private LoggerSinkProvider _provider;
        private readonly ILogger _logger;
        private Thread BackgroundThread;
        private readonly ManualResetEvent _wait;
        private bool _logEntryBackgroundStackIsEmpty = true;
        private LogEntryStackManager _logEntryStackManager = new();
        public BackgroundWorker(LoggerSinkProvider provider)
        {
            this._provider = provider;
            this._logger = provider.CreateLogger("Tentakel.Logger");
            this._wait = new ManualResetEvent(false);
        }

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

        public void AddLogEntry(LogEntry logEntry)
        {
            if (this.IsDisposed) return;
            if (logEntry == null) return;

            lock (this._logEntryStackManager)
            {
                this._logEntryStackManager.AddLogEntry(logEntry);
                if (!this._logEntryBackgroundStackIsEmpty) return;
                this._wait.Reset();
                this._wait.Set();
            }
        }

        public void Start()
        {
            if (this.IsDisposed) throw new ObjectDisposedException("BackgroundWorker", SR.BackgroundWorkerHasBeenDisposed);
            if (this.IsRunning) return;

            this.BackgroundThread = new Thread(this.Run)
            {
                Name = "TracingBW",
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            };

            this.BackgroundThread.Start();
        }
        public void Stop()
        {
            if (this.IsDisposed) return;
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

                if (next != null)
                {
                    try
                    {
                        this._provider.InternalLog(next);
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
                        this._provider.BackgroundStackIsEmpty();
                    }

                    this._wait.Reset();
                    this._wait.WaitOne();

                    //Thread.Sleep(10);

                    lock (this._logEntryStackManager)
                    {
                        this._logEntryBackgroundStackIsEmpty = false;
                    }
                }
            }
        }

        private LogEntry GetNextLogEntry()
        {
            var next = this._logEntryStackManager.GetLogEntry();

            if (next == null)
            {
                lock (this._logEntryStackManager)
                {
                    this._logEntryStackManager.ChangPointer();
                    next = this._logEntryStackManager.GetLogEntry();
                }
            }

            return next;
        }

        #region IDisposable
        public bool IsDisposed { get; private set; }

        ~BackgroundWorker()
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
            this.Stop();
            this.IsDisposed = true;

            this._logEntryStackManager.Dispose();

            if (this._wait?.SafeWaitHandle != null && !this._wait.SafeWaitHandle.IsClosed)
            {
                this._wait.Dispose();
            }

            if (disposing) GC.SuppressFinalize(this);
        }

        #endregion
    }
}
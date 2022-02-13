using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Tentakel.Extensions.Logging.Providers;

namespace Tentakel.Extensions.Logging
{
    internal class BackgroundWorker : IDisposable
    {
        private LoggerSinkProvider _provider;
        private readonly ILogger _logger;

        private Thread BackgroundThread;

        private readonly List<LogEntry> _logEntryCache = new();
        private readonly List<LogEntry> _logEntryBackgroundStack = new();

        private readonly ManualResetEvent _wait;

        private bool _logEntryBackgroundStackIsEmpty = true;

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
                lock (this._logEntryCache)
                {
                    return this._logEntryCache.Count + this._logEntryBackgroundStack.Count;
                }
            }
        }



        public void AddLogEntry(LogEntry logEntry)
        {
            if (this.IsDisposed) return;
            if (logEntry == null) return;

            lock (this._logEntryCache)
            {
                this._logEntryCache.Add(logEntry);
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
                    lock (this._logEntryCache)
                    {
                        this._logEntryBackgroundStackIsEmpty = true;
                        this._provider.BackgroundStackIsEmpty();
                    }

                    this._wait.Reset();
                    this._wait.WaitOne();

                    Thread.Sleep(10);

                    lock (this._logEntryCache)
                    {
                        this._logEntryBackgroundStackIsEmpty = false;
                    }
                }
            }
        }

        private int _index;

        private LogEntry GetNextLogEntry()
        {
            this._index++;

            if (this._index >=  this._logEntryBackgroundStack.Count)
            {
                this._index = 0;

                lock (this._logEntryCache)
                {
                    Debug.WriteLine($"_logEntryCache.Count: {_logEntryCache.Count}");

                    this._logEntryBackgroundStack.Clear();
                    this._logEntryBackgroundStack.AddRange(this._logEntryCache);
                    this._logEntryCache.Clear();

                    if (this._logEntryBackgroundStack.Count == 0) return null;

                }
            }

            //next = this._logEntryBackgroundStack.FirstOrDefault();
            //if (next != null) this._logEntryBackgroundStack.Remove(next);

            return this._logEntryBackgroundStack[this._index];

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

            this._logEntryCache.Clear();
            this._logEntryBackgroundStack.Clear();

            if (this._wait?.SafeWaitHandle != null && !this._wait.SafeWaitHandle.IsClosed)
            {
                this._wait.Dispose();
            }


            if (disposing) GC.SuppressFinalize(this);
        }

        #endregion
    }
}
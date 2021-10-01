using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Tentakel.Extensions.Logging
{
    internal class BackgroundWorker : IDisposable
    {
        private readonly ILogger _innerLogger;

        private readonly List<BackgroundAction> _backgroundActions;
        private readonly ManualResetEvent _wait;
        private readonly List<AutoResetEvent> _waitHandles = new();

        private string _currentActionTrace;
        private int _disposedBackgroundActions;

        private class BackgroundAction
        {
            internal static long InstanceCounter;
            private readonly long _instanceCounter;

            public BackgroundAction(Action action, Func<bool> isActive)
            {
                Interlocked.Decrement(ref InstanceCounter);
                this._instanceCounter = InstanceCounter;
                this.Action = action;
                this.IsActive = isActive;
            }

            public readonly Action Action;
            public readonly Func<bool> IsActive;

            public override string ToString()
            {
                return $"{this.Action.Target}.{this.Action.Method} {this._instanceCounter}";
            }
        }

        public BackgroundWorker(ILogger innerLogger)
        {
            this._innerLogger = innerLogger;
            this._backgroundActions = new List<BackgroundAction>();
            this._wait = new ManualResetEvent(false);
        }

        public Thread BackgroundThread { get; private set; }
        public bool IsRunning { get; private set; }

        public void AddBackgroundAction(Action action, Func<bool> isActive = null)
        {
            if (this.IsDisposed) return;
            if (action == null) throw new ArgumentNullException(nameof(action));

            lock (this._backgroundActions)
            {
                if (isActive == null) isActive = () => true;
                this._backgroundActions.Add(new BackgroundAction(action, isActive));
                this._wait.Set();
            }
        }

        public void InsertBackgroundAction(int index, Action action, Func<bool> isActive = null)
        {
            if (this.IsDisposed) return;
            if (action == null) throw new ArgumentNullException(nameof(action));

            lock (this._backgroundActions)
            {
                if (this.IsDisposed) return;
                if (isActive == null) isActive = () => true;
                this._backgroundActions.Insert(index, new BackgroundAction(action, isActive));
            }

            this._wait.Set();
        }

        public bool WaitOne(int millisecondsTimeout)
        {
            return this.WaitOne(TimeSpan.FromMilliseconds(millisecondsTimeout));
        }

        public bool WaitOne(TimeSpan timeout)
        {
            if (this.IsDisposed) return this._disposedBackgroundActions == 0;

            if (this.BackgroundThread == Thread.CurrentThread)
            {
                throw new InvalidOperationException(SR.PossibleDeadlockDetected);
            }

            var wh = new AutoResetEvent(false);
            var wh1 = wh;
            var wh2 = wh;

            lock (this._waitHandles)
            {
                this._waitHandles.Add(wh);
            }

            this.AddBackgroundAction(() => wh1.Set(), () =>
            {
                lock (this._waitHandles)
                {
                    return this._waitHandles.Contains(wh2);
                }
            });

            var result = wh.WaitOne(timeout);

            lock (this._waitHandles)
            {
                this._waitHandles.Remove(wh);
            }

            wh.Dispose();

            if (result) return true;

            if (this._innerLogger.IsEnabled(LogLevel.Debug))
            {
                this._innerLogger.LogDebug("WaitOne failed! Current action: {StackTrace}", this.StackTrace());
            }

            if (this.IsRunning) return false;
            throw new InvalidOperationException(SR.BackgroundWorkerIsNotRunning);
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

        public string StackTrace()
        {
            lock (this._backgroundActions)
            {
                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(this._currentActionTrace))
                {
                    sb.AppendLine($"{this._currentActionTrace}*");
                }

                foreach (var action in this._backgroundActions)
                {
                    sb.AppendLine(action.ToString());
                }

                return sb.ToString();
            }
        }

        private void Run()
        {
            if (this.IsRunning) return;
            this.IsRunning = true;

            while (this.IsRunning)
            {
                var nextAction = this.GetNextAction();

                if (nextAction != null)
                {
                    try
                    {
                        nextAction.Action();
                    }
                    catch (Exception ex)
                    {
                        this._innerLogger.LogError(ex, "Error occurred during execute background action: {nextAction}!", nextAction);
                    }
                }
                else
                {
                    this._wait.Reset();
                    this._wait.WaitOne();
                }
            }
        }

        private BackgroundAction GetNextAction()
        {
            lock (this._backgroundActions)
            {
                BackgroundAction nextAction;

                do
                {
                    nextAction = this._backgroundActions.FirstOrDefault();
                    if (nextAction != null) this._backgroundActions.Remove(nextAction);

                } while (nextAction != null && !nextAction.IsActive());

                this._currentActionTrace ??= string.Empty;

                this._currentActionTrace = nextAction == null ?
                    string.Concat(this._currentActionTrace, " (null)") :
                    nextAction.ToString();

                return nextAction;
            }
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

            this._disposedBackgroundActions = this._backgroundActions.Count;
            this._backgroundActions.Clear();

            if (this._wait?.SafeWaitHandle != null && !this._wait.SafeWaitHandle.IsClosed)
            {
                this._wait.Dispose();
            }

            BackgroundAction.InstanceCounter = 0;
            if (disposing) GC.SuppressFinalize(this);
        }

        #endregion
    }
}
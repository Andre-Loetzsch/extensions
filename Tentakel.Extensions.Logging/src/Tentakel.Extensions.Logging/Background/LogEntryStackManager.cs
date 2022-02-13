using System;
using System.Diagnostics;

namespace Tentakel.Extensions.Logging.Background
{
    internal class LogEntryStackManager : IDisposable
    {
        private LogEntryStack[] _stacks = new LogEntryStack[2];
        private LogEntryStackPointer _pointer  = new ();

        public LogEntryStackManager()
        {
            this._stacks[0] = new();
            this._stacks[1] = new();
        }

        public LogEntryStack AddStack
        {
            get { return this._stacks[this._pointer.AddPointer]; }
        }

        public LogEntryStack GetStack
        {
            get { return this._stacks[this._pointer.GetPointer]; }
        }

        public void AddLogEntry(LogEntry logEntry)
        {
            this.AddStack.AddLogEntry(logEntry);
        }

        public LogEntry GetLogEntry()
        {
            return this.GetStack.GetLogEntry();

        }


        public void ChangPointer()
        {
            this._pointer.Change();
            Debug.WriteLine($"GetStack: { GetStack.Length}");
        }

        #region IDisposable 

        private bool _disposed = false;
        ~LogEntryStackManager()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (this._disposed) return;
            this._disposed = true;
            this._stacks = null;

            if (disposing) GC.SuppressFinalize(this);
        }

        #endregion
    }
}
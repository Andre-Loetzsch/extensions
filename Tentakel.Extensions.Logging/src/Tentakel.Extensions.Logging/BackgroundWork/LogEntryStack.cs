using System;
using System.Collections.Generic;

namespace Tentakel.Extensions.Logging.BackgroundWork
{
    internal class LogEntryStack : IDisposable
    {
        private readonly List<LogEntry> _stack = new();
        private int _getIndex;

        public int Length => this._stack.Count;

        public void AddLogEntry(LogEntry logEntry)
        {
            this._stack.Add(logEntry);
            this._getIndex = 0;
        }

        public LogEntry GetLogEntry()
        {
            if (this._getIndex >= this._stack.Count)
            {
                this._getIndex = 0;
                this._stack.Clear();
                return null;
            }

            var value = this._stack[this._getIndex];
            this._stack[this._getIndex] = null;
            this._getIndex++;

            return value;
        }

        #region IDisposable 

        private bool _disposed;
        ~LogEntryStack()
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
            this._stack.Clear();

            if (disposing) GC.SuppressFinalize(this);
        }

        #endregion
    }
}
using System;
using System.Diagnostics;

namespace Tentakel.Extensions.Logging.BackgroundWork
{
    internal class LogEntryStackManager : IDisposable
    {
        private readonly LogEntryStack[] _stacks = new LogEntryStack[2];
        private readonly LogEntryStackPointer _pointer = new();

        public LogEntryStackManager()
        {
            this._stacks[0] = new();
            this._stacks[1] = new();
        }

        public LogEntryStack AddStack => this._stacks[this._pointer.AddPointer];

        public LogEntryStack GetStack => this._stacks[this._pointer.GetPointer];

        public void AddLogEntry(LogEntry logEntry)
        {
            this.AddStack.AddLogEntry(logEntry);
        }

        public LogEntry? GetLogEntry()
        {
            return this.GetStack.GetLogEntry();
        }

        public void ChangPointer()
        {
            this._pointer.Change();
            Debug.WriteLine($"GetStack: {this.GetStack.Length}");
        }

        #region IDisposable 

        private bool _disposed;
        ~LogEntryStackManager()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (this._disposed) return;
            this._disposed = true;

            this._stacks[0].Dispose();
            this._stacks[1].Dispose();

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
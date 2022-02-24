using System.Collections.Generic;

namespace Tentakel.Extensions.Logging.BackgroundWork
{
    internal class LogEntryStack
    {
        private readonly List<LogEntry> _stack = new();
        private int _getIndex;


        public int Length
        {
            get { return this._stack.Count; }
        }

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
    }
}
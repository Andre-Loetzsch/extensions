﻿using System.Collections;
using System.Collections.Generic;

namespace Oleander.Extensions.Logging.Abstractions
{
    internal readonly struct FormattedLogValues : IReadOnlyList<KeyValuePair<string, object?>>
    {
        private readonly List<KeyValuePair<string, object?>> _values;

        public FormattedLogValues(string logMessage, IEnumerable<KeyValuePair<string, object?>> values)
        {
            this._values = new (values);
            this.LogMessage = logMessage;
        }

        public KeyValuePair<string, object?> this[int index] => this._values[index];

        public int Count => this._values.Count;

        public string LogMessage { get; }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            return this._values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._values.GetEnumerator();
        }

        public override string ToString()
        {
            return this.LogMessage;
        }
    }
}
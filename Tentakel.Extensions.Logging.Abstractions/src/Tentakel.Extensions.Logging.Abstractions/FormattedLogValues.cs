using System.Collections;

namespace Tentakel.Extensions.Logging.Abstractions
{
    internal readonly struct FormattedLogValues : IReadOnlyList<KeyValuePair<string, object?>>
    {
        private readonly List<KeyValuePair<string, object?>> _values;

        public FormattedLogValues(string logMessage, IEnumerable<KeyValuePair<string, object?>> values)
        {
            this._values = new List<KeyValuePair<string, object?>>(values);
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
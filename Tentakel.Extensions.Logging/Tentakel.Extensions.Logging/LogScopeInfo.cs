using System.Collections.Generic;

namespace Tentakel.Extensions.Logging
{
    public class LogScopeInfo
    {
        public string Text { get; set; }
        public Dictionary<string, object> Properties { get; set; }

        public override string ToString()
        {
            return $"Text: {this.Text}; Properties: {this.Properties?.ToLogString()}";
        }
    }
}
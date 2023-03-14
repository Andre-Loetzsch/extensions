using System;
using System.Collections.Generic;

namespace Oleander.Extensions.Logging.File
{
    public class ValuesFormatter
    {
        public ValuesFormatter(string formatTemplate)
        {
            this.FormatTemplate = formatTemplate;
        }

        public string FormatTemplate { get; }

        public static IEnumerable<string> ExtractDateTimeFormats(string value)
        {
            var startIndex = value.IndexOf("{dateTime", StringComparison.Ordinal);

            while (startIndex > -1)
            {
                var endIndex = value.IndexOf("}", startIndex, StringComparison.Ordinal);

                if (endIndex > startIndex + 10)
                {
                    yield return value.Substring(startIndex +10, endIndex - startIndex - 10);
                }

                startIndex = value.IndexOf("{dateTime:", startIndex +1, StringComparison.Ordinal);
            }
        }

        public static IEnumerable<string> ExtractKeys(string value)
        {
            var startIndex = value.IndexOf("{", StringComparison.Ordinal);

            while (startIndex > -1)
            {
                var endIndex = value.IndexOf("}", startIndex, StringComparison.Ordinal);

                if (endIndex > startIndex + 1)
                {
                    var subStartIndex = value.IndexOf("{", startIndex + 1, StringComparison.Ordinal);

                    while (subStartIndex > -1 && subStartIndex < endIndex)
                    {
                        startIndex = subStartIndex;
                        subStartIndex = value.IndexOf("{", startIndex + 2, StringComparison.Ordinal);
                    }

                    yield return value.Substring(startIndex + 1, endIndex - startIndex - 1);

                }

                startIndex = value.IndexOf("{", startIndex + 1, StringComparison.Ordinal);
            }
        }
    }

}
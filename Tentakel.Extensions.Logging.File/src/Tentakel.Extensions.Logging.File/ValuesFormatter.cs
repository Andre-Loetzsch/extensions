namespace Tentakel.Extensions.Logging.JsonFile
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
            var startIndex = value.IndexOf("{dateTime");

            while (startIndex > -1)
            {
                var endIndex = value.IndexOf("}", startIndex);

                if (endIndex > startIndex + 10)
                {
                    yield return value.Substring(startIndex +10, endIndex - startIndex - 10);
                }

                startIndex = value.IndexOf("{dateTime:", startIndex +1);
            }
        }

        public static IEnumerable<string> ExtractKeys(string value)
        {
            var startIndex = value.IndexOf("{");

            while (startIndex > -1)
            {
                var endIndex = value.IndexOf("}", startIndex);

                if (endIndex > startIndex + 1)
                {
                    var subStartIndex = value.IndexOf("{", startIndex + 1);

                    while (subStartIndex > -1 && subStartIndex < endIndex)
                    {
                        startIndex = subStartIndex;
                        subStartIndex = value.IndexOf("{", startIndex + 2);
                    }

                    yield return value.Substring(startIndex + 1, endIndex - startIndex - 1);

                }

                startIndex = value.IndexOf("{", startIndex + 1);
            }
        }
    }

}
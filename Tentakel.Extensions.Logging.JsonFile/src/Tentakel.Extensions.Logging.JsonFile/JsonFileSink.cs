using System.Text;
using Tentakel.Extensions.Logging.LoggerSinks;

namespace Tentakel.Extensions.Logging.JsonFile
{
    public class JsonFileSink : LoggerSinkBase
    {

        private FileStream? _fileStream;

        public override async void Log(LogEntry logEntry)
        {

            if (this._fileStream == null)
            {
                var path = Path.Combine(AppContext.BaseDirectory, "Logging");

                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                path = Path.Combine(path, "Trace.log");
                //this._fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);

                this._fileStream = System.IO.File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);

            }

            //var x = 
            //"{\r\n" +
            //$"  \"DateTime\": \"{logEntry.DateTime:o}\",\r\n" +
            //$"  \"LogEntryId\": {logEntry.LogEntryId},\r\n" +
            //$"  \"ApplicationName\": \"{logEntry.ApplicationName}\",\r\n" +
            //$"  \"UserName\": \"{logEntry.UserName}\",\r\n" +
            //"  \"DomainName\": \"L316IT012\",\r\n" +
            //"  \"MachineName\": \"L316IT012\",\r\n" +
            //"  \"AppDomainId\": 1,\r\n" +
            //"  \"ProcessId\": 1388,\r\n" +
            //"  \"ProcessName\": \"testhost\",\r\n" +
            //"  \"ThreadId\": 16,\r\n" +
            //"  \"ThreadName\": \".NET Long Running Task\",\r\n" +
            //"  \"LoggerSinkType\": \"JsonFileSink\",\r\n" +
            //"  \"LoggerSinkName\": \"Unit Test Sink\",\r\n" +
            //"  \"StackTrace\": null,\r\n" +
            //"  \"Exception\": null,\r\n" +
            //"  \"State\": [" +
            //"    {" +
            //"      \"Key\": \"{OriginalFormat}\",\r\n" +
            //"     \"Value\": \"Hello, file logger!\"" +
            //"    }" +
            //"  ],\r\n" +
            //"  \"Correlation\": null,\r\n" +
            //"  \"LogLevel\": 1,\r\n" +
            //"  \"LogCategory\": \"\",\r\n" +
            //"  \"EventId\": 0,\r\n" +
            //"  \"SourceCategory\": \"test\",\r\n" +
            //"  \"Source\": null,\r\n" +
            //"  \"Message\": \"Hello, file logger!\",\r\n" +
            //"  \"DateTimeFormat\": \"yyyy-MM-dd HH:mm:ss fff\",\r\n" +
            //"  \"Attributes\": {" +
            //"    \"{OriginalFormat}\": \"Hello, file logger!\"" +
            //"  },\r\n" +
            //"  \"Scopes\": null" +
            //"}\r\n";


            //Thread.Sleep(10);


            //var json = System.Text.Json.JsonSerializer.Serialize(logEntry, options: new System.Text.Json.JsonSerializerOptions { WriteIndented = true });



            var buffer = Encoding.UTF8.GetBytes($"{logEntry.LogEntryId:000000} {logEntry.DateTime:yyyy.MM.dd hh:mm:ss} {logEntry.Message}\r\n");
            //var buffer = Encoding.UTF8.GetBytes($"{json},\r\n");

            //var buffer = Encoding.UTF8.GetBytes(string.Concat(logEntry.ToString(), Environment.NewLine));


            //this._fileStream.Write(buffer, 0, buffer.Length);
            //this._fileStream.Flush();



            this._fileStream.Write(buffer, 0, buffer.Length);
            this._fileStream.Flush();










        }


        protected override void Dispose(bool disposing)
        {
            this._fileStream?.Flush();
            this._fileStream?.Dispose();

            base.Dispose(disposing);
        }
    }
}
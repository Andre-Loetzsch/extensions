using System.Globalization;
using System.Text;
using Tentakel.Extensions.Logging.LoggerSinks;

namespace Tentakel.Extensions.Logging.JsonFile
{
    public class FileSink : LoggerSinkBase
    {
        private const string defaultFileNameTemplate = "{baseDirectory}/Logging/{dateTime:yyyy}/{dateTime:MM}/{processName}/{dateTime:yyyy-MM-dd}.{processId}.log";
        private FileStream? _fileStream;
        private DateTime _fileNameExpiryDateTime = DateTime.MinValue;

        public ITextFormatter? TextFormatter { get; set; }

        private string _fileNameTemplate = defaultFileNameTemplate;
        public string FileNameTemplate
        {
            get => this._fileNameTemplate;
            set
            {
                this._fileNameTemplate = value;
                this._fileNameExpiryDateTime = DateTime.MinValue;
            }
        }

        public string? FileName { get; private set; }
        public int MaxFileSize { get; set; }
        public bool OverrideExistingFile { get; set; }

        public override async void Log(LogEntry logEntry)
        {
            if (this._fileNameExpiryDateTime <= logEntry.DateTime || this._fileStream == null)
            {
                if (this._fileStream != null) this._fileStream.Close();

                this.SetFileName(logEntry);

                var directory = Path.GetDirectoryName(this.FileName);

                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if ( this.OverrideExistingFile)
                {
                    var fileExtension = Path.GetExtension(this.FileName) ?? string.Empty;
                    var index = 1;
                    var fileName = this.FileName!.Replace(fileExtension, $".Part{index}{fileExtension}");

                    while (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                        index++;
                        fileName = this.FileName!.Replace(fileExtension, $".Part{index}{fileExtension}");
                    }
                }

                this._fileStream = File.Open(this.FileName!, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

                if (!this.OverrideExistingFile)
                {
                    this._fileStream.Position = this._fileStream.Length;
                }
            }
            
            var logString = this.TextFormatter != null ?
                this.TextFormatter.Format(logEntry) :
                $"[{logEntry.LogEntryId:0000000}] [{logEntry.DateTime:yyyy.MM.dd hh:mm:ss}] [{logEntry.LogLevel}] [{logEntry.LogCategory}] {logEntry.Source} - {logEntry.Message}\r\n";

            var buffer = Encoding.UTF8.GetBytes(logString);

            if (this.MaxFileSize > 0 && this._fileStream.Length + buffer.Length > this.MaxFileSize)
            {
                var fileExtension = Path.GetExtension(this.FileName) ?? string.Empty;
                var index = 1;
                var newFileName = this.FileName!.Replace(fileExtension, $".Part{index}{fileExtension}");

                while (File.Exists(newFileName))
                {
                    index++;
                    newFileName = this.FileName!.Replace(fileExtension, $".Part{index}{fileExtension}");
                }

                this._fileStream.Close();
                File.Move(this.FileName, newFileName, true);
                this._fileStream = File.Open(this.FileName!, FileMode.Create, FileAccess.Write, FileShare.Read);
            }

            this._fileStream.Write(buffer, 0, buffer.Length);
            this._fileStream.Flush();
        }

        private void SetFileName(LogEntry logEntry)
        {
            var fileName = this.FileNameTemplate;
            fileName = fileName.Replace('\\', Path.DirectorySeparatorChar);
            fileName = fileName.Replace('/', Path.DirectorySeparatorChar);

            TimeSpan ts = TimeSpan.Zero;

            foreach (var dateTimeFormat in ValuesFormatter.ExtractDateTimeFormats(fileName))
            {
                var dateTimeAsString = logEntry.DateTime.ToString(dateTimeFormat);
                var dateTimeAsStringMinValue = new DateTime(1, 1, 1, 1, 1, 1).ToString(dateTimeFormat);

                if (DateTime.TryParseExact(dateTimeAsStringMinValue, dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                {
                    if (ts.Seconds == 0) ts = ts.Add(TimeSpan.FromSeconds(dateTime.Second));
                    if (ts.Minutes == 0) ts = ts.Add(TimeSpan.FromMinutes(dateTime.Minute));
                    if (ts.Hours == 0) ts = ts.Add(TimeSpan.FromHours(dateTime.Hour));
                    if (ts.Days == 0) ts = ts.Add(TimeSpan.FromDays(dateTime.Day));
                }

                fileName = fileName.Replace(string.Concat("{dateTime:", dateTimeFormat, "}"), dateTimeAsString);
            }

            if (ts.Seconds > 0)
            {
                this._fileNameExpiryDateTime = logEntry.DateTime.AddSeconds(1);
            }
            else if (ts.Minutes > 0)
            {
                this._fileNameExpiryDateTime = logEntry.DateTime.AddMinutes(1);
            }
            else if (ts.Hours > 0)
            {
                this._fileNameExpiryDateTime = logEntry.DateTime.AddMinutes(1);
            }
            else if (ts.Days > 0)
            {
                this._fileNameExpiryDateTime = logEntry.DateTime.Date.AddDays(1);
            }


            foreach (var key in ValuesFormatter.ExtractKeys(fileName))
            {
                switch (key)
                {
                    case "baseDirectory":
                        fileName = fileName.Replace("{baseDirectory}", AppDomain.CurrentDomain.BaseDirectory);
                        break;
                    case "processName":
                        fileName = fileName.Replace("{processName}", logEntry.ProcessName);
                        break;
                    case "processId":
                        fileName = fileName.Replace("{processId}", logEntry.ProcessId.ToString());
                        break;
                    case "appDomainId":
                        fileName = fileName.Replace("{appDomainId}", logEntry.AppDomainId.ToString());
                        break;
                    case "applicationName":
                        fileName = fileName.Replace("{applicationName}", logEntry.ApplicationName);
                        break;

                    default:
                        break;
                }
            }

            this.FileName = fileName;
        }





        protected override void Dispose(bool disposing)
        {
            this._fileStream?.Flush();
            this._fileStream?.Dispose();

            base.Dispose(disposing);
        }
    }
}
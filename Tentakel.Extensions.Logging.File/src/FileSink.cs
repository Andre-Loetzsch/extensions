using System.Diagnostics;
using System.Globalization;
using System.Text;
using Tentakel.Extensions.Logging.TextFormatters.Abstractions.LoggerSinks;
using IOFile = System.IO.File;

namespace Tentakel.Extensions.Logging.File
{
    public class FileSink : TextLoggerSinkBase
    {
        private const string defaultFileNameTemplate = "{baseDirectory}/Logging/{dateTime:yyyy}/{dateTime:MM}/{processName}/{dateTime:yyyy-MM-dd}.{processId}.log";
        private FileStream? _fileStream;
        private DateTime _fileNameExpiryDateTime = DateTime.MinValue;

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

        #region Log

        public override void Log(LogEntry logEntry)
        {
            if (this.IsDisposed) return;

            if (this._fileNameExpiryDateTime <= logEntry.DateTime || this._fileStream == null)
            {
                this.CreateTextFormatter();
                this.CreateFile();
            }

            var buffer = this.GetBytes(logEntry);

            if (this.MaxFileSize > 0 && this._fileStream!.Length + buffer.Length > this.MaxFileSize)
            {
                this.CreatePartialFile();
            }

            this._fileStream!.Write(buffer, 0, buffer.Length);
            this._fileStream!.Flush();
        }

        #endregion

        #region private methods

        private void CreateFile()
        {
            (var fileName, this._fileNameExpiryDateTime) = CreateFileNameAndExpiryDateTimeFomTemplate(this.FileNameTemplate);

            if (this._fileStream != null && this.FileName == fileName) return;

            if (this.OverrideExistingFile && !string.IsNullOrEmpty(this.FileName))
            {
                foreach (var partialFile in this.FindExistsPartialFileNames(this.FileName))
                {
                    IOFile.Delete(partialFile);
                }
            }

            var oldFileName = this.FileName;

            this.FileName = fileName;
            this._fileStream?.Close();

            this.FileNameChanged(oldFileName, fileName);

            var directory = Path.GetDirectoryName(this.FileName);

            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (this.OverrideExistingFile)
            {
                foreach (var partialFile in this.FindExistsPartialFileNames(this.FileName))
                {
                    IOFile.Delete(partialFile);
                }
            }

            this._fileStream = IOFile.Open(this.FileName!, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

            if (!this.OverrideExistingFile)
            {
                this._fileStream.Position = this._fileStream.Length;
            }
        }

        private void CreatePartialFile()
        {
            this._fileStream?.Close();

            if (string.IsNullOrEmpty(this.FileName))
            {
                (this.FileName, this._fileNameExpiryDateTime) = CreateFileNameAndExpiryDateTimeFomTemplate(this.FileNameTemplate);
            }

            var partialFileName = this.FindNextPartialFileName(this.FileName);

            IOFile.Move(this.FileName, partialFileName, true);
            this._fileStream = IOFile.Open(this.FileName!, FileMode.Create, FileAccess.Write, FileShare.Read);

            this.PartialFileCreated(partialFileName);
        }

        private static (string, DateTime) CreateFileNameAndExpiryDateTimeFomTemplate(string fileNameTemplate)
        {
            var fileName = fileNameTemplate;
            fileName = fileName.Replace('\\', Path.DirectorySeparatorChar);
            fileName = fileName.Replace('/', Path.DirectorySeparatorChar);

            var ts = TimeSpan.Zero;
            var fileDateTime = DateTime.Now;

            foreach (var dateTimeFormat in ValuesFormatter.ExtractDateTimeFormats(fileName))
            {
                var dateTimeAsString = fileDateTime.ToString(dateTimeFormat);
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

            var fileNameExpiryDateTime = fileDateTime.Date.AddDays(1);

            if (ts.Seconds > 0)
            {
                fileNameExpiryDateTime = fileDateTime.AddSeconds(1);
            }
            else if (ts.Minutes > 0)
            {
                fileNameExpiryDateTime = fileDateTime.AddMinutes(1);
            }
            else if (ts.Hours > 0)
            {
                fileNameExpiryDateTime = fileDateTime.AddMinutes(1);
            }
            else if (ts.Days > 0)
            {
                fileNameExpiryDateTime = fileDateTime.Date.AddDays(1);
            }

            foreach (var key in ValuesFormatter.ExtractKeys(fileName))
            {
                fileName = key switch
                {
                    "baseDirectory" => fileName.Replace("{baseDirectory}", AppDomain.CurrentDomain.BaseDirectory),
                    "processName" => fileName.Replace("{processName}", Process.GetCurrentProcess().ProcessName),
                    "processId" => fileName.Replace("{processId}", Environment.ProcessId.ToString()),
                    "appDomainId" => fileName.Replace("{appDomainId}", AppDomain.CurrentDomain.Id.ToString()),
                    "applicationName" => fileName.Replace("{applicationName}", AppDomain.CurrentDomain.FriendlyName),
                    _ => fileName
                };
            }

            return (fileName, fileNameExpiryDateTime);
        }

        #endregion

        #region protected virtual

        protected virtual IEnumerable<string> FindExistsPartialFileNames(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);
            var index = 1;
            var result = fileName.Replace(fileExtension, $".partial{index}{fileExtension}");

            while (IOFile.Exists(result))
            {
                yield return result;
                index++;
                result = fileName.Replace(fileExtension, $".partial{index}{fileExtension}");
            }
        }

        protected virtual string FindNextPartialFileName(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);
            var index = 1;
            var result = fileName.Replace(fileExtension, $".partial{index}{fileExtension}");

            while (IOFile.Exists(result))
            {
                index++;
                result = fileName.Replace(fileExtension, $".partial{index}{fileExtension}");
            }

            return result;
        }
        protected virtual void FileNameChanged(string? oldFileName, string newFileName)
        {

        }
        protected virtual void PartialFileCreated(string fileName)
        {

        }

        protected virtual byte[] GetBytes(LogEntry logEntry)
        {
            return Encoding.UTF8.GetBytes(this.TextFormatter.Format(logEntry));
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (this._fileStream?.CanWrite == true)
            {
                this._fileStream?.Flush();
            }

            this._fileStream?.Dispose();
            this._fileStream = null;

            base.Dispose(disposing);
        }

        #endregion
    }
}
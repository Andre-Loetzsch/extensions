using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Oleander.Extensions.Logging.TextFormatters.Abstractions.LoggerSinks;
using IOFile = System.IO.File;

namespace Oleander.Extensions.Logging.File
{
    public class FileSink : TextLoggerSinkBase
    {
        private FileStream? _fileStream;
        private DateTime _fileNameExpiryDateTime;
        private DateTime _archiveFileNameExpiryDateTime;

        #region FileName

        private const string defaultFileNameTemplate = "{baseDirectory}/Logging/{processName}/{processName}.log";

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

        private string _fileName = string.Empty;
        public string FileName
        {
            get => this._fileName;
            set
            {
                var directory = string.IsNullOrEmpty(value) ? string.Empty : Path.GetDirectoryName(value);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

                this._fileName = value;
            }
        }

        #endregion

        #region ArchiveFileNameTemplate

        private const string defaultArchiveFileNameTemplate = "{baseDirectory}/Logging/{dateTime:yyyy}/{dateTime:MM}/{processName}/{dateTime:yyyy-MM-dd}.log";

        private string _archiveFileNameTemplate = defaultArchiveFileNameTemplate;
        public string ArchiveFileNameTemplate
        {
            get => this._archiveFileNameTemplate;
            set
            {
                this._archiveFileNameTemplate = value;
                this._archiveFileNameExpiryDateTime = DateTime.MinValue;
            }
        }

        private string _archiveFileName = string.Empty;
        public string ArchiveFileName
        {
            get => this._archiveFileName;
            set
            {
                var directory = string.IsNullOrEmpty(value) ? string.Empty : Path.GetDirectoryName(value);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

                this._archiveFileName = value;
            }
        }

        #endregion

        public int MaxFileSize { get; set; }
        public bool OverrideExistingFile { get; set; }

        #region Log

        public override void Log(LogEntry logEntry)
        {
            if (this.IsDisposed) return;

            if (this.IsFileStreamOutOfDate(logEntry.DateTime))
            {
                this.CreateTextFormatter();
                this.UpdateFileStream();
            }

            var buffer = this.GetBytes(logEntry);

            if (this.MaxFileSize > 0 && this._fileStream!.Length + buffer.Length > this.MaxFileSize)
            {
                this.CreatePartialFile();
            }

            if (this._fileStream == null) return;
            
            if (this._fileStream.Position < this._fileStream.Length)
            {
                this._fileStream.Position = this._fileStream.Length;
            }

            this._fileStream.Write(buffer, 0, buffer.Length);
            this._fileStream.Flush();
        }

        #endregion

        #region private methods

        private bool IsFileStreamOutOfDate(DateTime dateTime)
        {
            if (this._fileStream == null) return true;
            if (this._fileNameExpiryDateTime <= dateTime) return true;
            return !this.OverrideExistingFile && this._archiveFileNameExpiryDateTime <= dateTime;
        }

        private void UpdateFileStream()
        {
            this._fileStream?.Close();

            if (this.OverrideExistingFile)
            {
                if (IOFile.Exists(this.FileName)) IOFile.Delete(this.FileName);

                (this.FileName, this._fileNameExpiryDateTime) = CreateFileNameAndExpiryDateTimeFromTemplate(DateTime.Now, this.FileNameTemplate);
                (this._fileStream, this.FileName) = OpenFileStream(this.FileName, FileMode.Create);
                return;
            }

            (var fileName, this._fileNameExpiryDateTime) = CreateFileNameAndExpiryDateTimeFromTemplate(DateTime.Now, this.FileNameTemplate);
            (var archiveFileName, this._archiveFileNameExpiryDateTime) = CreateFileNameAndExpiryDateTimeFromTemplate(DateTime.Now, this.ArchiveFileNameTemplate);
           
            if (string.IsNullOrEmpty(this.ArchiveFileName)) this.ArchiveFileName = archiveFileName;

            if (this.ArchiveFileName != archiveFileName)
            {
                if (IOFile.Exists(this.FileName))
                {
#if NET7_0_OR_GREATER
                    IOFile.Move(this.FileName, this.ArchiveFileName, true);
#else 
                    IOFile.Move(this.FileName, this.ArchiveFileName);
#endif
                    this.ArchiveFileCreated(this.FileName, this.ArchiveFileName);
                }
            }

            this.FileName = fileName;
            this.ArchiveFileName = archiveFileName;

            (this._fileStream, this.FileName) = OpenFileStream(this.FileName, FileMode.OpenOrCreate);
            this._fileStream.Position = this._fileStream.Length;
        }

        private void CreatePartialFile()
        {
            this._fileStream?.Close();

            if (this.OverrideExistingFile)
            {
                if (IOFile.Exists(this.FileName)) IOFile.Delete(this.FileName);

                (this.FileName, this._fileNameExpiryDateTime) = CreateFileNameAndExpiryDateTimeFromTemplate(DateTime.Now, this.FileNameTemplate);
                (this._fileStream, this.FileName) = OpenFileStream(this.FileName, FileMode.Create);

                return;
            }

            var partialFileName = this.FindNextPartialFileName(this.ArchiveFileName);

#if NET7_0_OR_GREATER

            IOFile.Move(this.FileName, partialFileName, true);
#else
            IOFile.Move(this.FileName, partialFileName);
#endif

            this.PartialFileCreated(this.FileName, partialFileName);

            (this._fileStream, this.FileName) = OpenFileStream(this.FileName, FileMode.Create);
        }

        internal static (string, DateTime) CreateFileNameAndExpiryDateTimeFromTemplate(DateTime dateTimeNow, string fileNameTemplate)
        {
            var fileName = fileNameTemplate;
            fileName = fileName.Replace('\\', Path.DirectorySeparatorChar);
            fileName = fileName.Replace('/', Path.DirectorySeparatorChar);

            var b = new bool[6];

            foreach (var dateTimeFormat in ValuesFormatter.ExtractDateTimeFormats(fileName))
            {
                var dateTimeAsString = dateTimeNow.ToString(dateTimeFormat); 
                var dateTimeAsStringMinValue = new DateTime(2, 2, 2, 1, 1, 1).ToString(dateTimeFormat);

                if (!DateTime.TryParseExact(dateTimeAsStringMinValue, dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime)) continue;

                if (dateTime.Year > 1) b[0] = true;
                if (dateTime.Month > 1) b[1] = true;
                if (dateTime.Day > 1) b[2] = true;
                if (dateTime.Hour > 0) b[3] = true;
                if (dateTime.Minute > 0) b[4] = true;
                if (dateTime.Second > 0) b[5] = true;

                fileName = fileName.Replace(string.Concat("{dateTime:", dateTimeFormat, "}"), dateTimeAsString);
            }

            var fileNameExpiryDateTime = dateTimeNow.Date.AddDays(1);

            if (b[5])
            {
                fileNameExpiryDateTime = dateTimeNow.AddSeconds(1);
            }
            else if (b[4])
            {
                fileNameExpiryDateTime = dateTimeNow
                    .AddMinutes(1)
                    .AddSeconds(dateTimeNow.Second * -1);
            }
            else if (b[3])
            {
                fileNameExpiryDateTime = dateTimeNow
                    .AddHours(1)
                    .AddMinutes(dateTimeNow.Minute * -1)
                    .AddSeconds(dateTimeNow.Second * -1);
            }
            else if (b[2])
            {
                fileNameExpiryDateTime = dateTimeNow.Date.AddDays(1);
            }
            else if (b[1])
            {
                fileNameExpiryDateTime = dateTimeNow.Date
                    .AddMonths(1)
                    .AddDays((dateTimeNow.Day - 1) * -1);

            }
            else if (b[0])
            {
                fileNameExpiryDateTime = dateTimeNow.Date
                    .AddYears(1)
                    .AddMonths((dateTimeNow.Month - 1) * -1)
                    .AddDays((dateTimeNow.Day - 1) * -1);
            }

            foreach (var key in ValuesFormatter.ExtractKeys(fileName))
            {
                fileName = key switch
                {
                    "baseDirectory" => fileName.Replace("{baseDirectory}", AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\')),
                    "processName" => fileName.Replace("{processName}", Process.GetCurrentProcess().ProcessName),

#if NET7_0_OR_GREATER
                    "processId" => fileName.Replace("{processId}", Environment.ProcessId.ToString()),
#else
                    "processId" => fileName.Replace("{processId}", Process.GetCurrentProcess().Id.ToString()),

#endif
                    "appDomainId" => fileName.Replace("{appDomainId}", AppDomain.CurrentDomain.Id.ToString()),
                    "applicationName" => fileName.Replace("{applicationName}", AppDomain.CurrentDomain.FriendlyName),
                    _ => fileName
                };
            }

            return (fileName, fileNameExpiryDateTime);
        }

        private static (FileStream, string) OpenFileStream(string fileName, FileMode fileMode)
        {
            FileStream? fs = null;
            var fileExtension = Path.GetExtension(fileName);
            var index = 0;

            while (fs == null)
            {
                try
                {
                    if (index > 0)
                    {
                        fileName = fileName.Replace(fileExtension, $"{index}{fileExtension}");
                    }

                    fs = IOFile.Open(fileName, fileMode, FileAccess.Write, FileShare.ReadWrite);
                }
                catch (Exception ex) when (IOFile.Exists(fileName))
                {
                    Debug.WriteLine(ex);
                    index++;
                }
            }

            return (fs, fileName);
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

        protected virtual void ArchiveFileCreated(string fileName, string archiveFileName)
        {

        }
        protected virtual void PartialFileCreated(string originalFileName, string partialFileName)
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
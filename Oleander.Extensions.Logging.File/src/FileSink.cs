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
                var directory = Path.GetDirectoryName(value);
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
                var directory = Path.GetDirectoryName(value);
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

                (this.FileName, this._fileNameExpiryDateTime) = CreateFileNameAndExpiryDateTimeFromTemplate(this.FileNameTemplate);
                (this._fileStream, this.FileName) = OpenFileStream(this.FileName, FileMode.Create);
                return;
            }

            (var fileName, this._fileNameExpiryDateTime) = CreateFileNameAndExpiryDateTimeFromTemplate(this.FileNameTemplate);
            (var archiveFileName, this._archiveFileNameExpiryDateTime) = CreateFileNameAndExpiryDateTimeFromTemplate(this.ArchiveFileNameTemplate);
           
            if (string.IsNullOrEmpty(this.ArchiveFileName)) this.ArchiveFileName = archiveFileName;

            if (this.ArchiveFileName != archiveFileName)
            {
                if (IOFile.Exists(this.FileName))
                {
                    IOFile.Move(this.FileName, this.ArchiveFileName, true);
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

                (this.FileName, this._fileNameExpiryDateTime) = CreateFileNameAndExpiryDateTimeFromTemplate(this.FileNameTemplate);
                (this._fileStream, this.FileName) = OpenFileStream(this.FileName, FileMode.Create);

                return;
            }

            var partialFileName = this.FindNextPartialFileName(this.ArchiveFileName);

            IOFile.Move(this.FileName, partialFileName, true);
            this.PartialFileCreated(this.FileName, partialFileName);

            (this._fileStream, this.FileName) = OpenFileStream(this.FileName, FileMode.Create);
        }

        private static (string, DateTime) CreateFileNameAndExpiryDateTimeFromTemplate(string fileNameTemplate)
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
                    "baseDirectory" => fileName.Replace("{baseDirectory}", AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\')),
                    "processName" => fileName.Replace("{processName}", Process.GetCurrentProcess().ProcessName),
                    "processId" => fileName.Replace("{processId}", Environment.ProcessId.ToString()),
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
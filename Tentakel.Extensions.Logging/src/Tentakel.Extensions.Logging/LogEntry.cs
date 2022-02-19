using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Tentakel.Extensions.Logging
{
    [DebuggerDisplay("{LogEntryId} Msg: {Message} LogCategory: SourceCategory: {SourceCategory} Source: {Source}")]
    public class LogEntry
    {
        private static long logEntryId;

        private static readonly string applicationName  = AppDomain.CurrentDomain.FriendlyName;
        private static readonly string userName = Environment.UserName;
        private static readonly string domainName = Environment.UserDomainName;
        private static readonly string machineName  = Environment.MachineName;
        private static readonly int appDomainId = AppDomain.CurrentDomain.Id;
        private static readonly int processId  = Process.GetCurrentProcess().Id;
        private static readonly string processName  = Process.GetCurrentProcess().ProcessName;

        public LogEntry()
        {
            this.DateTime = DateTime.Now;
            this.ThreadId = Thread.CurrentThread.ManagedThreadId;
            this.ThreadName = string.IsNullOrWhiteSpace(Thread.CurrentThread.Name) ?
                $"Thread{this.ThreadId}" : Thread.CurrentThread.Name;

            this.Attributes = new Dictionary<string, object>();
            this.LogCategory = string.Empty;
            this.DateTimeFormat = "yyyy-MM-dd HH:mm:ss fff";

            Interlocked.Increment(ref logEntryId);
            this.LogEntryId = logEntryId;
        }

        #region public Properties

        public DateTime DateTime { get; set; }
        public long LogEntryId { get; set; }
        public string ApplicationName { get; set; } = applicationName;
        public string UserName { get; set; } = userName;
        public string DomainName { get; set; } = domainName;
        public string MachineName { get; set; } = machineName;
        public int AppDomainId { get; set; } = appDomainId;
        public int ProcessId { get; set; } = processId;
        public string ProcessName { get; set; } = processName;
        public int ThreadId { get; set; }
        public string ThreadName { get; set; }
        public Type LoggerSinkType { get; set; }
        public string LoggerSinkName { get; set; }
        public StackTrace StackTrace { get; set; }
        public Exception Exception { get; set; }
        public object State { get; set; }
        public object Correlation { get; set; }
        public LogLevel LogLevel { get; set; }
        public string LogCategory { get; set; }
        public int EventId { get; set; }
        public string SourceCategory { get; set; }


        internal string _source;
        public string Source
        { 
            get
            {
                if (string.IsNullOrEmpty(this._source) &&
                 this.Attributes.TryGetValue("assembly.FullName", out var value) && value is string assemblyFullName &&
                    this.Attributes.TryGetValue("callerFilePath", out value) && value is string callerFilePath &&
                    this.Attributes.TryGetValue("callerMemberName", out value) && value is string callerMemberName &&
                    this.Attributes.TryGetValue("callerLineNumber", out value) && value is int callerLineNumber)
                {

                    var assemblyName = assemblyFullName.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)[0];
                    var indexOf = callerFilePath.IndexOf(assemblyName);

                    if (indexOf > 0)
                    {
                        var source = string
                            .Join(".", callerFilePath
                            .Replace("\\", ".")
                            .Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries)[..^1])[indexOf..];

                        this._source = $"{source}.{callerMemberName}[{callerLineNumber}]";
                    }
                }
                else
                {
                    if (this.StackTrace != null)
                    {
                        if (SourceResolver.TryFindStackTraceSource(this.LoggerSinkType, this.StackTrace, out var source))
                        {
                            this._source = source;
                        }
                    }
                }

                return this._source;    
            }

            set
            {
                this._source = value;   
            }
        }





        public string Message { get; set; }
        public string DateTimeFormat { get; set; }
        public IDictionary<string, object> Attributes { get; set; }
        public List<LogScopeInfo> Scopes { get; set; }

        #endregion

        public override string ToString()
        {
            return $"MachineName:{this.MachineName}" +
                   $"|ProcessId:{this.ProcessId}" +
                   $"|ProcessName:{this.ProcessName}" +
                   $"|AppDomainId:{this.AppDomainId}" +
                   $"|ApplicationName:{this.ApplicationName}" +
                   $"|DomainName:{this.DomainName}" +
                   $"|UserName:{this.UserName}" +
                   $"|LoggerSinkType:{this.LoggerSinkType}" +
                   $"|TraceListenerName:{this.LoggerSinkName}" +
                   $"|DateTimeFormat:{this.DateTimeFormat}" +
                   $"|DateTime:{this.DateTime.ToString(this.DateTimeFormat)}" +
                   $"|LogEntryId:{this.LogEntryId}" +
                   $"|ThreadName:{this.ThreadName}" +
                   $"|ThreadId:{this.ThreadId}" +
                   $"|Correlation:{this.Correlation}" +
                   $"|LogLevel:{this.LogLevel}" +
                   $"|LogCategory:{this.LogCategory}" +
                   $"|EventId:{this.EventId}" +
                   $"|SourceCategory:{this.SourceCategory}" +
                   $"|Source:{this.Source}" +
                   $"|Message:{this.Message}" +
                   $"|Attributes:{this.Attributes.ToLogString()}";
        }
    }
}
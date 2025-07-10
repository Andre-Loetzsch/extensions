using System.Collections.Generic;
using System.Linq;
using Oleander.Extensions.Logging.TextFormatters.Abstractions;

namespace Oleander.Extensions.Logging.TextFormatters;

public class BlockTextFormatter : BlockTextFormatterBase
{
    private readonly List<TextBlockInfo> _textBlockInfos = [];

    public BlockTextFormatter()
    {
        this._textBlockInfos.Add(new(Pad.PadLeft, "LogEntryId:", "|"));
        this._textBlockInfos.Add(new(Pad.PadRight, "DateTime:", "|"));

        this._textBlockInfos.Add(new(Pad.PadRight, "MachineName:", "|"));

        this._textBlockInfos.Add(new(Pad.PadLeft, "AppDomainId:", "|"));
        this._textBlockInfos.Add(new(Pad.PadRight, "ApplicationName:", "|"));

        this._textBlockInfos.Add(new(Pad.PadLeft, "ProcessId:", "|"));
        this._textBlockInfos.Add(new(Pad.PadRight, "ProcessName:", "|"));

        this._textBlockInfos.Add(new(Pad.PadLeft, "ThreadId:", "|"));
        this._textBlockInfos.Add(new(Pad.PadRight, "ThreadName:", "|"));

        this._textBlockInfos.Add(new(Pad.PadRight, "DomainName:", "|"));
        this._textBlockInfos.Add(new(Pad.PadRight, "UserName:", "|"));

        this._textBlockInfos.Add(new(Pad.PadRight, "LoggerSinkType:", "|"));
        this._textBlockInfos.Add(new(Pad.PadRight, "LoggerSinkName:", "|"));
        this._textBlockInfos.Add(new(Pad.PadRight, "Source:", "|"));

        this._textBlockInfos.Add(new(Pad.PadRight, "LogLevel:", "|"));
        this._textBlockInfos.Add(new(Pad.PadRight, "LogCategory:", "|"));

        this._textBlockInfos.Add(new(Pad.PadRight, "Correlation:", "|"));
        this._textBlockInfos.Add(new(Pad.PadRight, "EventId:", "|"));

        this._textBlockInfos.Add(new(Pad.PadRight, "Message:", "|"));
    }
   
    protected override IEnumerable<TextBlockInfo> GeTextBlockInfos(LogEntry logEntry)
    {

        this._textBlockInfos[0].SetValue(logEntry.LogEntryId.ToString("000000"));
        this._textBlockInfos[1].SetValue(logEntry.DateTime.ToString("yyyy-MM-dd HH:mm:ss fff"));
        
        this._textBlockInfos[2].SetValue(logEntry.MachineName);
        
        this._textBlockInfos[3].SetValue(logEntry.AppDomainId);
        this._textBlockInfos[4].SetValue(logEntry.ApplicationName);

        this._textBlockInfos[5].SetValue(logEntry.ProcessId);
        this._textBlockInfos[6].SetValue(logEntry.ProcessName);

        this._textBlockInfos[7].SetValue(logEntry.ThreadId);
        this._textBlockInfos[8].SetValue(logEntry.ThreadName);

        this._textBlockInfos[9].SetValue(logEntry.DomainName);
        this._textBlockInfos[10].SetValue(logEntry.UserName);
        
        this._textBlockInfos[11].SetValue(logEntry.LoggerSinkType);
        this._textBlockInfos[12].SetValue(logEntry.LoggerSinkName);
        this._textBlockInfos[13].SetValue(logEntry.Source);
       
        this._textBlockInfos[14].SetValue(logEntry.LogLevel);
        this._textBlockInfos[15].SetValue(logEntry.LogCategory);
      
        this._textBlockInfos[16].SetValue(logEntry.Correlation);
        this._textBlockInfos[17].SetValue(logEntry.EventId);
        
        this._textBlockInfos[18].SetValue(logEntry.Message);

        if (logEntry.Exception is null) return this._textBlockInfos;

        var list = this._textBlockInfos.ToList();
        var item = new TextBlockInfo(Pad.PadRight, "Exception:", "|");
        item.SetValue(logEntry.Exception);

        list.Add(item);

        return list;
    }
}
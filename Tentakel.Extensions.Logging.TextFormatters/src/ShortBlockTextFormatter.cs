using System.Collections.Generic;

namespace Tentakel.Extensions.Logging.TextFormatters;

public class ShortBlockTextFormatter : BlockTextFormatterBase
{
    private readonly List<TextBlockInfo> _textBlockInfos = new();

    public ShortBlockTextFormatter()
    {
        this._textBlockInfos.Add(new(Pad.PadLeft,  string.Empty, "|"));
        this._textBlockInfos.Add(new(Pad.PadRight, string.Empty, "|"));

        this._textBlockInfos.Add(new(Pad.PadRight, string.Empty, "|"));

        this._textBlockInfos.Add(new(Pad.PadLeft,  string.Empty, "|"));
        this._textBlockInfos.Add(new(Pad.PadRight, string.Empty, "|"));

        this._textBlockInfos.Add(new(Pad.PadLeft,  string.Empty, "|"));
        this._textBlockInfos.Add(new(Pad.PadRight, string.Empty, "|"));

        this._textBlockInfos.Add(new(Pad.PadLeft,  string.Empty, "|"));
        this._textBlockInfos.Add(new(Pad.PadRight, string.Empty, "|"));

        this._textBlockInfos.Add(new(Pad.PadRight, string.Empty, "|"));
        this._textBlockInfos.Add(new(Pad.PadRight, string.Empty, "|"));

        this._textBlockInfos.Add(new(Pad.PadRight, string.Empty, "|"));
        this._textBlockInfos.Add(new(Pad.PadRight, string.Empty, "|"));
        this._textBlockInfos.Add(new(Pad.PadRight, string.Empty, "|"));

        this._textBlockInfos.Add(new(Pad.PadRight, string.Empty, "|"));
        this._textBlockInfos.Add(new(Pad.PadRight, string.Empty, "|"));

        this._textBlockInfos.Add(new(Pad.PadRight, string.Empty, "|"));
        this._textBlockInfos.Add(new(Pad.PadRight, string.Empty, "|"));

        this._textBlockInfos.Add(new(Pad.PadRight, string.Empty, "|"));
    }

    public static ShortBlockTextFormatter CreateInstance()
    {
        return new ShortBlockTextFormatter();
    }

    protected override IEnumerable<TextBlockInfo> GeTextBlockInfos(LogEntry logEntry)
    {
        this._textBlockInfos[0].SetValue(logEntry.LogEntryId);
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

        return this._textBlockInfos;
    }
}
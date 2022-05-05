using System.Text;
using Tentakel.Extensions.Logging.TextFormatters.Abstractions;

namespace Tentakel.Extensions.Logging.TextFormatters;

public abstract class BlockTextFormatterBase : ITextFormatter
{
    private readonly StringBuilder _formatBuilder = new();
    protected abstract IEnumerable<TextBlockInfo> GeTextBlockInfos(LogEntry logEntry);

    public string Format(LogEntry logEntry)
    {
        this._formatBuilder.Length = 0;

        var textBlockInfos = this.GeTextBlockInfos(logEntry).ToList();
        var maxLines = textBlockInfos.Max(x => x.Lines.Length);

        for (var i = 0; i < maxLines; i++)
        {
            foreach (var textBlockInfo in textBlockInfos)
            {
                this._formatBuilder.Append(textBlockInfo[i]);
            }

            this._formatBuilder.AppendLine();
        }

        return this._formatBuilder.ToString();
    }
}
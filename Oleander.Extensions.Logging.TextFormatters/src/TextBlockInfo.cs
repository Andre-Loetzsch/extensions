using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Oleander.Extensions.Logging.TextFormatters;

[DebuggerDisplay("Prefix={Prefix}, PostFix={PostFix}, Pad={Pad}, MaxLength={MaxLength}, Line.Length={Lines.Length}")]
public class TextBlockInfo(Pad pad, string preFix = "", string postFix = "")
{
    private static readonly char[] separators = [' ', '\\', '/', '.'];

    public void SetValue(object? value)
    {
        if (value == null) return;
        var valueAsString = value.ToString() ?? string.Empty;

        this.Lines = WordWrap(valueAsString, this.WordWrapWidth);

        var maxLength = this.Lines.Max(x => x.Length);
        if (maxLength > this.MaxLength) this.MaxLength = maxLength;
    }

    public int WordWrapWidth { get; set; }

    public Pad Pad { get; } = pad;
    public string Prefix { get; } = preFix;
    public string PostFix { get; } = postFix;

    public int MaxLength { get; set; }
    public string[] Lines { get; private set; } = [];

    public string this[int index]
    {
        get
        {
            var line = index < 0 || index >= this.Lines.Length ?
                string.Empty : this.Lines[index];

            line = this.Pad == Pad.PadLeft ?
                line.PadLeft(this.MaxLength) : line.PadRight(this.MaxLength);

            return index == 0 ?
                string.Concat(this.Prefix, line, this.PostFix) :
                string.Concat(string.Empty.PadLeft(this.Prefix.Length), line, this.PostFix);
        }
    }

    private static string[] WordWrap(string text, int maxWidth)
    {
        var lines = text.Split([Environment.NewLine], StringSplitOptions.None).ToList();

        if (maxWidth < 1 || text.Length <= maxWidth || lines.All(x => x.Length <= maxWidth)) return lines.ToArray();

        var resultList = new List<string>();

        foreach (var separator in separators)
        {
            resultList.Clear();

            foreach (var line in lines)
            {
                resultList.AddRange(WordWrap(line, separator, maxWidth)
                    .Split([Environment.NewLine], StringSplitOptions.None));
            }

            lines = string.Join(Environment.NewLine, resultList)
                .Split([Environment.NewLine], StringSplitOptions.None).ToList();

            if (lines.All(x => x.Length <= maxWidth)) break;
        }

        return lines.Select(x => x.TrimEnd(' ')).ToArray();
    }

    private static string WordWrap(string text, char separator, int maxWidth)
    {
        if (maxWidth < 1 || text.Length <= maxWidth) return text;

        var words = text.Split(separator);
        if (words.Length < 2) return text;

        var wrappedText = new StringBuilder();
        var currentLine = new StringBuilder();

        var index = 0;

        foreach (var word in words)
        {
            if (currentLine.Length + word.Length + 1 > maxWidth)
            {
                if (wrappedText.Length > 0) wrappedText.AppendLine();

                wrappedText.Append(currentLine);
                currentLine.Clear();
            }

            currentLine.Append(word);
            index += word.Length;

            if (text.Length <= index || text[index] != separator) continue;
            currentLine.Append(separator);
            index++;
        }

        if (currentLine.Length <= 0) return wrappedText.ToString();
        if (wrappedText.Length > 0) wrappedText.AppendLine();
        wrappedText.Append(currentLine.ToString().TrimEnd(' '));

        return wrappedText.ToString();
    }
}
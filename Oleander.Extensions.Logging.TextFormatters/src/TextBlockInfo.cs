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

    public Pad Pad { get; private set; } = pad;
    public string Prefix { get; private set; } = preFix;
    public string PostFix { get; private set; } = postFix;

    public int MaxLength { get; private set; }
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
                string.Concat(string.Empty.PadLeft(this.Prefix.Length), line, string.Empty.PadRight(this.PostFix.Length));
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

            if (lines.All(x => x.Length <= maxWidth)) return lines.ToArray();
        }

        return lines.ToArray();
    }

    private static string WordWrap(string text, char separator, int maxWidth)
    {
        if (maxWidth < 1 || text.Length <= maxWidth) return text;

        var words = text.Split(separator);
        if (words.Length < 2) return text;

        var wrappedText = new StringBuilder();
        var currentLine = new StringBuilder();

        foreach (var word in words)
        {
            if (currentLine.Length + word.Length + 1 > maxWidth)
            {
                if (wrappedText.Length > 0) wrappedText.AppendLine();

                wrappedText.Append(currentLine.ToString().TrimEnd(' '));
                currentLine.Clear();
            }

            //currentLine.Append(separator).Append(word);


            currentLine.Append(word);

            if (currentLine.Length + 1 > maxWidth) continue;
            currentLine.Append(separator);






            //if (currentLine.Length > 0)
            //{
            //    currentLine.Append(word).Append(separator);
            //    continue;
            //}

            //if (separator != ' ')
            //{
            //    currentLine.Append(separator).Append(word);
            //}
            //else
            //{
            //    currentLine.Append(separator).Append(word);
            //}




        }

        if (currentLine.Length <= 0) return wrappedText.ToString();
        if (wrappedText.Length > 0) wrappedText.AppendLine();
        wrappedText.Append(currentLine);

        return wrappedText.ToString();
    }
}
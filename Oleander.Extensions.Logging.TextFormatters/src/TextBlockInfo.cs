using System;
using System.Diagnostics;
using System.Linq;

namespace Oleander.Extensions.Logging.TextFormatters;

[DebuggerDisplay("Prefix={Prefix}, PostFix={PostFix}, Pad={Pad}, MaxLength={MaxLength}, Line.Length={Lines.Length}")]
public class TextBlockInfo
{
    public TextBlockInfo(Pad pad, string preFix = "", string postFix = "")
    {
        this.Pad = pad;
        this.MaxLength = 0;
        this.Prefix = preFix;
        this.PostFix = postFix;
        this.Lines = Array.Empty<string>();
    }

    public void SetValue(object? value)
    {
        if (value == null) return;
        var valueAsString = value.ToString() ?? string.Empty;

        this.Lines = valueAsString.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        var maxLength = this.Lines.Max(x => x.Length);

        if (maxLength > this.MaxLength) this.MaxLength = maxLength;
    }

    public Pad Pad { get; init; }
    public int MaxLength { get; private set; }
    public string[] Lines { get; private set; }
    public string Prefix { get; init; }
    public string PostFix { get; init; }


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
}
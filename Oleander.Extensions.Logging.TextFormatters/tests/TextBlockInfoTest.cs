using System;
using Xunit;

namespace Oleander.Extensions.Logging.TextFormatters.Tests;

public class TextBlockInfoTest
{
    [Fact]
    public void TestBlockTextFormatterNullValue()
    {
        var textBlockInfo = new TextBlockInfo(Pad.PadRight, "Message:", "|");

        textBlockInfo.SetValue(null);

        Assert.Equal(0, textBlockInfo.MaxLength);
        Assert.Empty(textBlockInfo.Lines);
        Assert.Equal("Message:|", textBlockInfo[0]);
        Assert.Equal("        |", textBlockInfo[1]);
        Assert.Equal("        |", textBlockInfo[2]);
        Assert.Equal("        |", textBlockInfo[3]);
    }

    [Fact]
    public void TestBlockTextFormatterPreFixAndNullValue()
    {
        var textBlockInfo = new TextBlockInfo(Pad.PadRight, "Message:", "|");

        textBlockInfo.SetValue(null);

        Assert.Equal(0, textBlockInfo.MaxLength);
        Assert.Empty(textBlockInfo.Lines);
        Assert.Equal("Message:|", textBlockInfo[0]);
        Assert.Equal("        |", textBlockInfo[1]);
        Assert.Equal("        |", textBlockInfo[2]);
        Assert.Equal("        |", textBlockInfo[3]);
    }


    [Fact]
    public void TestBlockTextFormatterEmptyValue()
    {
        var textBlockInfo = new TextBlockInfo(Pad.PadRight, "Message:", "|");

        textBlockInfo.SetValue(string.Empty);

        Assert.Equal(0, textBlockInfo.MaxLength);
        Assert.Single(textBlockInfo.Lines);
        Assert.Equal("Message:|", textBlockInfo[0]);
        Assert.Equal("        |", textBlockInfo[1]);
        Assert.Equal("        |", textBlockInfo[2]);
        Assert.Equal("        |", textBlockInfo[3]);
    }

    [Fact]
    public void TestBlockTextFormatterSingleLine()
    {
        var textBlockInfo = new TextBlockInfo(Pad.PadRight, "Message:", "|");

        textBlockInfo.SetValue("This is a test message.");

        Assert.Equal(23, textBlockInfo.MaxLength);
        Assert.Single(textBlockInfo.Lines);
        Assert.Equal("Message:This is a test message.|", textBlockInfo[0]);
        Assert.Equal("                               |", textBlockInfo[1]);
        Assert.Equal("                               |", textBlockInfo[2]);
        Assert.Equal("                               |", textBlockInfo[3]);
    }

    [Fact]
    public void TestBlockTextFormatterTwoLinesPadRight()
    {
        var textBlockInfo = new TextBlockInfo(Pad.PadRight, "Message:", "|");

        textBlockInfo.SetValue($"This is a test message.{Environment.NewLine}   This is line two.");

        Assert.Equal(23, textBlockInfo.MaxLength);
        Assert.Equal(2, textBlockInfo.Lines.Length);
        Assert.Equal("Message:This is a test message.|", textBlockInfo[0]);
        Assert.Equal("           This is line two.   |", textBlockInfo[1]);
        Assert.Equal("                               |", textBlockInfo[2]);
        Assert.Equal("                               |", textBlockInfo[3]);
    }

    [Fact]
    public void TestBlockTextFormatterTwoLinesPadLeft()
    {
        var textBlockInfo = new TextBlockInfo(Pad.PadLeft, "Message:", "|");

        textBlockInfo.SetValue($"This is a test message.{Environment.NewLine}   This is line two.");

        Assert.Equal(23, textBlockInfo.MaxLength);
        Assert.Equal(2, textBlockInfo.Lines.Length);
        Assert.Equal("Message:This is a test message.|", textBlockInfo[0]);
        Assert.Equal("              This is line two.|", textBlockInfo[1]);
        Assert.Equal("                               |", textBlockInfo[2]);
        Assert.Equal("                               |", textBlockInfo[3]);
    }

    [Fact]
    public void TestWrdWordWrapPadRight()
    {
        var textBlockInfo = new TextBlockInfo(Pad.PadRight, "Message:", "|") { WordWrapWidth = 10 };

        textBlockInfo.SetValue("This is a test message. This is line two.");

        Assert.Equal(9, textBlockInfo.MaxLength);
        Assert.Equal(5, textBlockInfo.Lines.Length);

        Assert.Equal("Message:This is a|", textBlockInfo[0]);
        Assert.Equal("        test     |", textBlockInfo[1]);
        Assert.Equal("        message. |", textBlockInfo[2]);
        Assert.Equal("        This is  |", textBlockInfo[3]);
        Assert.Equal("        line two.|", textBlockInfo[4]);
    }

    [Fact]
    public void TestWrdWordWrapPadPadLeft()
    {
        var textBlockInfo = new TextBlockInfo(Pad.PadLeft, "Message:", "|") { WordWrapWidth = 10 };

        textBlockInfo.SetValue("This is a test message. This is line two.");

        Assert.Equal(9, textBlockInfo.MaxLength);
        Assert.Equal(5, textBlockInfo.Lines.Length);

        Assert.Equal("Message:This is a|", textBlockInfo[0]);
        Assert.Equal("test             |", textBlockInfo[1]);
        Assert.Equal("message.         |", textBlockInfo[2]);
        Assert.Equal("This is          |", textBlockInfo[3]);
        Assert.Equal("line two.        |", textBlockInfo[4]);
    }
}
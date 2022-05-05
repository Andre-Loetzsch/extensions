using System;
using Xunit;

namespace Tentakel.Extensions.Logging.TextFormatters.Tests;

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
        Assert.Equal("         ", textBlockInfo[1]);
        Assert.Equal("         ", textBlockInfo[2]);
        Assert.Equal("         ", textBlockInfo[3]);
    }

    [Fact]
    public void TestBlockTextFormatterPreFixAndNullValue()
    {
        var textBlockInfo = new TextBlockInfo(Pad.PadRight, "Message:", "|");

        textBlockInfo.SetValue(null);

        Assert.Equal(0, textBlockInfo.MaxLength);
        Assert.Empty(textBlockInfo.Lines);
        Assert.Equal("Message:|", textBlockInfo[0]);
        Assert.Equal("         ", textBlockInfo[1]);
        Assert.Equal("         ", textBlockInfo[2]);
        Assert.Equal("         ", textBlockInfo[3]);
    }


    [Fact]
    public void TestBlockTextFormatterEmptyValue()
    {
        var textBlockInfo = new TextBlockInfo(Pad.PadRight, "Message:", "|");

        textBlockInfo.SetValue(string.Empty);

        Assert.Equal(0, textBlockInfo.MaxLength);
        Assert.Single(textBlockInfo.Lines);
        Assert.Equal("Message:|", textBlockInfo[0]);
        Assert.Equal("         ", textBlockInfo[1]);
        Assert.Equal("         ", textBlockInfo[2]);
        Assert.Equal("         ", textBlockInfo[3]);
    }

    [Fact]
    public void TestBlockTextFormatterSingleLine()
    {
        var textBlockInfo = new TextBlockInfo(Pad.PadRight, "Message:", "|");

        textBlockInfo.SetValue("This is a test message.");

        Assert.Equal(23, textBlockInfo.MaxLength);
        Assert.Single(textBlockInfo.Lines);
        Assert.Equal("Message:This is a test message.|", textBlockInfo[0]);
        Assert.Equal("                                ", textBlockInfo[1]);
        Assert.Equal("                                ", textBlockInfo[2]);
        Assert.Equal("                                ", textBlockInfo[3]);
    }

    [Fact]
    public void TestBlockTextFormatterTwoLinesPadRight()
    {
        var textBlockInfo = new TextBlockInfo(Pad.PadRight, "Message:", "|");

        textBlockInfo.SetValue($"This is a test message.{Environment.NewLine}   This is line two.");

        Assert.Equal(23, textBlockInfo.MaxLength);
        Assert.Equal(2, textBlockInfo.Lines.Length);
        Assert.Equal("Message:This is a test message.|", textBlockInfo[0]);
        Assert.Equal("           This is line two.    ", textBlockInfo[1]);
        Assert.Equal("                                ", textBlockInfo[2]);
        Assert.Equal("                                ", textBlockInfo[3]);
    }

    [Fact]
    public void TestBlockTextFormatterTwoLinesPadLeft()
    {
        var textBlockInfo = new TextBlockInfo(Pad.PadLeft, "Message:", "|");

        textBlockInfo.SetValue($"This is a test message.{Environment.NewLine}   This is line two.");

        Assert.Equal(23, textBlockInfo.MaxLength);
        Assert.Equal(2, textBlockInfo.Lines.Length);
        Assert.Equal("Message:This is a test message.|", textBlockInfo[0]);
        Assert.Equal("              This is line two. ", textBlockInfo[1]);
        Assert.Equal("                                ", textBlockInfo[2]);
        Assert.Equal("                                ", textBlockInfo[3]);
    }
}
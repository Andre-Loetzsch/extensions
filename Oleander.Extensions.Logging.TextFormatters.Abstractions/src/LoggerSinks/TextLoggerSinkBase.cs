﻿using System;
using Oleander.Extensions.Logging.LoggerSinks;

namespace Oleander.Extensions.Logging.TextFormatters.Abstractions.LoggerSinks;

public abstract class TextLoggerSinkBase : LoggerSinkBase
{
    public ITextFormatter TextFormatter { get; set; } = new DefaultTextFormatter();

    public string? TextFormatterType { get; set; }

    protected void CreateTextFormatter()
    {
        if (this.TextFormatterType == null) return;

        var type = Type.GetType(this.TextFormatterType);
        
        if (type == null) return;
        if (Activator.CreateInstance(type) is not ITextFormatter textFormatter) return;  

        this.TextFormatter = textFormatter;
    }
}
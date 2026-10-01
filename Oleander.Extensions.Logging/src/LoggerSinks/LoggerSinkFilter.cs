using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oleander.Extensions.Logging.LoggerSinks;

internal class LoggerSinkFilter
{
    private readonly LoggerCategoryFilter _filter;
    private readonly Dictionary<string, bool> _cache = new(StringComparer.OrdinalIgnoreCase);

    public LoggerSinkFilter(ILoggerSink loggerSink)
    {
        this.LoggerSink = loggerSink ?? throw new ArgumentNullException(nameof(loggerSink));
        this._filter = new([.. loggerSink.Categories.Select(c => new LoggerCategoryFilterRule(c))]);
    }

    public ILoggerSink LoggerSink { get; } 

    public bool IsEnabled(string category, LogLevel level)
    {
        if (!this.LoggerSink.IsEnabled(level)) return false;

        if (!this._cache.TryGetValue(category, out var isEnabled))
        {
            isEnabled = this._filter.IsEnabled(category);
            this._cache[category] = isEnabled;
        }

        return isEnabled;
    }
}
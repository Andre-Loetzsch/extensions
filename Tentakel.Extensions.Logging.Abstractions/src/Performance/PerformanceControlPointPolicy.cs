using System;
using Microsoft.Extensions.Logging;

namespace Tentakel.Extensions.Logging.Abstractions.Performance
{
    public class PerformanceControlPointPolicy
    {
        public bool IsEnabled { get; set; }
        public string? Name { get; set; }
        public TimeSpan TimeLimit { get; set; }
        public LogLevel LogLevel { get; set; }
    }
}
using System;
using Microsoft.Extensions.Logging;

namespace Tentakel.Extensions.Logging.Performance
{
    public class PerformanceControlPointPolicy
    {
        public PerformanceControlPointPolicy()
        {
        }

        public PerformanceControlPointPolicy(string name)
        {
            this.Name = name;
        }

        public bool IsEnabled { get; set; }
        public string Name { get; set; }
        public TimeSpan TimeLimit { get; set; }
        public LogLevel LogLevel { get; set; }
    }


    public class PerformanceControlPointPolicyOption// : IOptionsMonitor<PerformanceControlPointPolicy>
    {
        public PerformanceControlPointPolicy[] Policies { get; set; } = Array.Empty<PerformanceControlPointPolicy>();
        //public PerformanceControlPointPolicy Get(string name)
        //{
        //    return this.Policies.FirstOrDefault(x => x.Name == name);
        //}

        //public IDisposable OnChange(Action<PerformanceControlPointPolicy, string> listener)
        //{
        //    Console.WriteLine($"{listener}");

        //    return null;
        //}

        //public PerformanceControlPointPolicy CurrentValue { get; }
    }


}
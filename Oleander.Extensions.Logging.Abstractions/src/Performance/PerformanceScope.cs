using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Oleander.Extensions.Logging.Abstractions.Performance
{
    public class PerformanceScope(ILogger logger, IEnumerable<PerformanceControlPointPolicy> policies, IDisposable? innerScope)
        : IDisposable
    {
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IEnumerable<PerformanceControlPointPolicy> _policies = policies ?? throw new ArgumentNullException(nameof(policies));
        private DateTime _startDateTime = DateTime.Now;

        public void SetPerformanceControlPoint(string policyName)
        {
            var timeSpan = DateTime.Now - this._startDateTime;
            this._startDateTime = DateTime.Now;

            var policy = this._policies.FirstOrDefault(x => x.Name == policyName);
            if (policy == null) return;

            if (policy.TimeLimit >= timeSpan) return;
            this._logger.Log(policy.LogLevel, 0, null, "The time limit has been exceeded. Policy name: {policyName} Time limit: {timeLimit} Needed time: {neededTime}", policy.Name, policy.TimeLimit, timeSpan);
        }

        #region IDisposable

        public void Dispose()
        {
            innerScope?.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
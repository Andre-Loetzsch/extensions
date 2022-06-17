﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Tentakel.Extensions.Logging.Abstractions.Performance
{
    public class PerformanceScope : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<PerformanceControlPointPolicy> _policies;
        private readonly IDisposable _innerScope;
        private DateTime _startDateTime = DateTime.Now;

        public PerformanceScope(ILogger logger, IEnumerable<PerformanceControlPointPolicy> policies, IDisposable innerScope)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._policies = policies ?? throw new ArgumentNullException(nameof(policies));
            this._innerScope = innerScope ?? throw new ArgumentNullException(nameof(innerScope));
        }

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

        private bool _disposed;

        ~PerformanceScope()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (this._disposed) return;
            this._disposed = true;

            this._innerScope?.Dispose();

            if (!disposing) return;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
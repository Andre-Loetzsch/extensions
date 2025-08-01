﻿using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace Oleander.Extensions.Logging.SourceHelper
{
    internal class SourceCache
    {
        [DebuggerDisplay("Count:{Count} Source:{Source}")]
        private class SourceCacheItem(string source)
        {
            public string Source { get; } = source;
            public int Count { get; set; } = 1;
        }

        private readonly ConcurrentDictionary<string, SourceCacheItem> _cache = new();

        public void AddSource(string originalFormat, string source)
        {
            this.CleanUp(1000);
            this._cache[originalFormat] = new(source);
            //Debug.WriteLine($"Add cache item: {originalFormat} Count:{this._cache.Count}");
        }

        public bool TryGetSource(string originalFormat, out string? source)
        {
            source = null;
            if (!this._cache.TryGetValue(originalFormat, out var item)) return false;

            source = item.Source;
            item.Count++;
            return true;
        }

        private void CleanUp(int maxSize)
        {
            if (this._cache.Count <= maxSize) return;
            var keys = this._cache.OrderBy(x => x.Value.Count).Select(x => x.Key).ToList();

            foreach (var key in keys)
            {
                if (this._cache.Count <= maxSize) return;

                //Debug.WriteLine($"Remove cache item: {this._cache[key].Count} - {key}");
                this._cache.TryRemove(key, out _);
            }
        }
    }
}
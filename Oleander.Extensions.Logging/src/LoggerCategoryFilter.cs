using System;

namespace Oleander.Extensions.Logging;

internal sealed class LoggerCategoryFilter(LoggerCategoryFilterRule[] rules)
{
    public bool IsEnabled(string category)
    {
        LoggerCategoryFilterRule? bestMatch = null;
        var bestScore = -1;

        foreach (var rule in rules)
        {
            if (!Matches(category, rule, out var score)) continue;
            if (score <= bestScore) continue;
            bestScore = score;
            bestMatch = rule;
        }

        return bestMatch != null;
    }

    private static bool Matches(string category, LoggerCategoryFilterRule rule, out int score)
    {
        score = -1;

        // Microsoft.* -> StartsWith
        if (rule.IsSimplePrefixRule)
        {
            if (!category.StartsWith(rule.Prefix, StringComparison.OrdinalIgnoreCase)) return false;
            score = rule.Prefix.Length;
            return true;

        }

        // Kein Wildcard -> Microsoft
        if (!rule.HasWildcards)
        {
            if (!category.Equals(rule.Pattern, StringComparison.OrdinalIgnoreCase)) return false;
            score = rule.Pattern.Length;
            return true;
        }

        if (!WildcardMatch(category, rule.Pattern)) return false;
        score = rule.Pattern.Length;
        return true;

    }

    private static bool WildcardMatch(ReadOnlySpan<char> text, ReadOnlySpan<char> pattern)
    {
        var t = 0;
        var p = 0;

        var star = -1;
        var match = 0;

        while (t < text.Length)
        {
            if (p < pattern.Length &&
                (pattern[p] == '?' ||
                 char.ToUpperInvariant(text[t])
                 == char.ToUpperInvariant(pattern[p])))
            {
                t++;
                p++;
            }
            else if (p < pattern.Length && pattern[p] == '*')
            {
                star = p++;
                match = t;
            }
            else if (star >= 0)
            {
                p = star + 1;
                t = ++match;
            }
            else
            {
                return false;
            }
        }

        while (p < pattern.Length && pattern[p] == '*')
        {
            p++;
        }

        return p == pattern.Length;
    }
}
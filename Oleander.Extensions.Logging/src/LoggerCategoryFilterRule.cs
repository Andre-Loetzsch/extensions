using System.Linq;

namespace Oleander.Extensions.Logging;

internal sealed class LoggerCategoryFilterRule(string pattern)
{
    public string Pattern { get; } = pattern;

    public bool HasWildcards => this.Pattern.IndexOfAny(['*', '?']) >= 0;

    public bool IsSimplePrefixRule => this.Pattern.EndsWith("*") && 
                                      this.Pattern.Count(c => c is '*' or '?') == 1;

    public string Prefix => this.IsSimplePrefixRule
        ? this.Pattern.Substring(0, this.Pattern.Length - 1)
        : this.Pattern;
}
using Nestify.Abstractions;
using System;
using System.Collections.Generic;

namespace Nestify.Services;

internal class AutoNestRuleEngine(IReadOnlyList<INestingRule> rules) : IAutoNestRuleEngine
{
    private readonly IReadOnlyList<INestingRule> _rules = rules ?? throw new ArgumentNullException(nameof(rules));

    public string FindParent(string fileName, HashSet<string> availableFiles)
    {
        foreach (var rule in _rules)
        {
            if (!rule.CanHandle(fileName)) continue;
            var parent = rule.FindParent(fileName, availableFiles);
            if (parent == null) continue;
            return parent;
        }

        return null;
    }
}
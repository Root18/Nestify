using Nestify.Abstractions;
using System;
using System.Collections.Generic;

namespace Nestify.Services
{
    internal class AutoNestRuleEngine : IAutoNestRuleEngine
    {
        private readonly IReadOnlyList<INestingRule> _rules;

        public AutoNestRuleEngine(IReadOnlyList<INestingRule> rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public string FindParent(string fileName, HashSet<string> availableFiles)
        {
            foreach (var rule in _rules)
            {
                if (rule.CanHandle(fileName))
                {
                    string parent = rule.FindParent(fileName, availableFiles);
                    if (parent != null)
                        return parent;
                }
            }

            return null;
        }
    }
}

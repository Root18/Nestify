using System.Collections.Generic;

namespace Nestify.Abstractions
{
    internal interface IAutoNestRuleEngine
    {
        string FindParent(string fileName, HashSet<string> availableFiles);
    }
}

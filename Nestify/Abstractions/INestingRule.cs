using System.Collections.Generic;

namespace Nestify.Abstractions;

internal interface INestingRule
{
    bool CanHandle(string fileName);
    string FindParent(string fileName, HashSet<string> availableFiles);
}
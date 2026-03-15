using Nestify.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nestify.Rules;

internal class MarkdownNestingRule : INestingRule
{
    public bool CanHandle(string fileName)
    {
        return fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase);
    }

    public string FindParent(string fileName, HashSet<string> availableFiles)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

        // Try .cs first (C# documentation)
        var csharpParent = nameWithoutExt + ".cs";
        if (availableFiles.Contains(csharpParent))
            return csharpParent;

        // Try .js (JavaScript documentation)
        var jsParent = nameWithoutExt + ".js";
        if (availableFiles.Contains(jsParent))
            return jsParent;

        return null;
    }
}

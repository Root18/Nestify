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

    // Code files a markdown document can be nested under, in preference order.
    private static readonly string[] ParentExtensions = [".cs", ".vb", ".ts", ".tsx", ".js", ".jsx"];

    public string FindParent(string fileName, HashSet<string> availableFiles)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

        foreach (var extension in ParentExtensions)
        {
            var candidate = nameWithoutExt + extension;
            if (availableFiles.Contains(candidate))
                return candidate;
        }

        return null;
    }
}

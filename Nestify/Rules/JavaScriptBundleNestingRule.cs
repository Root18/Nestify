using Nestify.Abstractions;
using System;
using System.Collections.Generic;

namespace Nestify.Rules;

internal class JavaScriptBundleNestingRule : INestingRule
{
    public bool CanHandle(string fileName)
    {
        return fileName.EndsWith(".bundle.js", StringComparison.OrdinalIgnoreCase)
               && !fileName.EndsWith(".bundle.min.js", StringComparison.OrdinalIgnoreCase);
    }

    public string FindParent(string fileName, HashSet<string> availableFiles)
    {
        // foo.bundle.js → foo.js
        var parent = fileName.Substring(0, fileName.Length - ".bundle.js".Length) + ".js";

        return availableFiles.Contains(parent) ? parent : null;
    }
}
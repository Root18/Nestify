using Nestify.Abstractions;
using System;
using System.Collections.Generic;

namespace Nestify.Rules;

internal class JavaScriptBundleMinNestingRule : INestingRule
{
    public bool CanHandle(string fileName)
    {
        return fileName.EndsWith(".bundle.min.js", StringComparison.OrdinalIgnoreCase);
    }

    public string FindParent(string fileName, HashSet<string> availableFiles)
    {
        // foo.bundle.min.js → foo.bundle.js
        var parent = fileName.Substring(0, fileName.Length - ".bundle.min.js".Length) + ".bundle.js";

        return availableFiles.Contains(parent) ? parent : null;
    }
}
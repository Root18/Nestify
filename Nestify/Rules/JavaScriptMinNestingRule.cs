using Nestify.Abstractions;
using System;
using System.Collections.Generic;

namespace Nestify.Rules
{
    internal class JavaScriptMinNestingRule : INestingRule
    {
        public bool CanHandle(string fileName)
        {
            return fileName.EndsWith(".min.js", StringComparison.OrdinalIgnoreCase)
                && !fileName.EndsWith(".bundle.min.js", StringComparison.OrdinalIgnoreCase);
        }

        public string FindParent(string fileName, HashSet<string> availableFiles)
        {
            // foo.min.js → foo.js
            string parent = fileName.Substring(0, fileName.Length - ".min.js".Length) + ".js";

            if (availableFiles.Contains(parent))
                return parent;

            return null;
        }
    }
}

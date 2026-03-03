using Nestify.Abstractions;
using System;
using System.Collections.Generic;

namespace Nestify.Rules
{
    internal class JavaScriptBundleMinNestingRule : INestingRule
    {
        public bool CanHandle(string fileName)
        {
            return fileName.EndsWith(".bundle.min.js", StringComparison.OrdinalIgnoreCase);
        }

        public string FindParent(string fileName, HashSet<string> availableFiles)
        {
            // foo.bundle.min.js → foo.bundle.js
            string parent = fileName.Substring(0, fileName.Length - ".bundle.min.js".Length) + ".bundle.js";

            if (availableFiles.Contains(parent))
                return parent;

            return null;
        }
    }
}

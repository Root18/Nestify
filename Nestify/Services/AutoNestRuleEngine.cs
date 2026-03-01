using System;
using System.Collections.Generic;
using System.IO;

namespace Nestify.Services
{
    internal static class AutoNestRuleEngine
    {
        public static string FindParent(string fileName, HashSet<string> availableFiles)
        {
            if (fileName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                return FindCSharpParent(fileName, availableFiles);
            }

            if (fileName.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
            {
                return FindJavaScriptParent(fileName, availableFiles);
            }

            return null;
        }

        private static string FindCSharpParent(string fileName, HashSet<string> availableFiles)
        {
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

            // Don't nest interfaces themselves (files starting with I + uppercase letter)
            if (nameWithoutExt.Length > 1 && nameWithoutExt[0] == 'I' && char.IsUpper(nameWithoutExt[1]))
                return null;

            // ClassName.cs → IClassName.cs
            string interfaceName = "I" + nameWithoutExt + ".cs";

            if (availableFiles.Contains(interfaceName))
                return interfaceName;

            return null;
        }

        private static string FindJavaScriptParent(string fileName, HashSet<string> availableFiles)
        {
            // foo.bundle.min.js → foo.bundle.js
            if (fileName.EndsWith(".bundle.min.js", StringComparison.OrdinalIgnoreCase))
            {
                string parent = fileName.Substring(0, fileName.Length - ".bundle.min.js".Length) + ".bundle.js";
                if (availableFiles.Contains(parent))
                    return parent;
            }

            // foo.bundle.js → foo.js
            if (fileName.EndsWith(".bundle.js", StringComparison.OrdinalIgnoreCase))
            {
                string parent = fileName.Substring(0, fileName.Length - ".bundle.js".Length) + ".js";
                if (availableFiles.Contains(parent))
                    return parent;
            }

            // foo.min.js → foo.js
            if (fileName.EndsWith(".min.js", StringComparison.OrdinalIgnoreCase)
                && !fileName.EndsWith(".bundle.min.js", StringComparison.OrdinalIgnoreCase))
            {
                string parent = fileName.Substring(0, fileName.Length - ".min.js".Length) + ".js";
                if (availableFiles.Contains(parent))
                    return parent;
            }

            return null;
        }
    }
}

using System;
using System.IO;

namespace Nestify.Utilities
{
    internal static class PathUtilities
    {
        public static string GetRelativePath(string basePath, string fullPath)
        {
            if (!basePath.EndsWith("\\", StringComparison.Ordinal))
                basePath += "\\";

            if (fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(basePath.Length);
            }

            return Path.GetFileName(fullPath);
        }
    }
}

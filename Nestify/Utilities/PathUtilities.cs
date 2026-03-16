using System;
using System.IO;

namespace Nestify.Utilities;

internal static class PathUtilities
{
    public static string GetRelativePath(string basePath, string fullPath)
    {
        if (!basePath.EndsWith("\\", StringComparison.Ordinal))
            basePath += "\\";

        return fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase)
            ? fullPath.Substring(basePath.Length)
            : Path.GetFileName(fullPath);
    }
}
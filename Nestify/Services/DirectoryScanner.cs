using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Nestify.Abstractions;
using Nestify.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nestify.Services;

internal class DirectoryScanner(IAutoNestRuleEngine ruleEngine, IFileNestingService nestingService)
    : IDirectoryScanner
{
    private readonly IAutoNestRuleEngine _ruleEngine =
        ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));

    private readonly IFileNestingService _nestingService =
        nestingService ?? throw new ArgumentNullException(nameof(nestingService));

    private static readonly HashSet<string> ExcludedDirectories =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "bin", "obj", "node_modules",
            "dist", "build", "out",
            ".next", ".nuxt"
        };

    public int ScanAndNest(string directory, IVsHierarchy hierarchy, IVsBuildPropertyStorage storage)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (hierarchy == null)
            return 0;

        var nestedCount = 0;
        ProcessDirectory(directory, hierarchy, storage, ref nestedCount);
        return nestedCount;
    }

    private void ProcessDirectory(string directory, IVsHierarchy hierarchy, IVsBuildPropertyStorage storage,
        ref int nestedCount)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var files = Directory.GetFiles(directory);
        var fileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in files)
        {
            fileNames.Add(Path.GetFileName(f));
        }

        foreach (var filePath in files)
        {
            var fileName = Path.GetFileName(filePath);
            var parentName = _ruleEngine.FindParent(fileName, fileNames);

            if (parentName == null)
                continue;

            var childItem = VsHierarchyHelper.GetProjectItem(hierarchy, filePath);
            if (childItem == null || ProjectItemHelper.IsNestedUnderFile(childItem))
                continue;

            var parentItem = VsHierarchyHelper.GetProjectItem(hierarchy, Path.Combine(directory, parentName));
            if (parentItem == null) continue;

            _nestingService.NestFile(childItem, parentItem, hierarchy, storage);
            nestedCount++;
        }

        foreach (var subDir in Directory.GetDirectories(directory))
        {
            var dirName = Path.GetFileName(subDir);
            if (dirName.StartsWith(".", StringComparison.Ordinal) ||
                ExcludedDirectories.Contains(dirName))
                continue;

            ProcessDirectory(subDir, hierarchy, storage, ref nestedCount);
        }
    }
}
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Nestify.Abstractions;
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
            "bin", "obj", "node_modules"
        };

    public int ScanAndNest(string directory, IVsHierarchy hierarchy, IVsBuildPropertyStorage storage)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
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

            if (hierarchy.ParseCanonicalName(filePath, out var itemId) != 0 || itemId == 0)
                continue;

            hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out var itemObj);
            if (itemObj is not ProjectItem childItem || IsAlreadyNested(childItem))
                continue;

            storage.GetItemAttribute(itemId, "NestifyExclude", out var excludeValue);
            if (string.Equals(excludeValue, "true", StringComparison.OrdinalIgnoreCase))
                continue;

            var parentFullPath = Path.Combine(directory, parentName);
            if (hierarchy.ParseCanonicalName(parentFullPath, out uint parentId) != 0 || parentId == 0) continue;
            hierarchy.GetProperty(parentId, (int)__VSHPROPID.VSHPROPID_ExtObject, out object parentObj);
            if (parentObj is not ProjectItem parentItem) continue;
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

    private static bool IsAlreadyNested(ProjectItem item)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        try
        {
            var parent = item.Collection.Parent as ProjectItem;
            return parent != null && string.Equals(parent.Kind,
                EnvDTE.Constants.vsProjectItemKindPhysicalFile,
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
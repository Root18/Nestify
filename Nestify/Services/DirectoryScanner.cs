using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Nestify.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nestify.Services
{
    internal class DirectoryScanner : IDirectoryScanner
    {
        private readonly IAutoNestRuleEngine _ruleEngine;
        private readonly IFileNestingService _nestingService;

        private static readonly HashSet<string> ExcludedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "bin", "obj", "node_modules"
        };

        public DirectoryScanner(IAutoNestRuleEngine ruleEngine, IFileNestingService nestingService)
        {
            _ruleEngine = ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));
            _nestingService = nestingService ?? throw new ArgumentNullException(nameof(nestingService));
        }

        public int ScanAndNest(string directory, IVsHierarchy hierarchy, IVsBuildPropertyStorage storage)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            int nestedCount = 0;
            ProcessDirectory(directory, hierarchy, storage, ref nestedCount);
            return nestedCount;
        }

        private void ProcessDirectory(string directory, IVsHierarchy hierarchy, IVsBuildPropertyStorage storage, ref int nestedCount)
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
                string fileName = Path.GetFileName(filePath);
                string parentName = _ruleEngine.FindParent(fileName, fileNames);

                if (parentName == null)
                    continue;

                if (hierarchy.ParseCanonicalName(filePath, out uint itemId) != 0 || itemId == 0)
                    continue;

                storage.GetItemAttribute(itemId, "DependentUpon", out string existingParent);
                if (!string.IsNullOrEmpty(existingParent))
                    continue;

                _nestingService.NestFile(storage, itemId, parentName);
                nestedCount++;
            }

            foreach (var subDir in Directory.GetDirectories(directory))
            {
                string dirName = Path.GetFileName(subDir);
                if (dirName.StartsWith(".", StringComparison.Ordinal) ||
                    ExcludedDirectories.Contains(dirName))
                    continue;

                ProcessDirectory(subDir, hierarchy, storage, ref nestedCount);
            }
        }
    }
}

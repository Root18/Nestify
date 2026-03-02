using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Nestify.Abstractions;
using System.Collections.Generic;

namespace Nestify.Services
{
    internal class FileNestingService : IFileNestingService
    {
        public void NestFile(IVsBuildPropertyStorage storage, uint itemId, string parentFileName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            storage.SetItemAttribute(itemId, "DependentUpon", parentFileName);
        }

        public void UnnestFile(ProjectItem childItem, IVsHierarchy hierarchy, IVsBuildPropertyStorage storage)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string filePath = childItem.FileNames[1];

            var parentFile = childItem.Collection.Parent as ProjectItem;
            if (parentFile == null) return;

            var descendants = new List<(string filePath, string parentFileName)>();
            CollectDescendants(childItem, descendants);

            ProjectItems targetCollection = parentFile.Collection;

            childItem.Remove();

            targetCollection.AddFromFile(filePath);

            foreach (var desc in descendants)
            {
                targetCollection.AddFromFile(desc.filePath);

                if (hierarchy.ParseCanonicalName(desc.filePath, out uint itemId) == 0 && itemId != 0)
                {
                    storage.SetItemAttribute(itemId, "DependentUpon", desc.parentFileName);
                }
            }
        }

        private static void CollectDescendants(ProjectItem item, List<(string filePath, string parentFileName)> descendants)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (item.ProjectItems == null || item.ProjectItems.Count == 0) return;

            foreach (ProjectItem child in item.ProjectItems)
            {
                descendants.Add((child.FileNames[1], item.Name));
                CollectDescendants(child, descendants);
            }
        }
    }
}

using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;

namespace Nestify.Services
{
    internal static class FileNestingService
    {
        public static void NestFile(IVsBuildPropertyStorage storage, uint itemId, string parentFileName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            storage.SetItemAttribute(itemId, "DependentUpon", parentFileName);
        }

        public static void UnnestFile(ProjectItem childItem, IVsHierarchy hierarchy, IVsBuildPropertyStorage storage)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string filePath = childItem.FileNames[1];

            var parentFile = childItem.Collection.Parent as ProjectItem;
            if (parentFile == null) return;

            // Collect all nested descendants before removing (they'd be lost otherwise)
            var descendants = new List<(string filePath, string parentFileName)>();
            CollectDescendants(childItem, descendants);

            // Get the folder-level collection where the parent file lives
            ProjectItems targetCollection = parentFile.Collection;

            // Remove from nested position (keeps files on disk but removes from project tree)
            childItem.Remove();

            // Re-add the unnested file at the same folder level as the former parent
            targetCollection.AddFromFile(filePath);

            // Re-add all descendants and restore their nesting via DependentUpon
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

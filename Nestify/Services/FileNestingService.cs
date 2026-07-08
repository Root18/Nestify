using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Nestify.Abstractions;
using Nestify.Utilities;
using System;
using System.IO;

namespace Nestify.Services;

internal class FileNestingService : IFileNestingService
{
    public void NestFile(ProjectItem childItem, ProjectItem parentItem, IVsHierarchy hierarchy,
        IVsBuildPropertyStorage storage)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var childPath = childItem.FileNames[1];
        var projectPath = childItem.ContainingProject?.FullName;

        // Forcing the None item type is only safe for files that never take part in the
        // build (markdown docs). Code files must keep their existing build action, otherwise
        // nesting a .cs file would silently exclude it from compilation.
        var enforceNoneItemType = IsCSharpProject(projectPath) && IsMarkdownFile(childPath);

        var applied = TrySetDependentUponViaDte(childItem, parentItem.Name, enforceNoneItemType);

        if (!applied &&
            storage != null &&
            VsHierarchyHelper.TryGetItemId(hierarchy, childPath, out var itemId))
        {
            applied = SetDependentUponAttribute(storage, itemId, parentItem.Name);
        }

        // Legacy project systems (non-SDK csproj, njsproj, ...) don't re-render Solution
        // Explorer on a metadata-only change until the project is reloaded. Re-adding the
        // file under its parent makes the project system move the node immediately.
        // Project systems that already refreshed (CPS/SDK) skip this.
        var displayed = IsDisplayedUnder(childItem, parentItem);
        if (!displayed)
        {
            var addedItem = TryReparentViaAddFromFile(parentItem.ProjectItems, childPath);
            if (addedItem != null && IsDisplayedUnder(addedItem, parentItem))
            {
                displayed = true;
                if (enforceNoneItemType && addedItem.Properties != null)
                {
                    TrySetPropertyValue(addedItem.Properties, "ItemType", "None");
                }
            }
        }

        if (!applied && !displayed)
        {
            ApplyProjectFileFallback(projectPath, childPath, parentItem.Name, isNesting: true);
        }
    }

    public void UnnestFile(ProjectItem childItem, IVsHierarchy hierarchy, IVsBuildPropertyStorage storage)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var filePath = childItem.FileNames[1];
        var projectPath = childItem.ContainingProject?.FullName;

        var applied = TrySetDependentUponViaDte(childItem, string.Empty, enforceNoneItemType: false);

        if (!applied &&
            storage != null &&
            VsHierarchyHelper.TryGetItemId(hierarchy, filePath, out var itemId))
        {
            applied = SetDependentUponAttribute(storage, itemId, string.Empty);
        }

        // Same immediate-refresh problem as nesting: if the tree still shows the item
        // nested, re-add it at the level of the file it was nested under.
        var stillNested = ProjectItemHelper.IsNestedUnderFile(childItem);
        if (stillNested)
        {
            var targetCollection = GetContainingCollectionOfParent(childItem);
            var addedItem = TryReparentViaAddFromFile(targetCollection, filePath);
            if (addedItem != null && !ProjectItemHelper.IsNestedUnderFile(addedItem))
            {
                stillNested = false;
            }
        }

        if (!applied && stillNested)
        {
            ApplyProjectFileFallback(projectPath, filePath, null, isNesting: false);
        }
    }

    /// <summary>True when the tree currently shows <paramref name="child"/> directly under <paramref name="parent"/>.</summary>
    private static bool IsDisplayedUnder(ProjectItem child, ProjectItem parent)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        try
        {
            return child?.Collection?.Parent is ProjectItem displayedParent &&
                   string.Equals(displayedParent.FileNames[1], parent.FileNames[1],
                       StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>The ProjectItems collection (folder or project) that contains the file the item is nested under.</summary>
    private static ProjectItems GetContainingCollectionOfParent(ProjectItem childItem)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        try
        {
            return (childItem?.Collection?.Parent as ProjectItem)?.Collection;
        }
        catch
        {
            return null;
        }
    }

    // AddFromFile with a file that is already part of the project makes the project
    // system re-parent the existing item into the target collection and update the
    // tree immediately. Project systems that don't support it throw or return the
    // item in its old position — callers validate the result.
    private static ProjectItem TryReparentViaAddFromFile(ProjectItems targetCollection, string filePath)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        try
        {
            return targetCollection?.AddFromFile(filePath);
        }
        catch
        {
            return null;
        }
    }

    // Fall back to direct project file manipulation only when project-system update is unavailable.
    private static void ApplyProjectFileFallback(string projectPath, string itemPath, string parentName,
        bool isNesting)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        try
        {
            if (string.IsNullOrEmpty(projectPath) || !File.Exists(projectPath)) return;
            if (!ProjectFileNestingUpdater.ShouldUseDirectProjectFileFallback(projectPath)) return;

            ProjectFileNestingUpdater.UpdateNesting(projectPath, itemPath, parentName, isNesting);
        }
        catch (Exception ex)
        {
            ActivityLog.LogWarning("Nestify", $"Project file fallback failed for '{itemPath}': {ex}");
        }
    }

    private static bool SetDependentUponAttribute(IVsBuildPropertyStorage storage, uint itemId, string value)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        return storage.SetItemAttribute(itemId, "DependentUpon", value) == 0;
    }

    private static bool TrySetDependentUponViaDte(ProjectItem item, string value, bool enforceNoneItemType)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        try
        {
            if (item?.Properties == null)
                return false;

            if (enforceNoneItemType)
            {
                TrySetPropertyValue(item.Properties, "ItemType", "None");
            }

            Property dependentUponProperty;
            try
            {
                dependentUponProperty = item.Properties.Item("DependentUpon");
            }
            catch
            {
                return false;
            }

            if (dependentUponProperty == null)
                return false;

            dependentUponProperty.Value = value;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TrySetPropertyValue(EnvDTE.Properties properties, string propertyName, object value)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        try
        {
            var property = properties.Item(propertyName);
            if (property == null)
                return false;

            property.Value = value;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsCSharpProject(string projectPath)
    {
        if (string.IsNullOrEmpty(projectPath))
            return false;

        return string.Equals(Path.GetExtension(projectPath), ".csproj", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMarkdownFile(string filePath)
    {
        return string.Equals(Path.GetExtension(filePath), ".md", StringComparison.OrdinalIgnoreCase);
    }
}

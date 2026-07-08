using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Nestify.Utilities;

internal static class VsHierarchyHelper
{
    /// <summary>
    /// Resolves the hierarchy item id for a file path. Returns false when the hierarchy is
    /// unavailable or the path does not map to a real item (VSITEMID_NIL).
    /// </summary>
    public static bool TryGetItemId(IVsHierarchy hierarchy, string path, out uint itemId)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        itemId = 0;
        return hierarchy != null &&
               hierarchy.ParseCanonicalName(path, out itemId) == VSConstants.S_OK &&
               itemId != 0 &&
               itemId != (uint)VSConstants.VSITEMID.Nil;
    }

    /// <summary>
    /// Resolves the DTE <see cref="ProjectItem"/> for a file path, or null when it cannot be found.
    /// </summary>
    public static ProjectItem GetProjectItem(IVsHierarchy hierarchy, string path)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (!TryGetItemId(hierarchy, path, out var itemId))
            return null;

        hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out var itemObj);
        return itemObj as ProjectItem;
    }
}

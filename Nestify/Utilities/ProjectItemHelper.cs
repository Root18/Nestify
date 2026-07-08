using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;

namespace Nestify.Utilities;

internal static class ProjectItemHelper
{
    /// <summary>
    /// Returns true when the item is currently displayed nested under another physical file.
    /// </summary>
    public static bool IsNestedUnderFile(ProjectItem item)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        try
        {
            if (item?.Collection?.Parent is not ProjectItem parent)
                return false;

            return string.Equals(parent.Kind,
                EnvDTE.Constants.vsProjectItemKindPhysicalFile,
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}

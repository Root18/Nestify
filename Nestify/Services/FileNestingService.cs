using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Nestify.Abstractions;

namespace Nestify.Services;

internal class FileNestingService : IFileNestingService
{
    public void NestFile(ProjectItem childItem, ProjectItem parentItem, IVsHierarchy hierarchy,
        IVsBuildPropertyStorage storage)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (storage == null) return;

        var childPath = childItem.FileNames[1];
        if (hierarchy.ParseCanonicalName(childPath, out var itemId) != 0 || itemId == 0) return;
        storage.SetItemAttribute(itemId, "DependentUpon", parentItem.Name);
    }

    public void UnnestFile(ProjectItem childItem, IVsHierarchy hierarchy, IVsBuildPropertyStorage storage)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (storage == null) return;

        var filePath = childItem.FileNames[1];
        if (hierarchy.ParseCanonicalName(filePath, out var itemId) != 0 || itemId == 0) return;
        storage.SetItemAttribute(itemId, "DependentUpon", "");
    }
}
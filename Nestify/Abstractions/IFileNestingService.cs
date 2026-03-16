using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;

namespace Nestify.Abstractions;

internal interface IFileNestingService
{
    void NestFile(ProjectItem childItem, ProjectItem parentItem, IVsHierarchy hierarchy,
        IVsBuildPropertyStorage storage);

    void UnnestFile(ProjectItem childItem, IVsHierarchy hierarchy, IVsBuildPropertyStorage storage);
}
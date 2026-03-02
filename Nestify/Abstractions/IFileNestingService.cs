using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;

namespace Nestify.Abstractions
{
    internal interface IFileNestingService
    {
        void NestFile(IVsBuildPropertyStorage storage, uint itemId, string parentFileName);
        void UnnestFile(ProjectItem childItem, IVsHierarchy hierarchy, IVsBuildPropertyStorage storage);
    }
}

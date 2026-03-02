using Microsoft.VisualStudio.Shell.Interop;

namespace Nestify.Abstractions
{
    internal interface IDirectoryScanner
    {
        int ScanAndNest(string directory, IVsHierarchy hierarchy, IVsBuildPropertyStorage storage);
    }
}

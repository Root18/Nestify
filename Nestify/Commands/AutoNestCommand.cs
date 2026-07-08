using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Nestify.Abstractions;
using System;
using System.ComponentModel.Design;
using System.IO;
using Task = System.Threading.Tasks.Task;

namespace Nestify.Commands;

internal sealed class AutoNestCommand
{
    public const int CommandId = 0x0300;
    public static readonly Guid CommandSet = new("e2c2b1a0-3d4e-4f5a-8b6c-7d8e9f0a1b2c");

    private readonly AsyncPackage _package;
    private readonly IDirectoryScanner _directoryScanner;

    private AutoNestCommand(
        AsyncPackage package,
        OleMenuCommandService commandService,
        IDirectoryScanner directoryScanner)
    {
        _package = package ?? throw new ArgumentNullException(nameof(package));
        commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _directoryScanner = directoryScanner ?? throw new ArgumentNullException(nameof(directoryScanner));

        var menuCommandId = new CommandID(CommandSet, CommandId);
        var menuItem = new OleMenuCommand(Execute, menuCommandId);
        menuItem.BeforeQueryStatus += OnBeforeQueryStatus;
        commandService.AddCommand(menuItem);
    }

    public static AutoNestCommand Instance { get; private set; }

    public static async Task InitializeAsync(AsyncPackage package, IDirectoryScanner directoryScanner)
    {
        await package.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
        var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
        Instance = new AutoNestCommand(package, commandService, directoryScanner);
    }

    private void OnBeforeQueryStatus(object sender, EventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var cmd = (OleMenuCommand)sender;
        cmd.Visible = false;

        if (NestifyPackage.Options == null || !NestifyPackage.Options.AutoNestEnabled)
            return;

        var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
        if (dte?.SelectedItems == null || dte.SelectedItems.Count == 0)
        {
            return;
        }


        foreach (SelectedItem item in dte.SelectedItems)
        {
            if (item.Project == null && item.ProjectItem == null) continue;
            cmd.Visible = true;
            return;
        }
    }

    private void Execute(object sender, EventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        try
        {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            if (dte?.SelectedItems == null || dte.SelectedItems.Count == 0)
                return;

            Project project = null;
            string targetDirectory = null;

            foreach (SelectedItem item in dte.SelectedItems)
            {
                if (item.Project != null)
                {
                    project = item.Project;
                    targetDirectory = Path.GetDirectoryName(project.FullName);
                    break;
                }

                if (item.ProjectItem == null) continue;
                project = item.ProjectItem.ContainingProject;
                targetDirectory = string.Equals(item.ProjectItem.Kind,
                    EnvDTE.Constants.vsProjectItemKindPhysicalFolder,
                    StringComparison.OrdinalIgnoreCase)
                    ? item.ProjectItem.FileNames[1]
                    : Path.GetDirectoryName(item.ProjectItem.FileNames[1]);
                break;
            }

            if (project == null || string.IsNullOrEmpty(targetDirectory) || !Directory.Exists(targetDirectory))
                return;

            if (Package.GetGlobalService(typeof(SVsSolution)) is not IVsSolution solution) return;

            if (solution.GetProjectOfUniqueName(project.UniqueName, out var hierarchy) != VSConstants.S_OK ||
                hierarchy == null)
            {
                return;
            }

            var storage = hierarchy as IVsBuildPropertyStorage;

            var nestedCount = ScanWithWaitDialog(targetDirectory, hierarchy, storage);

            if (nestedCount > 0 &&
                string.Equals(Path.GetExtension(project.FullName), ".njsproj", StringComparison.OrdinalIgnoreCase))
            {
                project.Save();
            }

            VsShellUtilities.ShowMessageBox(
                _package,
                nestedCount > 0
                    ? $"Auto-nested {nestedCount} file(s)."
                    : "No files found to auto-nest.",
                "Nestify",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
        catch (Exception ex)
        {
            VsShellUtilities.ShowMessageBox(
                _package,
                $"An error occurred during auto-nesting:\n{ex.Message}",
                "Nestify",
                OLEMSGICON.OLEMSGICON_CRITICAL,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }

    // Large directory trees are scanned synchronously on the UI thread; show the standard
    // VS wait dialog so the IDE does not appear frozen.
    private int ScanWithWaitDialog(string directory, IVsHierarchy hierarchy, IVsBuildPropertyStorage storage)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        IVsThreadedWaitDialog2 waitDialog = null;
        (Package.GetGlobalService(typeof(SVsThreadedWaitDialogFactory)) as IVsThreadedWaitDialogFactory)
            ?.CreateInstance(out waitDialog);

        waitDialog?.StartWaitDialog(
            "Nestify",
            "Scanning files to auto-nest...",
            null,
            null,
            "Nestify: auto-nesting files",
            2,
            false,
            true);

        try
        {
            return _directoryScanner.ScanAndNest(directory, hierarchy, storage);
        }
        finally
        {
            waitDialog?.EndWaitDialog(out _);
        }
    }
}
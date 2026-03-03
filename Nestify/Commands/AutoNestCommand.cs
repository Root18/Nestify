using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Nestify.Abstractions;
using System;
using System.ComponentModel.Design;
using System.IO;
using Task = System.Threading.Tasks.Task;

namespace Nestify.Commands
{
    internal sealed class AutoNestCommand
    {
        public const int CommandId = 0x0300;
        public static readonly Guid CommandSet = new Guid("e2c2b1a0-3d4e-4f5a-8b6c-7d8e9f0a1b2c");

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

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(Execute, menuCommandID);
            menuItem.BeforeQueryStatus += OnBeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        public static AutoNestCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package, IDirectoryScanner directoryScanner)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new AutoNestCommand(package, commandService, directoryScanner);
        }

        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var cmd = (OleMenuCommand)sender;
            cmd.Visible = false;

            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            if (dte?.SelectedItems == null || dte.SelectedItems.Count == 0)
                return;

            foreach (SelectedItem item in dte.SelectedItems)
            {
                if (item.Project != null || item.ProjectItem != null)
                {
                    cmd.Visible = true;
                    return;
                }
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
                    else if (item.ProjectItem != null)
                    {
                        project = item.ProjectItem.ContainingProject;
                        if (string.Equals(item.ProjectItem.Kind,
                            EnvDTE.Constants.vsProjectItemKindPhysicalFolder,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            targetDirectory = item.ProjectItem.FileNames[1];
                        }
                        else
                        {
                            targetDirectory = Path.GetDirectoryName(item.ProjectItem.FileNames[1]);
                        }
                        break;
                    }
                }

                if (project == null || string.IsNullOrEmpty(targetDirectory) || !Directory.Exists(targetDirectory))
                    return;

                var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
                solution.GetProjectOfUniqueName(project.UniqueName, out IVsHierarchy hierarchy);
                var storage = hierarchy as IVsBuildPropertyStorage;

                int nestedCount = _directoryScanner.ScanAndNest(targetDirectory, hierarchy, storage);

                if (nestedCount > 0)
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
    }
}

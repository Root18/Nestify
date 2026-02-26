using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Windows.Interop;
using Task = System.Threading.Tasks.Task;

namespace Nestify.Commands
{
    internal sealed class NestFilesCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("e2c2b1a0-3d4e-4f5a-8b6c-7d8e9f0a1b2c");

        private readonly AsyncPackage _package;

        private NestFilesCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(Execute, menuCommandID);
            menuItem.BeforeQueryStatus += OnBeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        public static NestFilesCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new NestFilesCommand(package, commandService);
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
                if (item.ProjectItem != null && FileNestingHelper.IsSupportedFile(item.ProjectItem.Name))
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

                var selectedItems = new List<ProjectItem>();
                Project project = null;

                foreach (SelectedItem item in dte.SelectedItems)
                {
                    if (item.ProjectItem != null && FileNestingHelper.IsSupportedFile(item.ProjectItem.Name))
                    {
                        selectedItems.Add(item.ProjectItem);
                        if (project == null)
                            project = item.ProjectItem.ContainingProject;
                    }
                }

                if (selectedItems.Count == 0 || project == null)
                    return;

                string firstItemPath = selectedItems[0].FileNames[1];
                string directory = Path.GetDirectoryName(firstItemPath);

                var selectedFileNames = new HashSet<string>(
                    selectedItems.Select(i => i.Name), StringComparer.OrdinalIgnoreCase);

                // Also exclude files already nested under any selected item (prevents circular nesting)
                foreach (var item in selectedItems)
                {
                    CollectDescendantNames(item, selectedFileNames);
                }

                var siblingFiles = GetSiblingFiles(directory, selectedFileNames);

                if (siblingFiles.Count == 0)
                {
                    VsShellUtilities.ShowMessageBox(
                        _package,
                        "No sibling files found to nest under.",
                        "Nestify",
                        OLEMSGICON.OLEMSGICON_INFO,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    return;
                }

                var dialog = new Dialogs.ParentFilePickerDialog(siblingFiles);
                var hwndHelper = new WindowInteropHelper(dialog);
                hwndHelper.Owner = dte.MainWindow.HWnd;

                if (dialog.ShowDialog() != true || string.IsNullOrEmpty(dialog.SelectedFile))
                    return;

                string parentFileName = dialog.SelectedFile;

                var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
                solution.GetProjectOfUniqueName(project.UniqueName, out IVsHierarchy hierarchy);
                var storage = hierarchy as IVsBuildPropertyStorage;

                foreach (var item in selectedItems)
                {
                    string itemFullPath = item.FileNames[1];
                    if (hierarchy.ParseCanonicalName(itemFullPath, out uint itemId) == 0 && itemId != 0)
                    {
                        Services.FileNestingService.NestFile(storage, itemId, parentFileName);
                    }
                }

                project.Save();
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(
                    _package,
                    $"An error occurred while nesting files:\n{ex.Message}",
                    "Nestify",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        private static List<string> GetSiblingFiles(string directory, HashSet<string> excludeNames)
        {
            var result = new List<string>();

            if (!Directory.Exists(directory))
                return result;

            foreach (string filePath in Directory.GetFiles(directory))
            {
                string fileName = Path.GetFileName(filePath);
                if (!excludeNames.Contains(fileName) && FileNestingHelper.IsPickerCandidate(fileName))
                {
                    result.Add(fileName);
                }
            }

            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        private static void CollectDescendantNames(ProjectItem item, HashSet<string> names)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (item.ProjectItems == null || item.ProjectItems.Count == 0) return;

            foreach (ProjectItem child in item.ProjectItems)
            {
                names.Add(child.Name);
                CollectDescendantNames(child, names);
            }
        }
    }
}

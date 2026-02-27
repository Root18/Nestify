using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
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

        private AutoNestCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(Execute, menuCommandID);
            menuItem.BeforeQueryStatus += OnBeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        public static AutoNestCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new AutoNestCommand(package, commandService);
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

                int nestedCount = 0;
                ProcessDirectory(targetDirectory, hierarchy, storage, ref nestedCount);

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

        private static void ProcessDirectory(string directory, IVsHierarchy hierarchy, IVsBuildPropertyStorage storage, ref int nestedCount)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Collect file names in this directory
            var files = Directory.GetFiles(directory);
            var fileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var f in files)
            {
                fileNames.Add(Path.GetFileName(f));
            }

            // Apply auto-nest rules
            foreach (var filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                string parentName = Services.AutoNestRuleEngine.FindParent(fileName, fileNames);

                if (parentName == null)
                    continue;

                // Resolve item in the project hierarchy
                if (hierarchy.ParseCanonicalName(filePath, out uint itemId) != 0 || itemId == 0)
                    continue;

                // Skip if already nested
                storage.GetItemAttribute(itemId, "DependentUpon", out string existingParent);
                if (!string.IsNullOrEmpty(existingParent))
                    continue;

                Services.FileNestingService.NestFile(storage, itemId, parentName);
                nestedCount++;
            }

            // Recurse into subdirectories, skipping build/hidden folders
            foreach (var subDir in Directory.GetDirectories(directory))
            {
                string dirName = Path.GetFileName(subDir);
                if (dirName.StartsWith(".", StringComparison.Ordinal) ||
                    dirName.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
                    dirName.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
                    dirName.Equals("node_modules", StringComparison.OrdinalIgnoreCase))
                    continue;

                ProcessDirectory(subDir, hierarchy, storage, ref nestedCount);
            }
        }
    }
}

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Nestify.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace Nestify.Commands
{
    internal sealed class UnnestFilesCommand
    {
        public const int CommandId = 0x0200;
        public static readonly Guid CommandSet = new Guid("e2c2b1a0-3d4e-4f5a-8b6c-7d8e9f0a1b2c");

        private readonly AsyncPackage _package;
        private readonly IFileValidator _fileValidator;
        private readonly IFileNestingService _nestingService;

        private UnnestFilesCommand(
            AsyncPackage package,
            OleMenuCommandService commandService,
            IFileValidator fileValidator,
            IFileNestingService nestingService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _fileValidator = fileValidator ?? throw new ArgumentNullException(nameof(fileValidator));
            _nestingService = nestingService ?? throw new ArgumentNullException(nameof(nestingService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(Execute, menuCommandID);
            menuItem.BeforeQueryStatus += OnBeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        public static UnnestFilesCommand Instance { get; private set; }

        public static async Task InitializeAsync(
            AsyncPackage package,
            IFileValidator fileValidator,
            IFileNestingService nestingService)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new UnnestFilesCommand(package, commandService, fileValidator, nestingService);
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
                if (item.ProjectItem != null && _fileValidator.IsSupportedFile(item.ProjectItem.Name))
                {
                    try
                    {
                        var parent = item.ProjectItem.Collection.Parent;
                        if (parent is ProjectItem parentItem &&
                            string.Equals(parentItem.Kind,
                                EnvDTE.Constants.vsProjectItemKindPhysicalFile,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            cmd.Visible = true;
                            return;
                        }
                    }
                    catch
                    {
                        // Ignore errors when checking nesting status
                    }
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
                    if (item.ProjectItem != null && _fileValidator.IsSupportedFile(item.ProjectItem.Name))
                    {
                        selectedItems.Add(item.ProjectItem);
                        if (project == null)
                            project = item.ProjectItem.ContainingProject;
                    }
                }

                if (selectedItems.Count == 0 || project == null)
                    return;

                var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
                solution.GetProjectOfUniqueName(project.UniqueName, out IVsHierarchy hierarchy);
                var storage = hierarchy as IVsBuildPropertyStorage;

                foreach (var item in selectedItems)
                {
                    _nestingService.UnnestFile(item, hierarchy, storage);
                }

                project.Save();
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(
                    _package,
                    $"An error occurred while unnesting files:\n{ex.Message}",
                    "Nestify",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }
    }
}

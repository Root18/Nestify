using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Nestify.Abstractions;
using Nestify.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace Nestify.Commands;

internal sealed class UnnestFilesCommand
{
    public const int CommandId = 0x0200;
    public static readonly Guid CommandSet = new("e2c2b1a0-3d4e-4f5a-8b6c-7d8e9f0a1b2c");

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

        var menuCommandId = new CommandID(CommandSet, CommandId);
        var menuItem = new OleMenuCommand(Execute, menuCommandId);
        menuItem.BeforeQueryStatus += OnBeforeQueryStatus;
        commandService.AddCommand(menuItem);
    }

    public static UnnestFilesCommand Instance { get; private set; }

    public static async Task InitializeAsync(
        AsyncPackage package,
        IFileValidator fileValidator,
        IFileNestingService nestingService)
    {
        await package.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
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
            if (item.ProjectItem == null || !_fileValidator.IsSupportedFile(item.ProjectItem.Name)) continue;
            if (!ProjectItemHelper.IsNestedUnderFile(item.ProjectItem)) continue;
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

            // Only touch files that are actually nested; unnesting must never rewrite
            // metadata of top-level items that happen to be part of the selection.
            var selectedItems = new List<ProjectItem>();
            foreach (SelectedItem item in dte.SelectedItems)
            {
                if (item.ProjectItem == null || !_fileValidator.IsSupportedFile(item.ProjectItem.Name)) continue;
                if (!ProjectItemHelper.IsNestedUnderFile(item.ProjectItem)) continue;
                selectedItems.Add(item.ProjectItem);
            }

            if (selectedItems.Count == 0)
                return;

            if (Package.GetGlobalService(typeof(SVsSolution)) is not IVsSolution solution) return;

            // Resolve the hierarchy per item so multi-project selections unnest correctly.
            var projectsToSave = new Dictionary<string, Project>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in selectedItems)
            {
                var project = item.ContainingProject;
                if (project == null) continue;

                if (solution.GetProjectOfUniqueName(project.UniqueName, out var hierarchy) != VSConstants.S_OK ||
                    hierarchy == null)
                {
                    continue;
                }

                _nestingService.UnnestFile(item, hierarchy, hierarchy as IVsBuildPropertyStorage);

                if (string.Equals(System.IO.Path.GetExtension(project.FullName), ".njsproj",
                        StringComparison.OrdinalIgnoreCase))
                {
                    projectsToSave[project.UniqueName] = project;
                }
            }

            foreach (var project in projectsToSave.Values)
            {
                project.Save();
            }
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

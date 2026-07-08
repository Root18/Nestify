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
using System.IO;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace Nestify.Commands;

internal sealed class NestFilesCommand
{
    public const int CommandId = 0x0100;
    public static readonly Guid CommandSet = new("e2c2b1a0-3d4e-4f5a-8b6c-7d8e9f0a1b2c");

    private readonly AsyncPackage _package;
    private readonly IFileValidator _fileValidator;
    private readonly IFileNestingService _nestingService;
    private readonly ISiblingFileProvider _siblingFileProvider;
    private readonly IDialogService _dialogService;

    private NestFilesCommand(
        AsyncPackage package,
        OleMenuCommandService commandService,
        IFileValidator fileValidator,
        IFileNestingService nestingService,
        ISiblingFileProvider siblingFileProvider,
        IDialogService dialogService)
    {
        _package = package ?? throw new ArgumentNullException(nameof(package));
        commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _fileValidator = fileValidator ?? throw new ArgumentNullException(nameof(fileValidator));
        _nestingService = nestingService ?? throw new ArgumentNullException(nameof(nestingService));
        _siblingFileProvider = siblingFileProvider ?? throw new ArgumentNullException(nameof(siblingFileProvider));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        var menuCommandId = new CommandID(CommandSet, CommandId);
        var menuItem = new OleMenuCommand(Execute, menuCommandId);
        menuItem.BeforeQueryStatus += OnBeforeQueryStatus;
        commandService.AddCommand(menuItem);
    }

    public static NestFilesCommand Instance { get; private set; }

    public static async Task InitializeAsync(
        AsyncPackage package,
        IFileValidator fileValidator,
        IFileNestingService nestingService,
        ISiblingFileProvider siblingFileProvider,
        IDialogService dialogService)
    {
        await package.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
        var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
        Instance = new NestFilesCommand(package, commandService, fileValidator, nestingService, siblingFileProvider,
            dialogService);
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

            var selectedItems = new List<ProjectItem>();
            Project project = null;

            foreach (SelectedItem item in dte.SelectedItems)
            {
                if (item.ProjectItem == null || !_fileValidator.IsSupportedFile(item.ProjectItem.Name)) continue;
                selectedItems.Add(item.ProjectItem);
                project ??= item.ProjectItem.ContainingProject;
            }

            if (selectedItems.Count == 0 || project == null)
                return;

            var firstItemPath = selectedItems[0].FileNames[1];
            var directory = Path.GetDirectoryName(firstItemPath);

            if (string.IsNullOrEmpty(directory))
                return;

            // DependentUpon nesting only works between items in the same folder of one project.
            foreach (var item in selectedItems)
            {
                if (string.Equals(item.ContainingProject?.UniqueName, project.UniqueName,
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(Path.GetDirectoryName(item.FileNames[1]), directory,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                VsShellUtilities.ShowMessageBox(
                    _package,
                    "Files can only be nested under a file in the same folder of the same project.\n" +
                    "Adjust your selection and try again.",
                    "Nestify",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }

            var selectedFileNames = new HashSet<string>(
                selectedItems.Select(i =>
                {
                    ThreadHelper.ThrowIfNotOnUIThread();
                    return i.Name;
                }), StringComparer.OrdinalIgnoreCase);

            foreach (var item in selectedItems)
            {
                CollectDescendantNames(item, selectedFileNames);
            }

            List<string> pickerCandidates;
            if (selectedItems.Count > 1)
            {
                pickerCandidates = selectedItems
                    .Select(i =>
                    {
                        ThreadHelper.ThrowIfNotOnUIThread();
                        return i.Name;
                    })
                    .ToList();
                pickerCandidates.Sort(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                pickerCandidates = _siblingFileProvider.GetSiblingFiles(directory, selectedFileNames);
            }

            if (pickerCandidates.Count == 0)
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

            var parentFileName = _dialogService.ShowParentFilePicker(pickerCandidates, dte.MainWindow.HWnd);
            if (string.IsNullOrEmpty(parentFileName))
                return;

            if (Package.GetGlobalService(typeof(SVsSolution)) is not IVsSolution solution) return;
            if (solution.GetProjectOfUniqueName(project.UniqueName, out IVsHierarchy hierarchy) != VSConstants.S_OK ||
                hierarchy == null)
            {
                return;
            }

            var storage = hierarchy as IVsBuildPropertyStorage;

            var parentFullPath = Path.Combine(directory, parentFileName);
            var parentItem = VsHierarchyHelper.GetProjectItem(hierarchy, parentFullPath);
            if (parentItem == null)
                return;

            foreach (var item in selectedItems)
            {
                if (string.Equals(item.Name, parentFileName, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (WouldCreateNestingCycle(item, parentItem))
                    continue;
                _nestingService.NestFile(item, parentItem, hierarchy, storage);
            }

            if (string.Equals(Path.GetExtension(project.FullName), ".njsproj", StringComparison.OrdinalIgnoreCase))
            {
                project.Save();
            }
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

    // Nesting a file under one of its own (transitive) nested children would create a cycle.
    private static bool WouldCreateNestingCycle(ProjectItem child, ProjectItem parent)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        try
        {
            var childPath = child.FileNames[1];
            var current = parent;
            while (current?.Collection?.Parent is ProjectItem ancestor)
            {
                if (string.Equals(ancestor.FileNames[1], childPath, StringComparison.OrdinalIgnoreCase))
                    return true;
                current = ancestor;
            }

            return false;
        }
        catch
        {
            return true;
        }
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
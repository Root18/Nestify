using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Nestify.Abstractions;
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
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
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

            var siblingFiles = _siblingFileProvider.GetSiblingFiles(directory, selectedFileNames);

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

            var parentFileName = _dialogService.ShowParentFilePicker(siblingFiles, dte.MainWindow.HWnd);
            if (string.IsNullOrEmpty(parentFileName))
                return;

            if (Package.GetGlobalService(typeof(SVsSolution)) is not IVsSolution solution) return;
            solution.GetProjectOfUniqueName(project.UniqueName, out IVsHierarchy hierarchy);
            var storage = hierarchy as IVsBuildPropertyStorage;

            var parentFullPath = Path.Combine(directory, parentFileName);
            if (hierarchy.ParseCanonicalName(parentFullPath, out var parentId) != 0 || parentId == 0)
                return;

            hierarchy.GetProperty(parentId, (int)__VSHPROPID.VSHPROPID_ExtObject, out var parentObj);
            if (parentObj is not ProjectItem parentItem)
                return;

            foreach (var item in selectedItems)
            {
                _nestingService.NestFile(item, parentItem, hierarchy, storage);
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
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace Nestify.Commands;

internal sealed class ToggleAutoNestCommand
{
    public const int CommandId = 0x0400;
    public static readonly Guid CommandSet = new("e2c2b1a0-3d4e-4f5a-8b6c-7d8e9f0a1b2c");

    private ToggleAutoNestCommand(OleMenuCommandService commandService)
    {
        commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

        var menuCommandId = new CommandID(CommandSet, CommandId);
        var menuItem = new OleMenuCommand(Execute, menuCommandId);
        menuItem.BeforeQueryStatus += OnBeforeQueryStatus;
        commandService.AddCommand(menuItem);
    }

    public static ToggleAutoNestCommand Instance { get; private set; }

    public static async Task InitializeAsync(AsyncPackage package)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
        var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
        Instance = new ToggleAutoNestCommand(commandService);
    }

    private void OnBeforeQueryStatus(object sender, EventArgs e)
    {
        var cmd = (OleMenuCommand)sender;
        cmd.Checked = NestifyPackage.Options != null && NestifyPackage.Options.AutoNestEnabled;
    }

    private void Execute(object sender, EventArgs e)
    {
        if (NestifyPackage.Options == null)
            return;

        NestifyPackage.Options.AutoNestEnabled = !NestifyPackage.Options.AutoNestEnabled;
    }
}

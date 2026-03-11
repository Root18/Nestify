using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Nestify.Abstractions;
using Nestify.Options;
using Nestify.Rules;
using Nestify.Services;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace Nestify;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[Guid(PackageGuidString)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
public sealed class NestifyPackage : AsyncPackage
{
    public const string PackageGuidString = "d79ad6c9-74af-4ea1-8c02-0c62866f1a7b";

    internal static NestifyOptions Options { get; private set; }

    protected override async Task InitializeAsync(CancellationToken cancellationToken,
        IProgress<ServiceProgressData> progress)
    {
        await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        var settingsManager = new ShellSettingsManager(this);
        var store = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
        Options = new NestifyOptions(store);

        // Compose the object graph (Composition Root)
        IFileValidator fileValidator = new FileValidator();
        IFileNestingService nestingService = new FileNestingService();

        INestingRule[] rules =
        [
            new CSharpInterfaceNestingRule(),
            new JavaScriptBundleMinNestingRule(),
            new JavaScriptBundleNestingRule(),
            new JavaScriptMinNestingRule()
        ];
        IAutoNestRuleEngine ruleEngine = new AutoNestRuleEngine(rules);
        IDirectoryScanner directoryScanner = new DirectoryScanner(ruleEngine, nestingService);
        ISiblingFileProvider siblingFileProvider = new SiblingFileProvider(fileValidator);
        IDialogService dialogService = new DialogService();

        await Commands.NestFilesCommand.InitializeAsync(this, fileValidator, nestingService, siblingFileProvider,
            dialogService);
        await Commands.UnnestFilesCommand.InitializeAsync(this, fileValidator, nestingService);
        await Commands.AutoNestCommand.InitializeAsync(this, directoryScanner);
        await Commands.ToggleAutoNestCommand.InitializeAsync(this);
    }
}
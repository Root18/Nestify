# Contributing to Nestify

Thank you for your interest in contributing to Nestify! This guide will help you get started.

## How Can I Contribute?

### Reporting Bugs

Before creating a bug report, please check the [existing issues](https://github.com/Root18/Nestify/issues) to avoid duplicates. When filing a bug report, use the [bug report template](https://github.com/Root18/Nestify/issues/new?template=bug_report.md) and include as much detail as possible.

### Suggesting Features

Feature requests are tracked as GitHub issues. Use the [feature request template](https://github.com/Root18/Nestify/issues/new?template=feature_request.md) and explain:

- The problem your feature would solve.
- How you envision it working.
- Any alternatives you have considered.

### Submitting Pull Requests

1. **Fork** the repository and create your branch from `develop`.
2. Make your changes in a focused, well-scoped commit.
3. Ensure the solution builds without errors in Visual Studio 2022+.
4. Run the unit tests and make sure they all pass (see [Running Tests](#running-tests)).
5. Add or update tests for any new or changed functionality.
6. Test the extension in the Visual Studio Experimental Instance.
7. Open a pull request against the `develop` branch.

## Development Setup

### Prerequisites

- **Visual Studio 2022** (17.0 or later) with the _Visual Studio extension development_ workload installed.
- **.NET Framework 4.7.2** targeting pack.

### Building

1. Open `Nestify.sln` in Visual Studio 2022.
2. Build the solution (`Ctrl+Shift+B`).
3. Press `F5` to launch the Experimental Instance with the extension loaded.

### Solution Structure

The solution contains two projects:

| Project | Description |
| --- | --- |
| **Nestify** | The Visual Studio extension (VSIX). Targets .NET Framework 4.7.2. |
| **Nestify.Tests** | Unit tests for the extension. Targets .NET Framework 4.7.2. |

Key folders inside the **Nestify** project:

| Folder | Contents |
| --- | --- |
| `Abstractions/` | Interfaces (`INestingRule`, `IFileValidator`, `IDirectoryScanner`, etc.) |
| `Commands/` | VS menu commands (`NestFilesCommand`, `UnnestFilesCommand`, `AutoNestCommand`) |
| `Dialogs/` | WPF dialogs (`ParentFilePickerDialog`) |
| `Rules/` | Auto-nest rule implementations (e.g. `CSharpInterfaceNestingRule`, `JavaScriptMinNestingRule`) |
| `Services/` | Core services (`AutoNestRuleEngine`, `DirectoryScanner`, `FileValidator`, etc.) |
| `Utilities/` | Helper classes (`PathUtilities`) |

### Running Tests

The **Nestify.Tests** project uses [MSTest](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest). Every class in the main project has a corresponding test class.

From the command line:

```bash
dotnet test Nestify.Tests\Nestify.Tests.csproj
```

Or from Visual Studio: open **Test Explorer** (`Ctrl+E, T`) and run all tests.

When contributing, please ensure:

- All existing tests continue to pass.
- New classes include a corresponding test class in the matching folder under `Nestify.Tests`.
- Test class naming follows the pattern `<ClassName>Tests` (e.g. `DirectoryScannerTests`).

## Coding Guidelines

- Follow the existing code style and conventions found in the codebase.
- Keep changes minimal and focused — one concern per pull request.
- Use meaningful commit messages that describe _what_ changed and _why_.
- Do not introduce new external dependencies unless absolutely necessary.

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). By participating, you agree to abide by its terms.

## Questions?

If you have questions about contributing, feel free to [open an issue](https://github.com/Root18/Nestify/issues/new) for discussion.

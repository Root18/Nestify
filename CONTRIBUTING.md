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
4. Test the extension in the Visual Studio Experimental Instance.
5. Open a pull request against the `develop` branch.

## Development Setup

### Prerequisites

- **Visual Studio 2022** (17.0 or later) with the _Visual Studio extension development_ workload installed.
- **.NET Framework 4.7.2** targeting pack.

### Building

1. Open `Nestify.sln` in Visual Studio 2022.
2. Build the solution (`Ctrl+Shift+B`).
3. Press `F5` to launch the Experimental Instance with the extension loaded.

## Coding Guidelines

- Follow the existing code style and conventions found in the codebase.
- Keep changes minimal and focused — one concern per pull request.
- Use meaningful commit messages that describe _what_ changed and _why_.
- Do not introduce new external dependencies unless absolutely necessary.

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). By participating, you agree to abide by its terms.

## Questions?

If you have questions about contributing, feel free to [open an issue](https://github.com/Root18/Nestify/issues/new) for discussion.

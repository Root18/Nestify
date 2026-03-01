# Nestify

A Visual Studio extension that brings smart file nesting to Solution Explorer. Nest, unnest, and auto-nest files based on naming conventions — keeping your projects tidy and navigable.

## Features

- **Nest Files** — Manually nest selected files under a parent file using a picker dialog.
- **Unnest Files** — Remove nesting and restore files to the top level.
- **Auto-Nest** — Automatically detect parent–child relationships using built-in rules:
  - C# implementations nest under their interfaces (`UserService.cs` → `IUserService.cs`)
  - Minified/bundled JS files nest under their source (`app.js` → `app.bundle.js` → `app.bundle.min.js`)

## Supported File Types

`.cs` `.vb` `.fs` `.js` `.jsx` `.ts` `.tsx` `.css` `.scss` `.less` `.html` `.htm` `.json` `.xml` `.config` `.resx` `.xaml` `.razor` `.cshtml`

## Requirements

| Requirement    | Version      |
| -------------- | ------------ |
| Visual Studio  | 2022 (17.0+) |
| .NET Framework | 4.7.2        |

## Installation

Download and install the Nestify extension from the latest release.

## Usage

1. Right-click one or more files in **Solution Explorer**.
2. Choose one of the Nestify commands from the context menu:
   - **Nest Files** — pick a parent file from the dialog.
   - **Unnest Files** — remove the nesting relationship.
   - **Auto-Nest** — let Nestify find parents automatically based on naming rules.

## Contributing

Contributions are welcome! Please read the [Contributing Guide](CONTRIBUTING.md) before submitting a pull request.

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

## Security

To report a vulnerability, please see [SECURITY.md](SECURITY.md).

## Support

Need help? Check out [SUPPORT.md](SUPPORT.md) for available resources.

## License

This project is open source. See the repository for license details.

## Sponsor

If you find Nestify useful, consider supporting its development:

<a href="https://github.com/sponsors/Root18" target="_blank">
  <img src="https://img.shields.io/badge/Sponsor-❤-ea4aaa?style=for-the-badge&logo=githubsponsors&logoColor=white"
       alt="GitHub Sponsors" height="41">
</a>

<a href="https://www.buymeacoffee.com/iroot18" target="_blank">
  <img src="https://cdn.buymeacoffee.com/buttons/default-orange.png"
       alt="Buy Me A Coffee" height="41" width="174">
</a>

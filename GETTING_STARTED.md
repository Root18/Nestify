# Getting Started with Nestify

Welcome to Nestify! This guide will help you get up and running in minutes.

## Installation

### From the Visual Studio Marketplace

1. Visit the [Nestify page on the Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=Root18.nestify).
2. Click **Download** and open the `.vsix` file, or search for **Nestify** directly in Visual Studio under **Extensions → Manage Extensions**.
3. Restart Visual Studio if prompted.

### From GitHub Releases

1. Download the latest `.vsix` file from the [Releases](https://github.com/Root18/Nestify/releases) page.
2. Double-click the `.vsix` file to install it, or go to **Extensions → Manage Extensions** in Visual Studio and install from file.
3. Restart Visual Studio if prompted.

## Quick Start

### Nest Files

1. In **Solution Explorer**, select one or more files you want to nest.
2. Right-click and choose **Nest Files** from the context menu.
3. In the picker dialog, select the parent file you want to nest under.
4. Click **OK** — the selected files are now nested beneath the parent.

### Unnest Files

1. Select any nested file(s) in **Solution Explorer**.
2. Right-click and choose **Unnest Files**.
3. The files are restored to the top level.

### Auto-Nest

1. Select files in **Solution Explorer**.
2. Right-click and choose **Auto-Nest**.
3. Nestify automatically detects parent–child relationships using built-in rules:
   - `UserService.md` nests under `UserService.cs` (C# documentation rule)
   - `account.md` nests under `account.js` (JS documentation rule)
   - `UserService.cs` nests under `IUserService.cs` (C# interface rule)
   - `app.bundle.js` nests under `app.js` (JS bundle rule)
   - `app.bundle.min.js` nests under `app.bundle.js` (JS minified bundle rule)
   - `app.min.js` nests under `app.js` (JS minified rule)

## Supported File Types

Nestify supports 20+ file types:

`.cs` `.vb` `.fs` `.js` `.jsx` `.ts` `.tsx` `.css` `.scss` `.less` `.html` `.htm` `.json` `.xml` `.config` `.resx` `.xaml` `.razor` `.cshtml` `.md`

## Tips

- **Multi-select** is supported — nest or unnest multiple files at once.
- **Auto-Nest** works best when your files follow common naming conventions.
- Nesting is non-destructive — it only changes how files appear in Solution Explorer, not on disk.

## Need Help?

- Check the [README](https://github.com/Root18/Nestify#readme) for full documentation.
- Report issues on [GitHub Issues](https://github.com/Root18/Nestify/issues).
- See [SUPPORT.md](https://github.com/Root18/Nestify/blob/develop/SUPPORT.md) for additional resources.

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

1. In **Solution Explorer**, select one or more files you want to nest (they must be in the same folder of the same project).
2. Right-click and choose **Nestify: Nest under...** from the context menu.
3. In the picker dialog, select the parent file you want to nest under (type to filter).
4. Click **OK** — the selected files are now nested beneath the parent.

### Unnest Files

1. Select any nested file(s) in **Solution Explorer**.
2. Right-click and choose **Nestify: Unnest**.
3. The files are restored to the top level. Files in the selection that aren't nested are left untouched.

### Auto-Nest

1. Right-click the project node in **Solution Explorer** and choose **Nestify: Enable Auto-nest** (one-time toggle; it also shows the current state).
2. Right-click a project, folder, or file and choose **Nestify: Auto-nest**.
3. Nestify automatically detects parent–child relationships using built-in rules:
   - `UserService.md` nests under `UserService.cs` (documentation rule; also pairs with `.vb`, `.ts`, `.tsx`, `.js`, `.jsx`)
   - `UserService.cs` nests under `IUserService.cs` (C# interface rule)
   - `app.bundle.js` nests under `app.js` (JS bundle rule)
   - `app.bundle.min.js` nests under `app.bundle.js` (JS minified bundle rule)
   - `app.min.js` nests under `app.js` (JS minified rule)

## Supported File Types

Nestify supports 20+ file types:

`.cs` `.vb` `.fs` `.js` `.jsx` `.ts` `.tsx` `.mjs` `.mts` `.cjs` `.cts` `.vue` `.css` `.scss` `.less` `.html` `.htm` `.json` `.xml` `.config` `.resx` `.xaml` `.razor` `.cshtml` `.md`

## Tips

- **Multi-select** is supported — nest or unnest multiple files at once.
- **Auto-Nest** works best when your files follow common naming conventions.
- Nesting is non-destructive — it only changes how files appear in Solution Explorer, not on disk. Build actions and other file metadata are never modified.
- Changes appear in Solution Explorer immediately — no project unload/reload needed.

## Project Compatibility Notes

- Nestify supports different Visual Studio project systems, including common `.csproj` and Node.js `.njsproj` scenarios.
- If you used an older version and saw project reload prompts after nesting operations, update to the latest build.
- For best results, run nesting commands from Solution Explorer after the project has fully loaded.

## Need Help?

- Check the [README](https://github.com/Root18/Nestify#readme) for full documentation.
- Report issues on [GitHub Issues](https://github.com/Root18/Nestify/issues).
- See [SUPPORT.md](https://github.com/Root18/Nestify/blob/develop/SUPPORT.md) for additional resources.

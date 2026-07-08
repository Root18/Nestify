# Nestify for VS Code

Smart file nesting for the **Explorer**. Nest, unnest, and auto-nest related files — the VS Code companion to [Nestify for Visual Studio](https://github.com/Root18/Nestify).

Nestify builds on VS Code's native file-nesting engine (`explorer.fileNesting.patterns`), giving it the point-and-click workflow it is missing: no more hand-editing settings JSON.

## Features

- **Nest under...** — Right-click one or more files and pick the parent to nest them under. With multiple files selected, any of them can be the parent.
- **Unnest** — Remove the nesting relationship for the selected files.
- **Auto-nest** — Apply built-in naming rules so related files nest automatically, now and as new files are created:
  - C# implementations nest under their interfaces (`UserService.cs` → `IUserService.cs`)
  - Markdown documentation nests under the matching code file (`UserService.md` → `UserService.cs`; also `.vb`, `.ts`, `.tsx`, `.js`, `.jsx`)
  - Minified/bundled JS nests under its source (`app.min.js` → `app.js`, `app.bundle.js` → `app.js`, `app.bundle.min.js` → `app.bundle.js`)
- **Remove auto-nest rules** — Cleanly removes exactly the rules Auto-nest added, leaving your own patterns untouched.
- **Toggle file nesting** — Switch `explorer.fileNesting.enabled` on or off.

## How it works

Nestify writes to the standard VS Code settings — nothing proprietary:

- Manual nesting adds exact-name entries to `explorer.fileNesting.patterns` (workspace settings when a workspace is open, user settings otherwise).
- Auto-nest adds glob rules with `${capture}` placeholders.
- File nesting is enabled automatically the first time you nest something.

Because it's just settings, your nesting is visible in `.vscode/settings.json`, travels with the repository, and works for teammates who don't have Nestify installed.

## Notes & limitations

- VS Code file nesting is **single-level** (a child cannot have its own children) and applies **per file name pattern**, not per project item — this differs from Visual Studio's `DependentUpon` nesting.
- **Unnest** removes exact-name entries (created by *Nest under...* or by hand). Files nested by glob rules are released with **Remove auto-nest rules** or by editing the pattern.
- Nesting only changes how files are displayed in the Explorer — files on disk and build configs are never touched.

## Commands

| Command | Where |
| --- | --- |
| `Nestify: Nest under...` | Explorer context menu (files), Command Palette |
| `Nestify: Unnest` | Explorer context menu (files), Command Palette |
| `Nestify: Auto-nest (apply naming rules)` | Explorer context menu, Command Palette |
| `Nestify: Remove auto-nest rules` | Command Palette |
| `Nestify: Toggle file nesting` | Command Palette |

## Requirements

- VS Code 1.85 or later

## License

[MIT](LICENSE)

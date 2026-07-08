# Release Notes

## v1.2 — Reliability & Safety Update

### Fixes

- **Build actions are preserved.** Nesting or unnesting a code file (e.g. `.cs`) no longer changes its build action — previously files could silently drop out of compilation in C# projects.
- **Solution Explorer updates immediately.** Nesting and unnesting refresh the tree right away, without unloading/reloading the project, across legacy and SDK-style projects.
- **Unnest only touches nested files.** Non-nested files in the selection are left untouched.
- **Safer multi-select.** Nesting across different folders or projects is blocked with a clear message (nesting only works between same-folder siblings), and circular nesting is prevented. Unnesting selections that span multiple projects now works correctly.
- **Safer project-file fallback.** Direct project-file editing is limited to project types where it is known to be safe (`.vbproj`, `.fsproj`, `.pyproj`), reuses existing items of any item type instead of creating duplicates, and never touches `.vcxproj`, `.shproj`, or other formats.
- Unnesting no longer disables the Auto-nest toggle.
- Fixed potential errors when a project hierarchy could not be resolved.

### Improvements

- Markdown documentation nesting now also pairs with `.vb`, `.ts`, `.tsx`, and `.jsx` files.
- The parent picker dialog follows the active Visual Studio theme, opens centered on the IDE, focuses the filter box, and supports keyboard navigation (↓ moves into the list).
- Auto-nest shows the standard Visual Studio wait dialog while scanning large directory trees.
- Fallback errors are logged to the Visual Studio Activity Log for easier diagnostics.
- Added ARM64 support (Visual Studio on Windows ARM64 devices).

### Supported File Types

`.cs` `.vb` `.fs` `.js` `.jsx` `.ts` `.tsx` `.mjs` `.mts` `.cjs` `.cts` `.vue` `.css` `.scss` `.less` `.html` `.htm` `.json` `.xml` `.config` `.resx` `.xaml` `.razor` `.cshtml` `.md`

### Supported Editions

- Visual Studio Community / Professional / Enterprise (17.0+), amd64 and arm64

### Requirements

- .NET Framework 4.7.2

---

## v1.1 — Multi-Select Picker & Markdown Documentation Support

### Features

- **Nest Files** — When multiple files are selected, the picker dialog now displays **all selected files** as parent candidates. You can nest any selected file under any other selected file.
- **Unnest Files** — Remove nesting and restore files to the top level.
- **Auto-Nest** — Automatically detect parent–child relationships using built-in naming rules:
  - Markdown documentation nesting (`ClassName.md` → `ClassName.cs` or `filename.md` → `filename.js`)
  - C# interface nesting (`UserService.cs` → `IUserService.cs`)
  - JavaScript bundle nesting (`app.bundle.js` → `app.js`)
  - JavaScript bundle + minified nesting (`app.bundle.min.js` → `app.bundle.js`)
  - JavaScript minified nesting (`app.min.js` → `app.js`)

### Fixes

- Improved nesting reliability across mixed project systems, including legacy and SDK-style projects.
- Fixed cases where nested items did not appear immediately in Solution Explorer.
- Reduced project-system conflicts that could trigger "File Modification Detected" / reload warnings.
- Improved Node.js project (`.njsproj`) handling so nesting metadata is applied in the expected item type.

### Notes

- Nesting metadata behavior is now applied through project-system-friendly paths first, with safer fallback behavior.

### Supported File Types

`.cs` `.vb` `.fs` `.js` `.jsx` `.ts` `.tsx` `.css` `.scss` `.less` `.html` `.htm` `.json` `.xml` `.config` `.resx` `.xaml` `.razor` `.cshtml` `.md`

### Supported Editions

- Visual Studio 2022 Community (17.0+)
- Visual Studio 2022 Professional (17.0+)
- Visual Studio 2022 Enterprise (17.0+)

### Requirements

- .NET Framework 4.7.2

---

## v1.0 — Initial Release

### Features

- **Nest Files** — Manually nest selected files under a parent file using a picker dialog.
- **Unnest Files** — Remove nesting and restore files to the top level.
- **Auto-Nest** — Automatically detect parent–child relationships using built-in naming rules:
  - C# interface nesting (`UserService.cs` → `IUserService.cs`)
  - JavaScript bundle nesting (`app.bundle.js` → `app.js`)
  - JavaScript bundle + minified nesting (`app.bundle.min.js` → `app.bundle.js`)
  - JavaScript minified nesting (`app.min.js` → `app.js`)

### Supported File Types

`.cs` `.vb` `.fs` `.js` `.jsx` `.ts` `.tsx` `.css` `.scss` `.less` `.html` `.htm` `.json` `.xml` `.config` `.resx` `.xaml` `.razor` `.cshtml`

### Supported Editions

- Visual Studio 2022 Community (17.0+)
- Visual Studio 2022 Professional (17.0+)
- Visual Studio 2022 Enterprise (17.0+)

### Requirements

- .NET Framework 4.7.2

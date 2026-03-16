# Release Notes

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

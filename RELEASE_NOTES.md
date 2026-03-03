# Release Notes

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

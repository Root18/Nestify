<p align="center">
  <img src="https://raw.githubusercontent.com/Root18/Nestify/develop/Nestify/Resources/nestify_icon.png" alt="Nestify logo" width="96" />
</p>


<p align="center">
  <h1 align="center">Nestify for Visual Studio</h1>
</p>

Smart file nesting for **Solution Explorer** in Visual Studio.

Nestify helps keep projects tidy by letting you **nest**, **unnest**, and **auto-nest** related files using practical naming rules.

<p align="center">
  <img src="https://raw.githubusercontent.com/Root18/Nestify/develop/Nestify/Resources/nestify_preview.gif" alt="Nestify preview" />
</p>

## Highlights

- **Nest Files** — Manually nest one or multiple selected files under a parent file.
- **Unnest Files** — Quickly remove nesting relationships.
- **Auto-Nest** — Detect and apply parent-child relationships automatically.

## Built-in Auto-Nest Rules

- C# interface nesting (`UserService.cs` → `IUserService.cs`)
- JavaScript bundle nesting (`app.bundle.js` → `app.js`)
- JavaScript minified bundle nesting (`app.bundle.min.js` → `app.bundle.js`)
- JavaScript minified nesting (`app.min.js` → `app.js`)
- Markdown documentation nesting (`ClassName.md` → `ClassName.cs` or `file.md` → `file.js`)

## Supported File Types

`.cs` `.vb` `.fs` `.js` `.jsx` `.ts` `.tsx` `.css` `.scss` `.less` `.html` `.htm` `.json` `.xml` `.config` `.resx` `.xaml` `.razor` `.cshtml` `.md`

## Compatibility

- Visual Studio **Community / Professional / Enterprise** (17.0+)
- .NET Framework **4.7.2**

## Links

- Source: https://github.com/Root18/Nestify
- Support: https://github.com/Root18/Nestify/blob/develop/SUPPORT.md
- Issues: https://github.com/Root18/Nestify/issues

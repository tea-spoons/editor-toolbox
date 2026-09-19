# Editor Toolbox
A collection of handy editor tools.

## Favorites Window
Allows you to drag important objects into a list and quickly get back to them at a later time.
Works for assets as well as GameObjects in a scene.
- Open the window using `Window/Favorites`.
- Find a manual in the window.

## String Input Dialog
A modal dialog that can be opened to require a string input from the editor user.
Can be used by other tools.

## SO Props Window
An editor window that can be used to filter properties of any ScriptableObject.
Used to find fields or values within overly complex SOs.

## ProjectPathUtility
Miscellaneous methods and properties for working with paths in the editor.

Example: `TryGetActiveFolderPath` for acquiring the folder that's currently open in the project view.
## Installation

In Unity: **Window > Package Manager > + > Add package from git URL**, then enter:

```
https://github.com/tea-spoons/editor-toolbox.git
```

Pin a release by appending a tag, for example `#v0.4.0`.

### Dependencies

Unity cannot resolve git dependencies automatically, so add these to your project first:

- `com.tea-spoons.package-core` 0.3.0

## Change plan

See [CHANGE-PLAN.md](CHANGE-PLAN.md) for what changed before publishing and what is planned next.

## License

Copyright (c) 2026 Bigpoint. Authored by Muhammad Tarek Abdou.

Available for research, education and other noncommercial use under the [PolyForm Noncommercial 1.0.0](LICENSE.md)
license. Commercial use is not permitted.

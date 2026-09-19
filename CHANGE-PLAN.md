# Change plan

> Draft. This file tracks what changed on the way to this repo and what I plan to change next. Edit freely.

## Origin

Originally developed at Bigpoint. Published here with Bigpoint's permission for research, education and other noncommercial use. Copyright (c) 2026 Bigpoint; see [LICENSE.md](LICENSE.md).

## Changes made before publishing

- Namespaces are now `TeaSpoons.*` and the package id is `com.tea-spoons.editor-toolbox` (assemblies renamed to match).
- Internal build, registry and tracker references were removed; the repo uses GitHub Actions (`CI` and `Release`) built on `unity-ci-kit`.
- Added `LICENSE.md` (PolyForm Noncommercial 1.0.0), an install section in the README, and package metadata (author, license and documentation URLs).
- Removed `FavoritesWindow` and `StringInputDialog`: they were adapted from a public gist without a stated license.
- Added a new `StringInputDialog` written from scratch against Unity's public `EditorWindow` API, with tests (version 0.4.0).

## Planned changes

- [x] Tag and publish `v0.4.0` with the Release workflow.
- [ ] Make installs resolve dependencies automatically, for example through a registry such as OpenUPM.
- [ ] Rebuild the removed favorites window as original code.
<!-- review-items:start -->
- [ ] **P1** Add a fallback for the active folder that needs no reflection (selection, then `AssetDatabase.GetAssetPath`, then its folder) and a test on 6000.3.
- [ ] **P1** Make package-core optional or inline what is used.
- [ ] **P1** Declares `unity: 2022.3`, but only Unity 6000.3.8f1 was tested. Add a Unity version matrix to CI once package tests run there (see the `unity-ci-kit` plan), or raise the minimum.
- [ ] **P1** Run the tests in CI. The kit's `run-tests` needs a Unity project, so this waits for package-mode support in `unity-ci-kit` (planned there; GameCI's test runner has a `packageMode` for the same reason).
- [ ] **P2** Add a `CHANGELOG.md`. Unity's package layout lists one next to `README.md`, and the `unity-ci-kit` validator warns without it.
- [ ] **P2** The README is only 47 lines. Add a short example for each public type.
<!-- review-items:end -->

<!-- review:start -->
## Review (September 2026)

Reviewed as a senior Unity engineer would: I read the code and compared the package with similar open-source projects (September 2026). Those projects are listed for ideas only. Nothing was copied from them, and their licenses are noted in case code is ever reused. Priorities: **P0** correctness bug or broken metadata, **P1** should be done soon, **P2** nice to have.

### Compared with

| Project | License | Worth noting |
|---|---|---|
| [Deadcows/MyBox](https://github.com/Deadcows/MyBox) | not checked | Attributes, tools and extensions for the Inspector and editor. |
| [dbrizov/NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes) | not checked | Inspector attribute extensions without custom editors. |

### Findings from reading the code

- **[Fragility]** `ProjectPathUtility.TryGetActiveFolderPath` calls the internal `ProjectWindowUtil.TryGetActiveFolderPath` through reflection. An internal method can disappear in any Unity release. It fails soft (returns `false`), so the feature would die silently.
- **[Scope]** Two public types (`ProjectPathUtility`, `StringInputDialog`). The favorites window was removed during the migration.
- **[Coupling]** Depends on package-core.
<!-- review:end -->

## Notes and ideas

_Add your own here._

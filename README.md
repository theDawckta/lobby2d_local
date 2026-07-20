# gamefactory starter template

Canonical starter project for [AI Game Factory](https://github.com/theDawckta/ai-game-factory)
games. Every new `gamefactory_<slug>` repo is generated from this template so it begins as a
correctly-configured project instead of the deprecated built-in render pipeline.

**What it ships:**

- **Universal Render Pipeline (URP)**, Unity 6.5 (`6000.5.2f1`). `Assets/Settings/URP-Standard.asset`
  (+ `URP-Standard-Renderer.asset`, a Forward+ Universal Renderer) is assigned in
  `ProjectSettings/GraphicsSettings.asset` **and every `QualitySettings` tier** — the assignment
  step that a hand-scaffolded project routinely misses.
- **A clear-only Main Camera** in `Assets/Game/Scenes/Main.unity` (the build scene), so a
  UI-only game never shows the "No cameras rendering" overlay.
- **Standard `Assets/Settings/GamePanelSettings.asset`** — UI Toolkit PanelSettings on
  Scale-With-Screen-Size (1920x1080, match height) so fixed-size layouts don't clip on smaller
  windows. Attach it to game UIDocuments.
- **CoreSystems + CoreAssets** referenced via UPM git URL, plus Input System (`1.19.0`), TMP, and
  the standard factory packages.

Do not revert any game to the built-in render pipeline (it is deprecated). All materials must use
URP shaders (`Universal Render Pipeline/Lit`, base map `_BaseMap`); the built-in `Standard` shader
renders solid magenta under URP.

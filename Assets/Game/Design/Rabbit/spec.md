# spec.md

## 1. Purpose

This asset is a rigged, realistic 3D rabbit creature that populates the ambient wildlife layer of the "Overgrown Facility" lobby environment. It appears as passive background wildlife in the 3D lobby scene (lobby2d_local / World scene), wandering and idling within designated terrain areas to reinforce the theme of nature reclaiming an abandoned industrial facility. The rabbit is non-interactive set dressing: it does not participate in gameplay logic, chat, or player systems, and exists purely to add environmental life and atmosphere alongside the deer and birds referenced in the wildlife plan.

This is a re-run of a previously closed-but-broken issue. The prior attempt produced a spec with no valid GLB output. This spec supersedes it and specifies rigging on the vendored fox quadruped skeleton (shared with the deer creature) rather than a bespoke rig, to reduce risk and reuse validated pipeline work.

## 2. Visual Style

- Realistic (not stylized/cartoon) small-mammal proportions: compact body, long upright ears, short fluffy tail, powerful hind legs, small forelegs.
- Fur rendered via a textured material (diffuse/albedo + normal map) approximating short fur, not geometry-based fur strands. No shader-based fur simulation.
- Color palette: natural wild rabbit coloring -- mottled brown/grey-brown body (base tones roughly #6B5A45 to #8A7860), lighter cream/off-white underbelly and inner ears (#D8CBB8), darker brown shading on ears tips and back (#4A3C2C). Avoid unnatural colors (no pure white, no pink, no cartoon patterns).
- Tone should match the deer creature and general facility environment: slightly desaturated, muted, "reclaimed nature" palette rather than bright/saturated storybook colors. Matte, non-glossy fur finish; eyes have a small specular highlight only.
- Reference: real-world cottontail/European wild rabbit anatomy and coloring. Match the fidelity level and shading approach already used on the deer creature asset for visual consistency within the same wildlife set.
- No accessories, no clothing, no anthropomorphism.

## 3. Dimensions & Format

- File format: `.glb` (single self-contained binary glTF, embedded textures).
- File path: `Design/lobby2d_local/Assets/Game/Sprites/RabbitCreature/RabbitCreature.glb`
- Scale: real-world scale in meters, approx. 0.35m body length (nose to base of tail), approx. 0.25m height at ear tip when standing neutral -- must match Unity's 1 unit = 1 meter convention used elsewhere in the scene.
- Polycount target: low-to-mid poly, suitable for a real-time background creature seen at mid-to-far distance -- target under 6,000 triangles.
- Texture resolution: single 1024x1024 (or 2048x2048 if normal map is included) texture set, power-of-two, packed as albedo + normal (+ optional roughness/metallic if using a PBR material setup consistent with the deer asset).
- Rig: skeleton reused/retargeted from the vendored fox quadruped skeleton (the same rig used for the deer creature). Bone naming and hierarchy must match that shared skeleton exactly so animation retargeting and the shared `GlbCharacterAnimator` component work without per-creature exceptions.
- Animations baked into the GLB as named animation clips:
  - `Idle` -- looping, subtle breathing/ear-twitch/weight-shift, neutral standing pose, no locomotion.
  - `Walk` -- looping, forward locomotion cycle matching quadruped gait timing of the shared skeleton (compatible root motion or in-place, matching whichever convention the deer asset uses).

## 4. Content Description

- A single static rigged rabbit mesh, no variants, no LODs required for this pass.
- Neutral standing pose as the bind/rest pose (all four feet on ground, ears up, head level, tail down).
- No text, no UI, no icons associated with this asset.
- States/Animations:
  - Idle (looping clip): default resting state when the rabbit is stationary. Subtle life-like motion only (breathing, occasional ear twitch) -- no locomotion, no root displacement.
  - Walk (looping clip): used when the wildlife AI/behavior system moves the rabbit between points. Standard quadruped walk gait, four-beat leg cycle, matches the timing/scale conventions of the deer's walk clip so both creatures look consistent when moving through the same environment.
- No hop/jump animation is in scope for this issue (rabbits are being treated as generic quadruped wildlife using the fox-skeleton rig, not with rabbit-specific hopping locomotion). If hopping is desired later, that is a new issue (see Open Questions).
- No death, hit, flee, or interaction animations in scope.
- No sound, no particle effects, no UI elements bundled with this asset.

## 5. Dependencies

- Issue #1 (base project/environment setup) -- must be complete, as stated in the original issue.
- Vendored fox quadruped skeleton must exist and be finalized in the project before this rig can be authored against it (shared with the deer creature asset -- if the deer rig changes, this asset must be re-verified against it).
- `GlbCharacterAnimator` (referenced as "pending" in the PM conversation) -- the Dev-side component responsible for loading and playing GLB-embedded animation clips at runtime must exist and be functional before this asset can be integrated into the scene, even though the asset file itself can be produced independently.
- Confirmation from Dev on which wildlife approach (3D creature vs 2D sprite) was finalized -- conversation indicates 3D was chosen for wildlife including rabbits; this spec proceeds on that basis.

## 6. Open Questions

- Should the rabbit use rabbit-specific hopping locomotion instead of a generic quadruped walk gait, given it is rigged on a fox skeleton not built for hopping? Current direction (per reopen note) is to accept the fox-skeleton walk gait for consistency/pipeline simplicity -- needs explicit design sign-off that a "walking rabbit" (rather than hopping) is acceptable visually.
- Does the shared fox skeleton's bone hierarchy actually accommodate rabbit-specific anatomy (notably the long ears and short tail) without visual distortion, or does the rig need custom ear/tail bones added on top of the shared skeleton?
- What is the confirmed root-motion convention (in-place vs root-displaced) used by the deer's Walk clip, so this asset's Walk clip matches it exactly for the shared `GlbCharacterAnimator`?
- Is a single texture/material sufficient, or does the wildlife system require a specific shader (e.g., a custom fur/subsurface shader) already established by the deer asset that this rabbit must also use for visual parity?
- Does the wildlife AI system require any additional baked animations (e.g., an alert/idle-vigilant pose, a flee trigger) for a future pass, or is Idle + Walk sufficient for all current wildlife behavior needs?
- What is the expected in-scene population count and spawn area for rabbits, and does that impose any additional polycount/texture budget constraints beyond the general target stated above?
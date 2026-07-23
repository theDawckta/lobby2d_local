# spec.md

## 1. Purpose

This asset is an animated particle sprite sheet depicting drifting pollen and spores, used as an ambient environmental effect throughout the "Overgrown Facility" lobby scene. It reinforces the derelict, reclaimed-by-nature tone of the space by adding subtle atmospheric motion to the air.

The sprite sheet will be driven by a particle system (e.g. Unity Shuriken/VFX particle emitter) which will handle spawn rate, drift path, fade in/out, and randomized scale/rotation per instance. This spec covers only the visual asset (the sprite sheet frames), not the particle system configuration.

Expected placement in-game:
- Ambient background layer in the main lobby environment, drifting slowly across open air spaces (near broken windows, shafts of light, overgrown floor areas).
- Rendered behind or alongside player avatars, non-interactive, purely atmospheric.
- Loops continuously for the duration the lobby scene is active.

## 2. Visual Style

- Art direction: soft, organic, slightly luminous particles that read as floating plant spores/pollen catching ambient light, not sharp geometric confetti.
- Shape language: irregular, small, roundish blobs with soft edges -- no hard outlines. Some frames may show gentle drifting "wisp" trails to suggest airborne motion.
- Color palette: warm, muted, nature-toned. Primary tones drawn from the facility's overgrown palette:
  - Pale yellow-green (#D8E29A range)
  - Dusty gold (#C9A85C range)
  - Faint moss green (#8FA66B range)
  - Highlights near-white with low opacity to suggest catching light
- Tone: gentle, quiet, slightly melancholic -- reinforces the abandoned-but-alive feeling of the facility. Not cheerful or cartoonish; not sharp sci-fi.
- Reference points: dust motes in sunbeams, dandelion seed drift, spores from moss/fungus in overgrown ruins concept art. Should feel like it belongs in the same world as the "weathered metallic circuitry with moss and cracks" floor and "buckled metal with vines" walls from the broader lobby aesthetic.
- Opacity: particles are semi-transparent throughout their lifecycle (never fully opaque), consistent with a fine airborne particulate rather than a solid object.

## 3. Dimensions & Format

- File format: PNG, straight alpha transparency (RGBA), no background color.
- Individual frame size: 64x64 px (particle is small on-screen; texture only needs to support close-up and mid-distance use without visible blur).
- Sprite sheet layout: single texture atlas, uniform grid, 4x2 (8 frames total), each cell 64x64 px, total sheet size 256x128 px.
- Frame order: left-to-right, top-to-bottom, representing one full loop cycle of a single pollen particle's subtle pulsing/drifting motion (in place -- actual world-space drift/translation is handled by the particle system, not baked into the frames).
- No trim/padding variance between cells; all cells same pixel dimensions for consistent UV slicing.
- Delivered file path (existing): `Design/lobby2d_local/Assets/Game/Sprites/PollenSpore/PollenSpore.png`

Note: a second variant sprite (`PollenParticle`) was planned per PM conversation but generation failed (ECONNREFUSED to local image generation service). This variant needs to be regenerated -- see Open Questions.

## 4. Content Description

- Depicts: a single small pollen/spore particle, roughly circular/irregular blob shape with soft, feathered edges (not a hard circle).
- No text of any kind.
- States/animation: single continuous loop, no discrete idle/hover/active states (this is a non-interactive ambient prop, not a UI element).
- Animation content across the 8 frames:
  - Subtle pulsing scale (slight grow/shrink, approx 90-110% size) to suggest organic floating motion.
  - Slight opacity flicker (approx 60-90% alpha) to suggest catching/losing ambient light as it drifts.
  - Slight shape wobble frame-to-frame (soft deformation, not rigid rotation) to avoid a mechanical/spinning look.
- The sheet is designed to be tiled/looped seamlessly: frame 8 blends back into frame 1 with no visible pop or jump.
- Color variation: the particle may shift subtly between pale yellow-green and dusty gold across the loop to add visual interest, but should never become fully saturated or opaque.
- Scale, rotation, translation (world-space drift), spawn/despawn fade, and randomization across multiple instances are all handled by the particle system at runtime, not by this sprite sheet.

## 5. Dependencies

- Issue #1 (foundational lobby/environment setup) must be completed before this asset can be integrated, per the issue's stated dependency.
- Requires a particle system component to be configured in-engine to consume this sprite sheet (spawn rate, lifetime, drift velocity/path, world-space scale, additive or alpha blending mode). This configuration is a Dev task, not covered by this spec.
- No font or CoreAssets UI primitive dependencies (this is not a UI element).

## 6. Open Questions

1. The `PollenParticle` variant sprite failed to generate due to a local connection error (ECONNREFUSED 127.0.0.1:8188) and needs to be regenerated. Should this be a distinct second particle variant (different shape/color) for visual variety in the particle system, or a duplicate/backup of `PollenSpore`? Needs confirmation before regeneration.
2. Should the particle system use additive blending (glowing, light-catching look) or standard alpha blending (softer, more matte look)? This affects whether the sprite's baked-in highlight opacity needs adjustment.
3. What is the intended on-screen size range (in world units) for these particles once instantiated by the particle system? This affects whether 64x64 source resolution is sufficient or whether a higher-res source is needed for close-up camera framing.
4. Should pollen density/behavior differ near specific environmental features (e.g. denser near broken windows/light shafts, sparser in shadowed areas)? This is a particle system placement question for Dev but may inform whether additional color/brightness variants are needed.
5. Per the PM conversation, is there a need for a distinct "spark" version of this ambient effect (referenced separately as "flickering sparks" in the environment description), or does that require its own separate spec/ticket?
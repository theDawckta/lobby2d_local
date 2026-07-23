# spec.md

## 1. Purpose

This asset is a rigged, textured 3D bird creature that populates the "Overgrown Facility" environment as ambient wildlife. It appears in the game world (not the lobby UI) as a background/ambient life element that reinforces the theme of nature reclaiming an abandoned industrial facility. The bird wanders and idles within visible or peripheral areas of the map to add life, movement, and atmosphere to the scene. It is not an interactive object, NPC with dialogue, or gameplay-relevant entity -- its sole function is environmental storytelling and ambience.

This asset depends on the wildlife direction established for Issue #31 (3D creature approach, per PM/Dev conversation, as opposed to 2D sprite-based wildlife).

## 2. Visual Style

- **Species reference**: A small-to-medium songbird (e.g. sparrow, robin, or similar generalized passerine bird) -- not an exotic or fantastical species. Should read as a plausible wild bird that would inhabit an overgrown, semi-natural facility.
- **Tone**: Realistic, naturalistic rendering. Proportions, feather groupings, and coloring should resemble real-world bird anatomy rather than a cartoon or stylized mascot design.
- **Color palette**: Muted, natural bird tones -- browns, tans, soft grays, with a small accent color (e.g. rust-orange breast patch or subtle blue-gray head tone) to keep it visually readable against foliage and metallic environment textures. Avoid bright, saturated, or cartoonish colors.
- **Material feel**: Matte, soft-textured plumage rather than glossy/plastic. Subtle color variation across feather groups (head, back, wings, breast, tail) rather than a single flat color.
- **Mood/tone alignment**: Should feel calm, small-scale, and non-threatening -- consistent with ambient wildlife rather than a creature of narrative importance. Complements the "Overgrown Facility" theme (nature reclaiming industrial ruin) without drawing focus away from the environment or players.
- **Reference direction**: Think real-world sparrow/finch reference photography for proportions and feather grouping, simplified to a game-appropriate polycount (see Dimensions & Format).

## 3. Dimensions & Format

- **File format**: GLB (glTF binary), containing mesh, skeleton/rig, skin weights, baked animation clips, and embedded or referenced textures.
- **Scale**: Real-world scale, approximately 12-18 cm body length (beak to tail tip), matching a small songbird. Modeled at 1 Unity unit = 1 meter convention.
- **Polycount target**: Low-to-mid poly for real-time rendering as background ambience -- target approximately 2,000-4,000 triangles. Not a hero asset; should not carry a AAA-character-level budget.
- **Texture maps**: 
  - Base color (albedo) texture, 1024x1024 (or 2048x2048 if the pipeline standard requires higher res for texel density consistency with other game assets).
  - Normal map, same resolution as base color.
  - Optional roughness/metallic or specular map if the project's material pipeline uses PBR (metallic should be at or near zero for feathers).
- **Rig**: Standard bone-based skeleton sufficient to drive idle and walk animations -- minimum bone set: root, spine/body, neck, head, tail, left/right wing (simple, non-flapping-detail unless flight is needed later), left/right leg with foot bones for walk cycle.
- **Animation clips baked into the GLB**:
  - `Idle` -- looping, subtle body/head movement (breathing motion, occasional head turn/tilt, blink if feasible), standing in place.
  - `Walk` -- looping ground locomotion cycle (hopping or walking gait, consistent with small bird locomotion -- most small birds hop rather than walk; confirm which gait is desired, see Open Questions).
- **File placement**: `Assets/Game/Models/Wildlife/Bird/Bird.glb` (or equivalent per-game models directory if one already exists in the repo -- check `Assets/Game/` structure before finalizing path).

## 4. Content Description

- **What is depicted**: A single small songbird model in a neutral standing pose (default/bind pose), fully textured with realistic plumage coloring and a visible beak, eyes, legs, and feet.
- **Text**: None. This is a 3D model asset with no in-model text, UI, or labels.
- **States/animations**:
  - **Idle**: Looping animation showing the bird standing still with subtle idle motion -- slight body bob (breathing), occasional head tilt/turn, optional blink. No vocalization animation required at this stage (audio is out of scope for this ticket).
  - **Walk**: Looping locomotion animation showing the bird moving across the ground. Gait style (hop vs. walk) must be confirmed (see Open Questions) since most small birds hop rather than walk with alternating legs.
  - No hover/active/pressed states apply -- this is a 3D world creature, not a UI element, so standard UI-state conventions (idle/hover/pressed) do not apply here.
  - No death, flight, or interaction animations are in scope for this ticket. If flight or additional behaviors are needed later, that should be scoped as a separate ticket.
- **Rig requirement**: Must be a true skeletal rig with skinned mesh (not a series of separate baked meshes), so that the Idle and Walk clips can be blended or swapped at runtime by whatever animation controller (Dev-owned, e.g. `GlbCharacterAnimator`) drives the creature in-engine.

## 5. Dependencies

- **Issue #1** -- must be completed first (per stated ticket dependency). This establishes foundational project/scene setup that this asset integrates into.
- **Dev: `GlbCharacterAnimator` (pending, per PM/Dev conversation)** -- this is the runtime component intended to drive 3D creature animation playback (idle/walk switching). The bird GLB should be authored with clip names and rig conventions compatible with whatever this component expects (bone naming convention and animation clip naming should be confirmed with Dev before final export -- see Open Questions).
- **Environment art direction for "Overgrown Facility"** -- the bird's color palette and material style should stay visually consistent with the broader environment set (floor/wall textures, foliage) being produced under related tickets for this theme, so that it does not visually clash once placed in-scene.

## 6. Open Questions

1. **Gait confirmation**: Should the Walk animation be a hopping gait (both feet together, typical of small songbirds) or a walking gait (alternating legs, typical of larger ground birds like pigeons/crows)? This affects rig design and animation authoring and should be confirmed with Design/Dev before production.
2. **Bone naming and clip naming convention**: What naming convention does `GlbCharacterAnimator` (pending Dev component) expect for skeleton bones and animation clip names? Needed before final GLB export to avoid rework.
3. **Placement and scale in-scene**: Will the bird be placed via a spawner/prefab system with fixed scale, or does it need to support runtime scale variation (e.g. multiple birds of slightly different sizes for variety)?
4. **Behavior scope**: Is a flight animation or takeoff/landing behavior needed in a future ticket, and if so, should the rig for this initial version anticipate wing articulation for flapping, or is the current wing rig (idle/walk only) sufficient?
5. **Species specificity**: Is a specific real-world bird species required (for licensing, reference accuracy, or thematic reasons), or is a generalized/composite songbird design acceptable? Confirm before finalizing color palette and proportions.
6. **Multiple wildlife reuse**: Given the PM/Dev conversation also references deer and rabbits as wildlife under the same "3D creature" decision, should this bird rig/skeleton structure be treated as a template or reference for a shared wildlife animation pipeline, or is each creature fully independent in rig structure?
7. **Texture resolution standard**: Does the project have an established texel density or texture resolution standard for background/ambient creatures (as opposed to hero characters) that this asset should conform to, to stay consistent with other environment and creature assets?
8. **GLB export/render pipeline verification**: Given the prior failed attempts noted in the issue history (silent failure on the mesh generation path), what is the confirmed working export pipeline/tool for producing this GLB, and is there a way to verify the file lands correctly on disk before this ticket is marked complete?
# spec.md

## 1. Purpose

This asset is a rigged, textured 3D deer creature that populates the "Overgrown Facility" environment as ambient wildlife. It appears in the main explorable lobby/world space, wandering the overgrown facility grounds to reinforce the theme of nature reclaiming an abandoned industrial site. The deer is a non-interactive (or lightly interactive, pending Dev input) background creature intended to add life, scale, and atmosphere to the world. It is one of several wildlife types (deer, rabbits, birds) planned for this environment, and is the largest and most visually prominent of the three.

This is a 3D asset deliverable: a model file plus animation clips, not a 2D sprite or UI element. No flat-fill or engine-primitive substitute is possible here; this requires actual modeling, texturing, rigging, and animation work.

## 2. Visual Style

- **Art direction**: Realistic (not stylized/cartoon) deer, consistent with the grounded, semi-realistic tone of the "Overgrown Facility" theme established by the metallic circuitry floor and buckled metal walls.
- **Species reference**: Common whitetail or red deer body type -- realistic proportions, no fantasy elements (no antlers glow, no unnatural coloration, no armor/tech attachments unless a future ticket specifies environmental corruption effects).
- **Color palette**: Natural, muted earth tones -- browns, tans, and off-white underbelly/tail patch, consistent with real-world deer coloration. Coat should read as slightly weathered/muted rather than glossy, to sit naturally against the desaturated, overgrown facility palette (moss greens, rust oranges, dull metal grays).
- **Tone**: Calm, neutral, non-threatening. The deer should look at home in a quiet, reclaimed ruin -- not aggressive, not cartoonish, not hyper-stylized.
- **References**: Realistic wildlife representations as seen in games like Red Dead Redemption 2 (ambient deer), The Last of Us (overgrown facility wildlife), or Firewatch's naturalistic animal proportions. Avoid low-poly/stylized treatments (e.g., no faceted low-poly art style) unless Dev/PM signal a lower fidelity target is required for performance.

## 3. Dimensions & Format

- **Model format**: glb (per PM conversation reference to `GlbCharacterAnimator`), containing mesh, skeleton/rig, materials, and baked animation clips in a single file.
- **Scale**: Real-world scale, approximately 1.1 - 1.3 meters at shoulder height, approximately 1.7 - 2.0 meters nose-to-tail, matching real whitetail/red deer proportions. Exported at 1 unit = 1 meter to match Unity's default scale conventions.
- **Polycount target**: Mid-poly, real-time game-ready. Target range 8,000-15,000 triangles for the base mesh (final number to be confirmed with Dev based on performance budget for ambient/background creatures; see Open Questions).
- **Texture maps**: Albedo/base color, normal map, and roughness/metallic (or specular, depending on the project's render pipeline) map. Texture resolution 2048x2048 recommended for the main body atlas; can be reduced to 1024x1024 if this is a background/distant creature with a small on-screen footprint (see Open Questions).
- **Rig**: Standard quadruped skeleton (spine chain, neck, head, four legs with appropriate joint chains, tail). Rig must be compatible with Unity's animation import pipeline (generic or humanoid-equivalent quadruped rig setup, whichever the `GlbCharacterAnimator` system expects -- to be confirmed with Dev).
- **Animation clips**: Two baked clips embedded in the glb:
  - `Idle` -- looping, neutral standing pose with subtle idle motion (breathing, ear twitch, head sway, weight shift).
  - `Walk` -- looping, natural quadruped walk cycle at a slow, ambient wandering pace.
- **File location**: Assets/Game/Models/Wildlife/Deer/ (to be created; no existing wildlife model folder currently exists in Assets/Game).

## 4. Content Description

- **Subject**: A single realistic deer model, depicted in a neutral standing pose as its bind/rest pose.
- **No text, no UI elements, no icons** -- this is a pure 3D world asset.
- **States/Animations**:
  - **Idle**: Deer stands still with subtle ambient motion -- slow breathing (chest rise/fall), occasional ear flick, slight head turn or graze-down motion. Loops seamlessly.
  - **Walk**: Deer moves forward at a slow, relaxed wandering pace with a natural quadruped gait (correct diagonal leg pairing, head bob, tail sway). Loops seamlessly and is designed to work with root motion or in-place looping, whichever Dev's `GlbCharacterAnimator` expects (see Open Questions).
  - No "hover" or "active/pressed" states apply, as this is not a UI element. If the deer is meant to react to player proximity (e.g., alert/flee state), that is out of scope for this ticket and would require a follow-up issue.
- **Texturing**: Fully textured coat (fur-pattern baked into albedo, not a shader-based fur system), including natural color variation across the body (darker along the spine, lighter belly/tail), visible in all three material maps (albedo, normal, roughness/metallic).
- **Rig/skeleton**: Visible only in-engine via animation; not a rendered feature, but required as part of the deliverable for animation playback.

## 5. Dependencies

- **Issue #1** -- listed as a direct dependency in the original issue; must be completed before this asset can be integrated (confirm scope of #1 with Dev/PM, as it is not detailed in this ticket's text).
- **Dev: `GlbCharacterAnimator` component** -- referenced in the PM conversation as "pending." This system must exist and its import/animation requirements (rig type, clip naming conventions, root motion vs. in-place looping) must be finalized before this model can be created to spec, since those constraints affect how the rig and animations must be authored.
- **Dev decision on 3D vs 2D wildlife** -- the PM conversation flags this as an open decision point ("Given the 'Overgrown Facility' theme, 3D creatures might look better, but 2D sprites are simpler"). This spec assumes the 3D path has been chosen per the issue title and Implementation section ("Generate a rigged 3D model of a deer"). If Dev/PM later reverse this decision in favor of 2D sprite-based wildlife, this spec is void and a new 2D sprite-sheet spec would be required instead.
- **Environment art direction** -- final deer coloration/weathering should be checked against the finished "weathered metallic circuitry with moss and cracks" floor and "buckled metal with vines" wall textures (separate tickets) to ensure the deer reads well against the environment's final palette.

## 6. Open Questions

1. **Rig/animation pipeline requirements**: What exact rig structure, bone naming convention, and clip-naming format does `GlbCharacterAnimator` require? This must be confirmed with Dev before modeling begins, since rig setup is not easily changed after the fact.
2. **Root motion vs in-place animation**: Should the Walk cycle use root motion (deer translates via animation) or in-place looping (translation handled by a separate movement/AI script)? This affects how the animation is authored and baked.
3. **Polycount and texture resolution budget**: Is 8,000-15,000 triangles and a 2048x2048 texture atlas appropriate, or does the project have a stricter performance budget for ambient wildlife (especially if multiple deer instances may be visible at once)? Should LODs be produced as part of this ticket or a follow-up?
4. **Behavior scope**: Is this deer purely decorative/ambient (wanders on a fixed path or simple AI), or will it need additional states in the future (alert, flee, graze) that should be planned for now to avoid re-rigging later? This ticket only covers Idle and Walk per the acceptance criteria, but flagging for future-proofing.
5. **Multiple wildlife consistency**: Since rabbits and birds are also planned per the PM conversation, should this deer's rig/animation approach (glb, `GlbCharacterAnimator`) serve as the template for those tickets, or will each creature type have bespoke requirements? Confirming now avoids inconsistent pipelines across the three wildlife tickets.
6. **Render pipeline material setup**: Is the project using Built-in, URP, or HDRP? This determines whether the deer's material maps should be authored as Metallic/Roughness or Specular/Glossiness, and affects shader assignment on import.
7. **Interaction with player**: Does the deer need a collider for player collision/blocking, or is it purely a visual/walk-through ambient element? Not required for this spec but relevant for Dev integration.
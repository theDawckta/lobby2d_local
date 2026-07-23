# spec.md

## 1. Purpose

The Supply Crate is a central interactive prop in the multiplayer lobby environment ("Overgrown Facility" derelict high-tech setting). It functions as the focal gathering point of the lobby space, visually communicating the theme of abandoned technology reclaimed by nature. Players interact with it as a shared object whose "opened" state is synchronized across all clients in the lobby session (see Dependencies for the networking hook that drives this).

This asset appears:
- In the center of the lobby scene, as the primary landmark players spawn around.
- As the target of a networked interaction trigger (open/closed state visible to all connected players).
- In any promotional/marketing screenshots or trailers depicting the lobby.

This is a 3D model deliverable, not a 2D sprite. No flat image asset is required; the output is a rigged, textured, animated 3D model file plus its associated texture maps.

## 2. Visual Style

- **Art direction:** Realistic, semi-hard-surface industrial prop. Reads as military/scientific supply storage left behind after facility abandonment.
- **Tone:** High-tech-gone-derelict. The crate should look like it was built with advanced tech (reinforced metal edges, sealed locking mechanism, faint stenciled tech markings) but has since been overtaken by time and environment (rust streaks, dirt grime, moss creeping at base and seams, a few small cracks in outer plating).
- **Color palette:**
  - Base metal: dark gunmeal gray / gunmetal blue (#3A3F44 range) with worn edges showing lighter scratched metal (#8C9096 range).
  - Rust and grime accents: burnt orange / umber (#7A4B2A, #5C3A1E) concentrated at seams, hinges, and base.
  - Moss/organic growth: desaturated green (#4C5A3A, #6B7A4A) limited to crevices, base, and lid seam -- should read as accent, not overwhelm the metal.
  - Tech accents: a single small status light or latch indicator in a muted amber or cyan (#B8862E or #3A8C8C) to imply dormant technology.
- **Reference feel:** Sci-fi military supply cache (e.g. Half-Life 2 supply crates, Returnal storage pods, Dead Space storage lockers) crossed with an overgrown/abandoned facility aesthetic (rust, vine growth, cracked paint). Not cartoonish; low-to-mid poly realism with PBR texturing, not stylized/toon shading.
- Must visually integrate with the "weathered metallic circuitry with moss and cracks" floor and "buckled metal with vines" wall textures used elsewhere in the lobby (see Issues covering those textures) -- consistent grime/moss treatment and metal tone across all facility props.

## 3. Dimensions & Format

- **Model file format:** FBX (preferred for Unity import with embedded rig/animation) or glTF/GLB if the team's pipeline (per PM's `GlbCharacterAnimator` note) standardizes on glb for animated props. Confirm with Dev which importer path is active before final export (see Open Questions).
- **Scale:** Real-world scale, approximately 1.0m (W) x 0.7m (D) x 0.8m (H) closed, matching a large reinforced storage crate. Modeled at true scale in Unity units (1 unit = 1 meter).
- **Poly budget:** Target 3,000-6,000 triangles for the closed crate body, plus lid/interior geometry revealed on open (additional 1,000-2,000 tris for interior tray/interior walls). Suitable for real-time rendering with multiple instances/players in view.
- **Textures:** PBR texture set at 2048x2048 per map:
  - BaseColor/Albedo
  - Normal
  - Metallic/Roughness (packed, single texture, per Unity's Standard/URP metallic workflow)
  - Ambient Occlusion (optional, can be baked into a separate map or packed into an unused channel)
  - Emissive (small mask, for the status light accent only -- mostly black except the light element)
- **Rig:** Simple 2-bone rig minimum: one bone for the base/body (static/root), one bone for the lid, hinged at the rear edge to allow the lid to swing open. If an interior tray or additional articulated panel is included, add bones as needed but keep the skeleton minimal -- this is a prop rig, not a character rig.
- **Animation clip:** One animation clip named `Open` (or `SupplyCrate_Open` per project naming convention), authored as a one-shot: plays once from closed to open and holds the final open pose on the last frame (no auto-loop, no return-to-closed). Clip length: approximately 1.0-1.5 seconds.
- **Delivery location:** `Assets/Game/Models/Props/SupplyCrate/` containing the model file, texture set, and any associated material asset.

## 4. Content Description

**What is depicted:**
A single reinforced storage crate, roughly waist-to-chest height, constructed of riveted metal panels with a hinged lid on top. The crate shows clear signs of both its high-tech origin and its long abandonment:

- Reinforced metal edge trim/corner brackets (slightly lighter metal, scratched).
- A latch/lock mechanism on the front, appearing disengaged/broken (implying it can now be opened freely).
- One small embedded light/indicator near the latch, dark/unlit by default (emissive map present but not necessarily driven by a script -- static dim glow is acceptable; confirm with Dev if this should be an actual controllable emissive material for future scripting).
- Rust streaks running down from seams and rivets.
- Patches of moss/small plant growth clinging to the base and along the lid seam, plus one or two small cracks in the side plating with a few strands of vine detail.
- No large-scale foliage covering the crate -- growth should be subtle accenting, not obscuring its readability as a crate.

**Text:** No readable text is required. A faint stenciled tech marking or serial number graphic (illegible/abstract, like worn stencil lettering) may be included in the albedo texture as a detail element, but must not be legible language -- purely a visual texture detail implying "military/industrial equipment."

**States:**
1. **Closed (default/idle state):** Lid fully down, latch visible, this is the resting pose the model loads into.
2. **Open (post-interaction state):** Lid rotated open on its hinge (~100-120 degrees from closed, resting past vertical so it reads clearly as "open" from typical player camera angles), revealing a simple interior (dark cavity or basic interior paneling -- no specific loot items need to be modeled unless a future issue specifies contents).

**Animation:**
- Single one-shot clip: `Open`. Plays once when triggered, animating the lid from closed to open pose, and holds on the final open frame indefinitely (no reverse/close animation required per this issue's scope).
- No idle animation loop is required beyond the static closed pose.
- No hover/rollover animation is specified in this issue -- if a hover-highlight visual (e.g., outline or glow on mouseover) is needed, that is a separate UI/VFX ticket, not part of this model deliverable.

## 5. Dependencies

- **Issue #1** (per ticket) -- must be completed before this asset can be integrated. Confirm scope of #1 covers foundational project/scene setup required for prop placement.
- **Networking integration (non-blocking for art production, but blocking for final in-game integration):** Per the PM/Dev conversation, the open-state synchronization is expected to use a `OneShotPropAnimator` component combined with a `NetworkedEntity` hook via `WorldConnection`. This model must expose a single clean animation clip (`Open`) with a clear start and end pose so it can be driven by that component. Confirm with Dev that clip naming and rig hierarchy match what `OneShotPropAnimator` expects before final export.
- **Shared environment texture consistency:** This asset's moss/rust/grime treatment should be visually cross-checked against the floor and wall texture tickets (weathered metallic circuitry floor, buckled metal walls with vines) once those are produced, to ensure a consistent material language across the lobby scene.

## 6. Open Questions

1. **File format confirmation:** Should the final model be delivered as FBX or GLB? This depends on whether the project standardizes on `GlbCharacterAnimator`-style pipeline (as floated for wildlife) for all rigged props, or uses FBX for props and glb only for characters/creatures. Needs Dev confirmation.
2. **Emissive light behavior:** Should the small status light on the crate be a purely static dim texture element, or does Dev want a real-time controllable emissive material property (e.g., to light up or change color when the crate is interacted with)? This affects whether the light is baked into the albedo/emissive texture or needs to be a separate material slot exposed for scripting.
3. **Interior detail scope:** Does the open state need a modeled/textured interior (e.g., foam padding, shelving) or is a simple dark cavity sufficient for this issue? Future issues may add loot/item props inside the crate -- if so, interior geometry may need to accommodate anchor points for those items.
4. **Close animation:** This issue only specifies a one-shot open animation that holds open. Confirm whether a close animation or reverse-playback capability will ever be needed (e.g., for a "reset lobby" feature), so the rig/animation can be planned to support it later without rework.
5. **LOD requirements:** Given multiple players may view this crate simultaneously in a shared lobby, does the project need an LOD (level of detail) variant for performance, or is a single mesh sufficient given expected lobby player counts?
6. **Interaction trigger visualization:** Is a separate hover/highlight indicator (glow, outline, prompt icon) expected to accompany this crate for player interaction affordance? If so, that should be scoped as its own ticket rather than folded into this model deliverable.
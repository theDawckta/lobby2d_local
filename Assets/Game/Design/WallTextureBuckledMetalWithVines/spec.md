# spec.md

## 1. Purpose

This asset is a seamless, tileable wall texture used throughout the "Overgrown Facility" lobby environment. It is applied to wall geometry/wall-facing quads across the derelict facility interior, providing the base surface material for all vertical wall surfaces the player sees while navigating the lobby space.

This texture works in conjunction with the floor texture (see dependency below) to establish the derelict, reclaimed-by-nature industrial tone of the lobby. It is a raw material texture only -- no props, characters, or scene-specific objects are baked in, so it can be reused and tiled across walls of any length or height in the level.

## 2. Visual Style

- Aesthetic: derelict industrial facility, long-abandoned and partially reclaimed by nature.
- Base material: corrugated or paneled metal wall siding, heavily buckled, dented, and warped as if from structural stress, age, or impact damage. Visible seams, rivets, or panel joins consistent with an industrial facility.
- Surface degradation: rust streaking (orange-brown, dark umber), oxidation blotches, scratched and flaking paint (suggest a faded industrial color such as olive-drab, dull gray, or safety yellow showing through in patches).
- Overgrowth: vines with leaves creeping across the metal surface -- growth should look organic and irregular, not a repeating decorative pattern. Vines in muted green tones (moss green, olive, dark green) with some dried/brown vine sections for variety.
- Color palette: primarily desaturated cool grays and browns (metal base), with rust accents (burnt orange, umber), and organic greens (moss green, forest green, olive) for vines/foliage. No bright saturated colors. Overall value range should be mid-to-dark to support a moody, dim lobby atmosphere.
- Tone: quiet, decayed, atmospheric -- not cartoonish or stylized-cute. Should read as "nature has been slowly taking over this structure for years."
- Reference touchpoints: abandoned industrial facility concept art, overgrown bunker/factory interiors, rust and vine texture studies (similar mood to derelict game environments like abandoned research stations reclaimed by plant life).
- Lighting baked into texture should be flat/neutral (no strong directional shadow baked in) so it reads correctly under the lobby's actual dynamic/ambient lighting.

## 3. Dimensions & Format

- Resolution: 512x512 pixels.
- Format: PNG, no alpha channel needed (fully opaque wall material).
- Tiling: must be seamlessly tileable on both horizontal and vertical axes -- edges must match exactly when repeated in a grid with no visible seams, offsets, or repeating "tell" patterns.
- Delivered as a single flat texture file (not a sprite atlas or sliced asset).
- File location: `Assets/Game/Sprites/DerelictWallMetal/DerelictWallMetal.png`

## 4. Content Description

- Depicts a section of buckled/warped metal wall paneling with rust discoloration and patchy faded paint.
- Vines with leaves growing across portions of the surface, irregular and organic in placement, not covering the entire texture (should leave enough exposed metal to read clearly as "metal wall," roughly 60-70% metal visible, 30-40% vine/foliage coverage).
- No text of any kind.
- No props, crates, pipework, signage, characters, or other scene objects baked into the texture -- this is a pure surface material only.
- Static image, no animation, no distinct states (idle/hover/active are not applicable to a tileable surface texture).
- Texture must tile cleanly in a repeating grid without visible seams; damage/rust/vine placement should be distributed so no obvious single repeating motif is easily spotted when tiled across a large wall surface.

## 5. Dependencies

- Issue #1 (base facility/material style foundation) must be completed before this texture is finalized, to ensure visual consistency with the established derelict facility material language (matching rust tone, metal panel style, and vine/foliage treatment used elsewhere, e.g. the floor texture).
- Should be visually consistent with the companion floor texture (weathered metallic circuitry with moss/cracks) so walls and floors read as part of the same environment kit.

## 6. Open Questions

- Should this wall texture share an exact color palette / rust-tone swatch with the floor texture to guarantee visual cohesion, or is close-but-not-identical acceptable given they are different surface types (wall siding vs. floor plating)?
- Is a single 512x512 tile sufficient for all wall surfaces in the level, or will larger walls need a second variant texture to avoid visible repetition at scale (e.g. a "variant B" panel with different vine/rust placement)?
- Should normal map / height map data be produced alongside the color texture to support lit material response on the buckled metal, or is this a flat-lit sprite/material with no additional texture maps expected at this stage?
- Confirm current lighting model in the lobby scene (unlit sprite vs. lit material) so the flat/neutral lighting assumption in this texture is correct.
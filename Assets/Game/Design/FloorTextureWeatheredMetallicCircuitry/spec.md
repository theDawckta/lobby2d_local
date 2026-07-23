# spec.md

## 1. Purpose

This asset is a tileable floor texture used as the primary ground surface throughout the lobby environment (the "Overgrown Facility" theme, derelict high-tech setting). It is applied as a repeating material across all walkable floor areas the player and other avatars traverse in the 2D lobby scene. This texture establishes the core visual identity of the facility as an abandoned, once-advanced technological space, and forms the base layer beneath any separate moss/crack overlay work (tracked separately, not part of this ticket).

## 2. Visual Style

- **Art direction:** High-tech derelict facility. The surface should read as an old circuit board or server-room floor panel that has been abandoned for a long time.
- **Tone:** Weathered, industrial, slightly ominous but not horror-themed. Sense of dormant technology rather than active danger.
- **Color palette:** Dark neutral base (charcoal, gunmetal gray, dark slate) with muted copper, tarnished silver, and faint oxidized green-blue circuit traces. Avoid bright saturated colors. Any glowing elements (if included) should be dim, faded, or fully dark to reinforce the "powered down" feel.
- **Surface detail:** Etched circuit-board line patterns, embedded metal plating seams, subtle rust/oxidation stains, scratches, and worn edges. Panel seams should be part of the tile design, not obviously repeating in a way that breaks the illusion of a continuous floor.
- **References:** Think old server room raised floor panels, sci-fi derelict spaceship interiors, weathered PCB (printed circuit board) close-ups, industrial grating with corrosion. Avoid a "clean sci-fi" or "shiny chrome" look -- this should feel neglected and aged.
- **Consistency:** Should visually complement (not necessarily match exactly) the wall texture and any other derelict-facility surface work being produced under the same environment theme, if/when those tickets exist.

## 3. Dimensions & Format

- **Size:** 512x512 pixels.
- **Format:** PNG, no alpha channel needed (fully opaque floor base).
- **Tiling:** Must be seamless and tileable on both horizontal and vertical axes -- no visible seams, repeating artifacts, or mismatched edges when tiled in a grid.
- **Delivery:** Single flat texture file, intended for use as a repeating material/sprite tile in Unity. No atlas or slicing required.

## 4. Content Description

- Depicts a weathered metallic floor surface with embedded circuitry-style detailing: etched line traces, metal panel seams, corrosion/oxidation staining, and general wear (scratches, scuffs, small dents).
- No moss, vines, cracks-with-vegetation, props, objects, debris, or characters are baked into this texture -- this ticket covers the bare metal/circuitry surface only. Any moss/crack overlay is a separate asset layered on top.
- No text or readable labels of any kind.
- No animation -- this is a static texture.
- No distinct states (idle/hover/active) -- this is an environment surface texture, not an interactive UI element.
- The pattern should have enough irregularity in wear and staining that the eye does not immediately detect the repeat, while still tiling perfectly at the pixel level.

## 5. Dependencies

- **Issue #1** must be completed before this asset can be integrated (per issue dependency list). Confirm with Dev what #1 delivers (likely core scene/material pipeline setup) before this texture is wired into the lobby floor material.
- No CoreAssets or Assets/Game/UI primitives apply here since this is an environment texture, not a UI element.

## 6. Open Questions

- Per the PM/Dev conversation: is a separate transparent moss/crack overlay texture being scoped as its own ticket, or should faint corrosion/staining suggestive of future overlay placement be included in this base texture's design (without actual moss/vine geometry)? Assume separate ticket unless told otherwise.
- Should this floor texture visually coordinate with a specific wall texture (buckled metal with vines), e.g. shared color grading or matching panel seam scale? Awaiting confirmation on whether the wall texture is a separate ticket or a material variant of this same texture.
- Confirm target tile density in-scene (i.e., how large one 512x512 tile should appear relative to a player avatar) so the level of surface detail (trace line thickness, seam spacing) is scaled appropriately and doesn't look either too fine or too blocky when tiled across the play area.
- Confirm whether any faint emissive/glow elements (e.g., a few still-lit circuit traces) are desired for atmosphere, or if the facility should read as fully powered-down/dark in this texture.
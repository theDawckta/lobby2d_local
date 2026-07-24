# spec.md

## 1. Purpose

This asset is the 360-degree environmental skybox for the game's main lobby. It provides the immersive backdrop visible in every direction as the player looks around the lobby space, establishing the overgrown-derelict-facility tone before the player proceeds into the rest of the game. It is a background-only environmental element -- it does not contain gameplay-relevant props, characters, or interactive elements. It is rendered as the Unity Skybox material applied to the lobby scene's camera/render settings, not as a flat UI backdrop or 2D sprite.

## 2. Visual Style

- Setting: interior of a large derelict industrial facility, abandoned and reclaimed by nature over time.
- Architecture: decayed industrial structure -- rusted metal support beams, corroded catwalks, collapsed or partially collapsed ceiling sections, cracked concrete and metal flooring/walls visible at all horizontal angles.
- Nature overtaking structure: moss, vines, and creeping plant growth covering walls, beams, and floor areas. Patches of damp discoloration and water staining on metal and concrete surfaces.
- Lighting: hazy volumetric light shafts streaming down through breaks in the ceiling, creating shafts of light cutting through dust/mist in the air. Overall lighting is moody and diffused, not harsh -- suggests filtered daylight from above mixing with dim ambient interior gloom.
- Color palette: desaturated industrial grays and rust-oranges for metal/concrete, muted mossy greens for vegetation, soft hazy off-white/pale gold for the light shafts. Avoid saturated or cartoonish colors -- tone should read as somber, quiet, atmospheric.
- Mood/tone: quiet, melancholic, abandoned-but-alive. Consistent with the lobby's existing overgrown-facility art direction (matches established mood of other lobby assets -- no stylistic deviation such as cheerful, bright, or sci-fi-clean).
- Reference cues: post-apocalyptic industrial interiors, abandoned factory/power-plant atria, nature-reclaimed brutalist architecture. Avoid exterior sky, clouds, or open-air horizon elements -- this is an interior space, so "up" and "down" should read as ceiling and floor, not sky and ground.

## 3. Dimensions & Format

- Format: single equirectangular panoramic image, 360-degree horizontal wrap, 180-degree vertical field.
- Aspect ratio: 2:1.
- Resolution: 2048x1024 pixels.
- File format: PNG (no alpha channel needed -- fully opaque environment).
- No atlas or slicing required -- single continuous equirectangular texture, to be assigned as a Skybox (Panoramic / Cubemap-equivalent) material in Unity.
- Left and right edges of the image must tile seamlessly (pixel-continuous wrap) with zero visible seam when mapped 360 degrees around the viewer.
- No visible frame, border, vignette, watermark, or text anywhere in the image.
- Poles (top/bottom of the equirectangular projection, mapped to "up"/ceiling and "down"/floor from the viewer) should resolve coherently with no visible distortion artifacts, stretching seams, or blank/solid-color caps -- ceiling detail (broken structure, light shafts) at the top pole, floor detail (concrete/moss) at the bottom pole.

## 4. Content Description

- Full 360-degree interior environment of the derelict facility as described in section 2, viewable seamlessly in every horizontal direction.
- Ceiling (top of panorama): partially collapsed industrial ceiling with visible gaps/breaks, structural beams, hazy light shafts pouring through the openings.
- Walls (horizon band): rusted metal wall paneling, corroded support structures, catwalks or gantries, moss and vine growth climbing the surfaces, damp staining.
- Floor (bottom of panorama): concrete or metal flooring, moss patches, minor rubble/debris texture baked into the surface material itself (not standalone 3D-readable props), water pooling or damp patches consistent with the decayed setting.
- Atmosphere: visible haze/dust motes suspended in the light shafts, soft depth fog toward more distant/darker areas of the facility to sell scale and depth.
- Explicitly excluded: no ground-level props (crates, machinery, furniture) rendered as distinct baked objects, no characters or creatures, no readable text/signage, no frames or UI borders, no flat "2D backdrop" framing (must read as a continuous enterable space, not a painted flat backdrop with an implied edge).
- States/animations: none. This is a single static texture. No hover/active states, no animated variants. (Any light-shaft flicker or dust motion, if desired, would be handled via a separate particle/shader effect in-engine, not baked into this texture.)

## 5. Dependencies

- Lobby scene/level layout must be finalized to the extent that camera height and expected sightlines within the lobby are known, so the skybox horizon line and ceiling break placement read correctly from the player's typical viewpoint.
- Confirmation of the lobby's established overgrown-facility art direction reference set (concrete color palette/mood references used for other lobby assets) should exist and be accessible, so this skybox matches rather than diverges stylistically.
- Unity scene must have Skybox render settings/material slot ready to receive a panoramic skybox material (standard Unity skybox pipeline; no custom shader work identified as required, but confirm render pipeline -- Built-in, URP, or HDRP -- as this affects which skybox shader/material type consumes this texture).

## 6. Open Questions

- Confirm target render pipeline (Built-in / URP / HDRP), since the skybox material type (Skybox/Panoramic vs Skybox/Cubemap conversion) differs and may affect how this equirectangular PNG is consumed.
- Should the skybox include a directional light-source cue (i.e., should light shaft direction align with an actual in-scene directional light for shadow consistency), or is this purely a visual backdrop with no lighting interaction required?
- Is any subtle animation (e.g., drifting dust, flickering light shafts) planned via a future in-engine effect layered on top of this static skybox, and if so should this texture avoid baking in strong directional light shafts that would look static/mismatched against animated dust?
- Should floor-level detail bias toward darker/less-detailed treatment if the player camera in the lobby rarely looks straight down, to avoid over-investing pixel detail in a rarely-seen pole region?
- Given the prior two failed generation attempts were infrastructure/timeout failures rather than spec issues, confirm no spec changes are needed before the next generation retry -- reissue as-is unless art direction feedback emerges from a successful render.
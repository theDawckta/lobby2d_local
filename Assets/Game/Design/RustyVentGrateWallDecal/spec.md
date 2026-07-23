# spec.md

## 1. Purpose

This asset is a single decorative wall decal depicting a rusted metal vent grate. It is used as a static environment prop within the Overgrown Facility lobby, applied to wall surfaces to add industrial detail and reinforce the sense of a decayed, abandoned facility. It functions the same way as existing decals such as MossAndCrackOverlay and DerelictWallMetal: placed via the GroundDecalScatter-style system (despite the name, used here for wall-mounted set dressing) to break up flat wall geometry with grounded, believable detail.

This decal does not carry gameplay function. It is purely atmospheric/environmental art, contributing to the read of the space as a long-neglected industrial structure being reclaimed by rust and rot.

Appears in: Overgrown Facility - Lobby (walls), and potentially reused in other Overgrown Facility interior areas that share the same wall material set.

## 2. Visual Style

- Art direction: decayed industrial realism, consistent with the Overgrown Facility's established look (see MossAndCrackOverlay, DerelictWallMetal for tonal reference).
- Subject: a rectangular vent grate cover, the kind mounted flush or slightly protruding from a wall, with horizontal or louvered slats allowing implied airflow behind it.
- Surface treatment: heavily oxidized metal. Rust blooms concentrated at screw/bolt points, seams, and slat edges where moisture collects. Streaking rust drips bleeding downward from the grate onto the implied wall below (the streaks should be part of this texture's bottom edge, so they read naturally when placed on a wall, but should not extend so far as to require a large canvas).
- Color palette: muted greys (base metal, #4a4a48 to #6b6b66 range), rust oranges and umbers (#8a4a2c, #6b3a20, #a35c34), dark grime/soot blacks in recessed grooves (#201d1a), faint muted green-brown grime consistent with the facility's moss/overgrowth motif in the darkest recesses only (subtle, not a dominant color).
- Lighting: flat-ish ambient lighting baked into the texture, slightly darker at the grate's recessed slats and edges to sell depth, with subtle highlight catch on raised rivets/bolt heads to suggest worn metal sheen. No strong directional light source baked in, so it reads correctly regardless of in-scene lighting angle.
- Edges: the grate itself should have a slightly irregular, worn silhouette (corners of the mounting frame chipped or corroded away in places) rather than a perfect rectangle, and the texture must fade/feather to transparency at the outer edge so it blends into the wall material rather than showing a hard box.
- Tone: matches the quiet, oppressive decay of the rest of the facility -- not cartoonish rust, not overly saturated.

## 3. Dimensions & Format

- Format: PNG, RGBA (transparent background required outside the grate silhouette).
- Dimensions: 256x384 px (portrait orientation), consistent with a small wall-mounted fixture rather than a full wall texture.
- Not tileable -- this is a standalone decal, not a repeating surface material.
- No atlas required; single standalone file.
- Alpha channel must cleanly separate the grate/rust-streak silhouette from surrounding transparency so it composites correctly over any wall base texture.

## 4. Content Description

- Depicts: one (1) rusted vent grate, wall-mounted, roughly rectangular with 4-6 horizontal louvered slats.
- Mounting details: four visible corner bolts/rivets, rust-stained at each fastening point.
- Surface wear: patchy rust coverage (not uniform) -- heavier rust at slat edges and corners, lighter/more intact metal near the center if the design calls for visual hierarchy.
- Bottom edge: subtle vertical rust-stain streaks bleeding down from the grate's lower edge, tapering to transparency, to suggest runoff staining the wall below the fixture.
- No text or legible signage on the grate.
- No animation -- this is a static sprite/decal.
- No interactive states (no idle/hover/active variants) -- this is environment set dressing, not a UI or interactive object.
- Single final image only (the earlier attempts at variants A/B/C were exploratory failed generations; this spec calls for one definitive asset).

## 5. Dependencies

- None. This ticket has no upstream dependencies and can be produced independently of other work.
- Engineering note (non-blocking): confirm the decal will be placed using the existing decal/scatter placement tooling already used for MossAndCrackOverlay and DerelictWallMetal, so no new placement system is required.

## 6. Open Questions

- Should this decal have a "closed/intact" visual variant for use in less-decayed areas of the facility, or is a single fully-rusted version sufficient for all placements? (Current scope assumes single version only.)
- Confirm whether the image generation pipeline outage (ECONNREFUSED 127.0.0.1:8188) noted in prior attempts is resolved before re-running generation; this is an infrastructure blocker, not a design ambiguity, but flagging since it previously prevented delivery.
- Should rust-streak bleed extend further down (implying a taller decal/larger canvas) to interact with floor-level moss decals, or should streaking stay fully contained within the 256x384 canvas as specified?
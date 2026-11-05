> ARCHIVE — Étude naturaliste précédente. La version active et ses mesures sont décrites dans Art/Illustrated/Sources/DIRECTION_ARTISTIQUE.md.

# Prototype_Maquis — art pass validation

Date: 2026-10-22. Unity 6000.6.7f1, Windows Editor.

## Saved scene

`Assets/Scenes/Prototype_Maquis.unity`, active, saved, outside Play mode.

The departure area contains 85 branching shrubs suggesting lentisque, ciste and bruyere, 250 dry grass clumps, 150 small stones, eight original rock objects with six eroded mesh variants, textured ground relief and a lower-resolution surrounding horizon. Vegetation is combined into 15 spatial mesh chunks. A clear-sky material, one warm directional sun and distance fog provide the lighting and horizon.

## Geometry and textures

Aggregate active scene geometry, including the distant ground: 93,320 triangles, 220,254 vertices, 33 mesh renderers. These are scene counts, not measured draw calls or headset frame rates. Shared opaque foliage materials avoid alpha blending. Decorative stones and plants have no physics colliders; the route remains clear. There are no runtime generation components or extra dynamic lights. Six 1024px textures use mipmaps, anisotropy 2 and a WebGL ASTC 6x6 import override. Normal maps use linear normal-map import. The gameplay camera has post-processing disabled.

The existing global render-pipeline assets were preserved. WebXR export, browser texture support, stereo rendering costs and frame time on a physical Quest 3 have not been tested. A device/browser profiling pass is still needed before claiming a frame-rate target.

## Functional checks

Completed in Play mode using the existing CharacterController and movement method:

- Gravity and grounding at Depart_WebXR pass. Camera local height remains 1.65 m; the grounded controller skin adds approximately 3 cm during simulation.
- The walking route through five waypoints, from z=-11 to z=11, passes without obstruction. Terrain raycasts remain beneath the player.
- All eight matching rock mesh colliders stop forward movement on contact.
- Mouse release passes. The desktop controller source is unchanged from the preceding version, whose ZQSD, arrows and mouse behavior were tested.
- Returned to Edit mode and saved the scene.
- Final Unity console check: zero errors and zero warnings.
- SampleScene SHA-256 unchanged: 07FCE8B04804DDCCB3026DB5B43D8DC41BA138E49D50D36A5C24877081EACDCA.
- Desktop controller SHA-256 unchanged: DB0AF3E81C217C9F09CF1CAEEB2A8EAA255FD0B2CEA46548E98A849CE56B73D7.

Texture authors, original download URLs, licenses and hashes are retained in `MAQUIS_SOURCES.md` and `manifest.json` beside this report.


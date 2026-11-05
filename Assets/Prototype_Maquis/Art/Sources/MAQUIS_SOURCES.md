> ARCHIVE — Étude naturaliste précédente. La version active et ses mesures sont décrites dans Art/Illustrated/Sources/DIRECTION_ARTISTIQUE.md.

# Maquis departure art — sources and reuse

Created for CorseVR / Prototype_Maquis on 2026-10-22.

## External textures

All six original JPEG files are retained under `Textures/`, unchanged. `manifest.json` records their exact download URLs, SHA-256 hashes and byte sizes. Runtime import is limited to 1024 pixels, with mipmaps; normal maps are imported as linear normal maps. The WebGL override uses ASTC 6x6 for the intended Quest browser target. Actual browser build support and performance still require device testing.

| Set | Author(s) | Source | Files used | License |
| --- | --- | --- | --- | --- |
| Rocky Trail | Amal Kumar | https://polyhaven.com/a/rocky_trail | 1K diffuse, OpenGL normal | CC0-1.0 |
| Brown Mud 02 | Rob Tuytel | https://polyhaven.com/a/brown_mud_02 | 1K diffuse, OpenGL normal | CC0-1.0 |
| Rock Boulder Dry | Dimitrios Savva (photography), Rico Cilliers (processing) | https://polyhaven.com/a/rock_boulder_dry | 1K diffuse, OpenGL normal | CC0-1.0 |

Provider license statement: https://polyhaven.com/license

CC0 deed and legal terms: https://creativecommons.org/publicdomain/zero/1.0/ and https://creativecommons.org/publicdomain/zero/1.0/legalcode

Poly Haven permits reuse, modification and redistribution, including commercial use, under CC0. No third-party models were imported. Attribution is retained here for provenance. Downloads used the original asset file service, with no runtime network or API dependency.

## Original project assets

The terrain, eroded boulder meshes, pebble scatter, branching shrubs, individual leaves, dry grasses, shaders and editor builder were authored for this prototype. The deterministic generator source is `Editor/MaquisArtBuilder.cs`; its saved meshes and materials are ordinary Unity assets, requiring no runtime generation. These newly authored procedural assets may be reused and modified with the project; no additional third-party restrictions apply to them.

The vegetation suggests lentisque, ciste and bruyere forms. It is an artistic approximation, not a botanically exact species collection. Habitat reference (no images or models copied): Conservatoire du littoral, Agriate: https://www.conservatoire-du-littoral.fr/siteLittoral/42/UTB_RESET/1/5-agriate-2b_haute-corse.htm

## Scope and performance choices

The departure area receives the detailed vegetation and stones. Surrounding low-resolution hills provide an atmospheric horizon. The 30 x 30 m walking ground uses a matching non-convex mesh collider; eight original rock objects receive matching mesh colliders. Small decorative pebbles and vegetation are non-blocking and are kept out of the central walking corridor. The original path primitives are retained inactive.

One directional sun, a procedural sky, static geometry, shared materials and spatial mesh chunks avoid per-plant GameObjects, alpha-blended foliage, runtime generation and additional lights. Desktop controls are preserved. No WebXR package, deployment or on-device performance claim is included in this art pass.

To regenerate: open Prototype_Maquis in Edit mode, then CorseVR > Build Maquis departure art. The builder only targets this scene. It preserves the player and controller. Manually placed changes inside Maquis_Depart_Detail are replaced on regeneration.


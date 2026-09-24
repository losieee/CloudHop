# Platform artwork
Original transparent PNGs are stored in Art/Platforms/Platform1.png through Platform8.png.

- Stage 1 / lab course 1: grassy islands (1–3), long island (4) for wide platforms.
- Stage 2 / lab course 2: grassy islands (2, 5) and exposed stone (8).
- Stage 3 / lab course 3: ice (6, 7) and exposed stone (8).
- Prototype and reusable Platform prefab: grass (1).
- Ice is a visual theme only; no slippery physics has been introduced.

PlatformArtwork keeps the sprite on a separate child, scales it proportionally to the existing collider width, and aligns its authored surface pivot with the collider top.
The original marker renderer is hidden but retained for existing builder and gimmick references.
Crumble warning tint and disappearance alpha are mirrored onto the art. Moving platforms carry the child automatically.
Positions, colliders, scoring and difficulty layouts remain unchanged. Decorative foliage and lower rocks are not extra collision surfaces.

To change a platform skin, replace the child PlatformArt Sprite and configure the artwork opaque width (sprite units). Source image pixels are unchanged.
PlatformArtInstaller is the initial editor setup utility; running it again reapplies the default theme assignments to all Cloud Hop scenes.

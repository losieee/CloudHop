# Layered background
Six original PNGs are imported unchanged under Art/Backgrounds.
The supplied complete gameplay mockup is a composition reference, not a flattened game background.

Hierarchy: Layered Background
1. Distant Mountains (image 6): scroll speed 0.08
2. Distant Castle (image 5): 0.12
3. Floating Islands (image 4): 0.22
4. High Clouds (image 2): 0.30
5. Mid Clouds (image 3): 0.40
6. Lower Cloud Sea (image 1): 0.50

Each layer has seven reusable tiles, negative sorting order, no collider.
BackgroundLayer: Scroll Speed, Repeat Distance, Offset and Tiles are Inspector settings.
Positions derive from the camera directly, so long travel, Retry and stage transitions do not accumulate drift.
BackgroundSky applies the bright blue sky behind the transparent assets.
Art, gameplay platforms, characters and screen UI remain separate layers.
Original source alpha edges are retained; no image repainting was performed.

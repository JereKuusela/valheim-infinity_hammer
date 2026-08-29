# Test fixtures

`fixtures/terrain-blueprint-contract.blueprint` is a minimal interoperability and manual regression fixture for terrain snapshots.

It deliberately places the first piece at X = -2 while the terrain grid is centered at X = 1. This catches multi-piece placement code that incorrectly uses the first child instead of the blueprint root. The height and paint sections are both 3 columns by 2 rows and contain one empty cell, which consumers must leave unchanged.

Manual round-trip check:

1. Load and place the fixture at a known position and yaw.
2. Confirm that the terrain grid is positioned from the blueprint root, not from the first floor piece.
3. Save the active selection under a new name.
4. Place it immediately, then reload the saved file and place it again at the same position and yaw on clean terrain.
5. Compare the affected nodes. Both placements must use the same anchor, search radius, height values, and paint values.

For paint, include both untouched biome paint and a clear-vegetation cell. Place the snapshot over different destination paint and confirm both cells are explicit final values; an unmodified `TerrainComp` cell must not leak destination paint into the result.

## Rotated center regression

`fixtures/terrain-rotated-center.blueprint` uses a center piece at `(2, 1, 0)` with yaw 90 degrees, while both terrain channels start at center `(5, 10, 7)` with reference yaw 30 degrees.

After `Blueprint.Center("wood_floor")`:

- The center piece must be at the origin with identity rotation.
- The terrain center must be approximately `(3.268, 9, 8)` in X, Y, Z order.
- The terrain reference yaw must be approximately 120 degrees.
- Height rows must retain their order and become `10;11` and `12;13` after subtracting the root Y offset.
- Paint rows must remain unchanged.

Repeat placement at yaw 0, 90 and 180 degrees. The terrain must retain the same offset and orientation relative to both pieces.

## Future section regression

`fixtures/terrain-future-section.blueprint` puts valid-looking payload rows after unknown and malformed headers. A parser must:

- Load exactly one piece (`wood_floor`), never `future_payload`.
- Load a 2 by 2 height grid; `999` and `888` must not be appended to it.
- Load the following 2 by 2 paint grid normally.

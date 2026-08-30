# Blueprints

Basic support is provided for BuildShare .vbuild and [PlanBuild](https://valheim.thunderstore.io/package/MathiasDecrock/PlanBuild/) .blueprint files.

## Placing blueprints

Blueprints can be selected with the `hammer_blueprint [file name]` command and then placed with the hammer.

Recommended to use `hammer_menu blueprints` for an easier access.

The command has following advanced parameters:

- `center=object`: If given, the blueprint is centered around this object.
  - Note: The object is not included in the selection.
- `data=true/false`: If false, object data is not loaded.
- `scale=number`: If given, the blueprint is scaled by this factor.
  - The scale is only applied to objects that can be scaled.
  - Install [Structure Tweaks](https://valheim.thunderstore.io/package/JereKuusela/Structure_Tweaks/) for all clients to unlock scaling for all objects.
- `snap=object`: If given, this object is placed at the each snap point.
  - This can be used to modify the snap points.
- `smooth=height,paint`: If given, overrides how gradually the blueprint's terrain height and/or paint snapshots are applied (0.0-1.0, 0 = exact, 1 = fully gradual).
  - Either value can be left empty to keep the configured default, for example `smooth=,0.5` only overrides paint.
  - Has no effect if the blueprint doesn't contain a terrain height/paint snapshot.

If the blueprint has no snap points, some are automatically generated.

The `hammer_restore [file name]` command selects the blueprint at its originally saved position and rotation instead of the current placement ghost location. It supports the same `center`, `data`, `scale`, `snap` and `smooth` parameters as `hammer_blueprint`.

## Creating blueprints

New PlanBuild blueprints can be created with the `hammer_save [file name]` command.

If file name is not given, a text input is shown.

The command has optional parameters. Their default values can be set in the config.

- `center=object`: If given, the blueprint is centered around this object.
  - Note: The object won't be included in the blueprint.
- `data=true/false`: If false, object data is not saved.
- `snap=object`: If given, these objects are converted into snap points.
  - Note: The snap objects won't be included in the blueprint.
  - Option `all` adds every snap point of every piece to the blueprint.
  - Option `auto` tries to select reasonable snap points from the snap points of every piece.
- `profile=true/false`: If true, the blueprint is saved into the mod profile folder.

Note: Infinity Hammer also stores the object data when creating blueprints. This can significantly increase the file size and cause incompatibility with future PlanBuild versions. If needed, disable "Save blueprint data" from the config.

### Terrain snapshots

Area selections can include the final terrain height and paint values. These are snapshots of the selected terrain nodes, not a list of terrain operations. Placing the blueprint applies the saved values once relative to the blueprint root, so the result doesn't depend on the order of the original terrain edits.

Terrain is stored in two optional sections after `#Pieces`:

```text
#TerrainHeight:<center x,z,y>;<reference yaw>;<node spacing>
<height or empty>;<height or empty>;...
#TerrainPaint:<center x,z,y>;<reference yaw>;<node spacing>
<r:g:b:a or empty>;<r:g:b:a or empty>;...
```

- Header vectors and all numbers use invariant decimal notation. The vector order is X, Z, Y.
- The center is relative to the blueprint root. Height samples are relative to the root Y coordinate.
- Each following line is one Z row and each semicolon-separated field is one X column. Every row in a section has the same number of columns.
- Empty fields are outside the captured shape and must not modify terrain.
- The first node is `center - ((columns - 1) * spacing / 2, 0, (rows - 1) * spacing / 2)`.
- Reference yaw records the terrain capture reference. Placement uses the yaw difference between this value and the blueprint root yaw.
- A section ends at the next line beginning with `#` or at the end of the file.
- Consumers without terrain support must skip the section rows until that boundary instead of interpreting them as pieces or snap points.
- These sections are an Infinity Hammer/Expand World Data extension. Current PlanBuild versions do not understand the snapshot rows, so terrain-enabled files must not be loaded directly in PlanBuild until it adds support for this contract.
- Blueprint object scaling does not scale the terrain grid.

When a blueprint is recentered, terrain rows keep their X/Z order. Given the selected center offset `T`, its inverse rotation `K` as applied to piece positions, terrain center `C` and reference yaw `R`, the terrain contract transforms as follows:

```text
C' = C - R * T
R' = R * inverse(K)
height' = height - T.y
```

Paint values and empty cells are unchanged.

The "Save simpler blueprints" option only writes mandatory piece data and omits terrain sections.

Following data is not copied:

- Object scale (redundant because the blueprint has own fields or the scale).
- Creature spawn coordinates (harmful because creatures try returning to the spawn coordinates when idle).
- Snap points (currently no good way to edit them).

## Configuration

- Blueprint center piece: The default center piece when saving blueprints.
- Blueprint folder (default: `PlanBuild`): Folder relative to the config folder. Both profile and base Valheim folders are searched for .blueprint and .vbuild files.
- Blueprint snap piece (default: `auto`): The default snap piece when saving blueprints.
- Build Share folder (default: `BuildShare/Builds`): Folder relative to the Valheim.exe.
- Include terrain height (default: `false`): If enabled, hammer area selection captures terrain height.
- Include terrain paint (default: `false`): If enabled, hammer area selection captures terrain paint.
- Save blueprint data (default: `true`): If enabled, object data values are saved to blueprints.
- Save blueprints to profile (default: `false`): If enabled, blueprints are saved to the profile folder instead of the base Valheim folder.
- Save object data blueprints: Object ids that save extra data when the "Save blueprint data" is disabled.
- Save simpler blueprints (default: `false`): If enabled, only mandatory information is saved.
- Terrain height offset (default: `0`): How much terrain height (in meters)  is captured outside the selected area.
- Terrain height smooth (default: `0`): How gradually blueprint terrain height changes are applied by default (0 = exact, 1 = fully gradual)
- Terrain height spacing (default: `0`): Distance between captured terrain height nodes in meters. 0 = smallest distance.
- Terrain paint offset (default: `0`): How much terrain paint (in meters) is captured outside the selected area.
- Terrain paint smooth (default: `0`): How gradually blueprint terrain paint changes are applied by default (0 = exact, 1 = fully gradual).
- Terrain paint spacing (default: `0`): Distance between captured terrain paint nodes in meters. 0 = smallest distance.
- Use blueprint chance (default: `false`): If enabled, the object chance from blueprints is used.

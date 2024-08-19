# Blueprints

Basic support is provided for BuildShare .vbuild and [PlanBuild](https://valheim.thunderstore.io/package/MathiasDecrock/PlanBuild/) .blueprint files.

## Placing blueprints

Blueprints can be selected with the `hammer_blueprint [file name]` command and then placed with the hammer.

The command has following parameters:

- `center=object`: If given, the blueprint is centered around this object.
  - Note: The object is not included in the selection.
- `data=true/false`: If false, object data is not loaded.
- `scale=number`: If given, the blueprint is scaled by this factor.
  - The scale is only applied to objects that can be scaled.
  - Install [Structure Tweaks](https://valheim.thunderstore.io/package/JereKuusela/Structure_Tweaks/) for all clients to unlock scaling for all objects.
- `snap=object`: If given, this object is placed at the each snap point.
  - This can be used to modify the snap points.

If the blueprint has no snap points, some are automatically generated.

## Creating blueprints

New PlanBuild blueprints can be created with the `hammer_save [file name]` command.

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

Following data is not copied:

- Object scale (redundant because the blueprint has own fields or the scale).
- Creature spawn coordinates (harmful because creatures try returning to the spawn coordinates when idle).
- Snap points (currently no good way to edit them).

## Configuration

- Blueprint center piece: The default center piece when saving blueprints.
- Blueprint folder (default: `PlanBuild`): Folder relative to the config folder. Both profile and base Valheim folders are searched for .blueprint and .vbuild files.
- Blueprint snap piece (default: `auto`): The default snap piece when saving blueprints.
- Build Share folder (default: `BuildShare/Builds`): Folder relative to the Valheim.exe.
- Save blueprint data (default: `true`): If enabled, object data values are saved to blueprints.
- Save blueprints to profile (default: `false`): If enabled, blueprints are saved to the profile folder instead of the base Valheim folder.
- Save object data blueprints: Object ids that save extra data when the "Save blueprint data" is disabled.
- Save simpler blueprints (default: `false`): If enabled, only mandatory information is saved.
- Use blueprint chance (default: `false`): If enabled, the object chance from blueprints is used.

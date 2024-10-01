- v1.59
  - Adds a new command `hammer_room` to add rooms to dungeons.
  - Adds a new field `hammer data=` to load simple data entries.
  - Adds snap points to dungeon rooms.
  - Changes the "Remove anything" feature to remove single rooms from dungeons.
  - Fixes repairing not being able to remove invulnerability.
  - Fixes picking up existing dungeons not copying the rooms.
  - Fixes some undo issues by reworking it (now uses World Edit Commands mod).
  - Fixes some data saving issues by reworking it (now uses World Edit Commands mod).
  - Fixes error with MaterialMan.
  - Fixes error when copying MineRock5 objects.

- v1.58
  - Changes the setting "No creator" to also remove data from X Ray Vision mod.

- v1.57
  - Adds a new setting "Snap points" for automatic snap point generation.
  - Adds a new setting "Blueprint snap piece" to set which piece is used for snap points.
  - Adds a new setting "Zoop magic mode" to do stuff.
  - Adds a new setting "Tool ignored ids" to separate ignored ids for commands.
  - Adds a new setting "Tool included ids" to support included ids for commands.
  - Adds a new parameter to the command "hammer_blueprint" to display the snap points.
  - Adds a new parameter to the command "hammer_save" to override the snap piece.
  - Adds new parameters "include" and "ignore" to the command `hammer`.
  - Adds setting toggling to the command "hammer_config".
  - Changes the format of command "hammer_blueprint" to have named parameters.
  - Changes the format of command "hammer_save" to have named parameters.
  - Fixes blueprint snap points not working at all.
  - Fixes escaping with "" not working on tool commands.
  - Merged Infinity Tools mod back to this mod.
  - Removes setting "Snap points for all objects" as obsolete.
  - Reorganizes the settings.

- v1.56
  - Fixes continuous placement being bit too happy with being placed.

- v1.55
  - Fixes compatibility issue with Marketplace NPC's mod.

- v1.54
  - Fixed for the new game version.

- v1.53
  - Fixes trees falling down during the placement selection.
  - Fixes custom locations from Expand World Data not being able to be placed.

- v1.52
  - Fixed for the new game version.

- v1.51
  - Adds a new setting "Use blueprint chance" to use the object chance from blueprints.
  - Adds a new setting "Save simpler blueprints" to only save the most important data.
  - Changes the "Ignored ids" setting to also prevent the connecting with the `connect` parameter.
  - Fixes compatibility issue with Building mod.
  - Fixes compatibility issue with More Vanilla Build Prefabs mod.
  - Removes description from saved blueprints (was bugged and it's not really needed).

- v1.50
  - Adds a new value "Legacy" to the setting "Set invulnerability".
  - Fixes repairing not working with the setting "Set invulnerability" (some cases, not all).
  - Fixes changing build tab with Q/E not selecting the new object.
  - Fixes build menu showing old selection when selecting existing objects.
  - Fixes copying not always copying the rotation (for example active furnaces).

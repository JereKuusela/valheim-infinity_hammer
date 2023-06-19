- v1.38
  - Fixes custom data and undo/redo not working on servers.
  - Fixes not working with the latest Comfy Gizmo.

- v1.37
  - Fixes custom data.

- v1.36
  - Updated for the new game version.

- v1.35
  - Fixes blueprint scaling not scaling the individual objects.

- v1.34
  - Adds an automatic snap point to non-default objects.

- v1.33
  - Adds a center piece support to blueprints.
  - Adds a new setting "Blueprint center piece" for the default center piece value.
  - Adds a new setting "Save blueprints to profile" to determine whether profile or base Valheim folder is used.
  - Improves placing of dungeon generators.
  - Fixes build protection bypass not working with Expand World.
  - Fixes the `hammer health=` not working for creature max health.
  - Merges the PlanBuild and BuildShare search folders to the same folder.
  - Removes "spawnpoint" data being saved to blueprints (so creatures won't try to return to arbitrary position on the world).
  - Removes snap points from blueprint saving (conflicts with the center piece).

- v1.32
  - Fixes error with the new update.

- v1.31
  - Fixes errors with some mods (possibly crashing the game).

- v1.30
  - Adds a new key bind for the grid.
  - Adds compatibility for Valheim Raft mod.
  - Adds a new setting "No health indicator".
  - Adds a new setting "No support indicator".
  - Adds dimensions for new objects.
  - Renames the setting "Remove effects" to "No effects".
  - Renames the setting "Select blacklist" to "Ignored ids". These now affect most operations.
  - Renames the setting "Remove blacklist" to "Ignored remove ids".
  - Renames the settings "No build cost", "No durability loss" and "No stamina cost" to "No cost".
  - Removes the setting "Repair taming" as obsolete (better do it with World Edit Commands).
  - Removes the setting "Custom binds" as obsolete (better to use the binds.yaml of Server Devcommands).
  - Removes the setting "Enable undo" as obsolete (now always uses Server Devcommands undo if installed).
  - Removes the setting "Server devcommands undo" as obsolete (now always uses Server Devcommands undo if installed).
  - Removes the setting "Copy rotation" as obsolete.
  - Fixes the rectangle shape having the dimensions reversed.

- v1.29
  - Adds a new setting `Snap points for all objects` (default false).
  - Improves compatibility with custom locations from Expand World mod.
  - Fixes error spam when trying to place creatures.
  - Fixes snap points not working when zooping.

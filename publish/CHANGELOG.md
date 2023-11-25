- v1.44
  - Adds minimum radius of 0.25 to tools.
  - Adds a new setting "Precise tools" (locks cursor to terrain grid, disables rotating).
  - Adds new settings to disable specific tool shapes.
  - Adds new command `hammer_tool` to select tools.
  - Adds new command `hammer_export` and `hammer_import` to export and import tools.
  - Adds a new field `highlight` to tools (highlights affected pieces).
  - Adds a new field `snapGround` to tools (can be used to disable tool visual snapping to ground).
  - Adds a new field `instant` to tools (can be used to execute command directly from the menu).
  - Adds a new field `tabIndex` to tools (to define the build menu tab).
  - Adds a new field `index` to tools (to define the position on build menu).
  - Adds offset support to tools.
  - Changes the default key binds for tools (scaling with just mouse wheel).
  - Removes commands `hammer_command`, `hoe_command`, `hammer_add`, `hoe_add`, `hammer_remove` and `hoe_remove`.

- v1.43
  - Adds a new setting to disable blueprint data saving.
  - Fixes repair destroying players (not tested).
  - Fixes copied objects or objects from blueprints sometimes disappearing.

- v1.42
  - Adds support for connected ZDO when copying objects.

- v1.41
  - Fixes copying not working for damaged stone or mineral deposits.

- v1.40
  - Adds a new command `hammer_restore` that selects a blueprint and automatically sets hammer at its position and rotation.
  - Adds a new command `hammer_pos` that allows setting the hammer position and rotation.
  - Changes the `hammer_save` command to save hammer position and rotation to the blueprint file.
  - Changes the `hammer_save` command to print hammer position and rotation to the console.
  - Changes the `hammer_save` command to automatically load the saved blueprint.
  - Changes the `hammer_grid` to use X,Z,Y format instead of X,Y,Z.
  - Fixed for the new patch.

- v1.39
  - Fixed for the new patch.

- v1.38
  - Fixes custom data and undo/redo not working on servers.
  - Fixes not working with the latest Comfy Gizmo.

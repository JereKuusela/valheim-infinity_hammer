# Tools

The hammer (and hoe) can execute any console commands, which provides a quick and an intuitive way to use them. These commands can be found at the build menu with a configurable position.

The custom tools are saved in `config/infinity_tools.yaml`. This file should be edited with a text editor.

Following fields are available:

- name: Name of the tool.
- description: Description of the tool.
  - Keywords `<mod1>`, `<mod2>` and `<alt>` can be used to show the modifier keys.
- icon: Icon of the tool.
  - Valid icons are object ids, skill ids and status effect ids.
- command: Command to execute.
  - Various parameters can be used inject coordinates and other information to the command.
  - Use `<area>` parameter to select the place and size with the mouse.
  - Use `<place>` parameter to select the place with the mouse.
- continous: If true, the command is executed continuously while holding the mouse button.
- initialHeight: If set, the height parameter is set to this value when the tool is selected.
  - This can be useful if you typically use a specific height, while still allowing changes if needed.
- initialSize: If set, the radius, depth and width parameters are set to this value when the tool is selected.
- initialShape: If set, the shape is set to this value when the tool is selected.
- targetEdge: If true, the targeted position is used as the edge instead of as the center.
  - Key codes are supported to enable/disable this with a modifier key.
  - Causes `<r>` and `<d>` parameters to be automatically calculated from the distance between the player and the targeted position.
- snapGround: If true, the visual lines snap to the ground.
  - Key codes are supported to enable/disable this with a modifier key.
  - This doesn't affect the actual targeting logic, only the visualization.
- playerHeight: If true, the y coordinate is set to the player height.
  - Key codes are supported to enable/disable this with a modifier key.
- highlight: If true, pieces in the area are highlighted.
  - Key codes are supported to enable/disable this with a modifier key.
  - This doesn't affect the actual targeting logic, only the visualization.
- terrainGrid: If true, the selected position and size snaps to the terrain nodes.
  - Key codes are supported to enable/disable this with a modifier key.
  - This can be used for precise terrain changes (especially with the rectable).
  - This disables rotation to align with the nodes.
- instant: If true, the command is executed instantly without any selection.
  - If not set, the value is automatically determined from the command.
- tabIndex: Tab of the tool in the build menu.
  - If not set, the tool is added to the same tab as the previous tool.
- index: Index of the tool in the build tab.
  - If not set, the tool is added after the previous tool.

## Command parameters

Following parameters can be used in the command:

- `<area>`: Includes the coordinate and size parameters for World Edit Commands.
- `<place>`: Includes the coordinate parameters for World Edit Commands.
- `<to>`: Includes the to and size parameters for World Edit Commands.
- `<id>`: Id of the hovered object.
- `<ignore>`: List of ignored ids.
- `<x>`: X coordinate.
- `<y>`: Y coordinate.
- `<z>`: Z coordinate.
- `<a>`: Angle. Mostly matters for rectangles.
- `<r>`: Radius. Enables circle shape.
- `<r2>`: Radius end range. Enables ring shape.
- `<w>`: Square width. Enables square shape.
- `<w2>`: Square end range. Enables frame shape.
- `<d>`: Rectangle depth. Enables rectangle shape.
- `<h>`: Height.
- `<mod1>`: Modifier key 1.
- `<mod2>`: Modifier key 2.
- `<alt>`: Alt key.

To simplify the usage, following aliases are available `tool_object`, `tool_terrain` and `tool_spawn`. These use the `object`, `terrain` and `spawn_objects` commands from World Edit Commands while containing correct parameters for different shapes.

Additionally there are `tool_terrain_to` and `tool_slope` for edge targeted commands.

## Modifier keys

Key codes can be used in some fields but also inside the command. For example `hammer_add goto 5000;goto keys=leftalt` would teleport up or down depending on whether the left alt key was pressed.

The config has keybindings for two modifier keys. These can be used in the commands with a value of `<mod1>` (default value is left alt) and `<mod2>` (default value is left control). The last modifier keys is `<alt>` that is based on the base game setting (defautl value is left shift).

## Configuration

Following powers are available with `hammer_config` command:

- Show command values (default: `false`, key: `show_command_values`): If enabled, the actual command is always shown on tool descriptions.
- Shape circle (default: `true`, key: `shape_circle`): If enabled, the circle shape can be used.
- Shape ring (default: `false`, key: `shape_ring`): If enabled, the ring shape can be used.
- Shape square (default: `true`, key: `shape_square`): If enabled, the square shape can be used.
- Shape frame (default: `false`, key: `shape_frame`): If enabled, the frame shape can be used.
- Shape rectangle (default: `true`, key: `shape_rectangle`): If enabled, the rectangle shape can be used.
- Command height amount (default: `0.1`, key: `command_height_amount`): How much the height changes when zooming.

## Binds

- Command modifier 1 (default: `leftalt`): Key code for the `<mod1>` modifier.
- Command modifier 2 (default: `leftctrl`): Key code for the `<mod2>` modifier.
- Change shape (default: `Q`): Key code for changing the shape.
- Command radius (default: `none`): Modifier key for mouse wheel to change the radius.
  - None means no modifier key is required.
- Command depth (default: `leftshift, leftalt`): Modifier key for mouse wheel to change the depth.
- Command height (default: `leftshift`): Modifier key for mouse wheel to change the height.
- Command rotation (default: `leftshift, leftctrl`): Modifier key for mouse wheel to change the rotation.

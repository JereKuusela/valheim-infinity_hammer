# Infinity Hammer

Bend the rules of building! Copy any object, make structures indestructible, remove all restrictions, destroy anything and more...

Install on the admin client (modding [guide](https://youtu.be/L9ljm2eKLrk)).

Install also [Server Devcommands](https://valheim.thunderstore.io/package/JereKuusela/Server_devcommands/) and [World Edit Commands](https://valheim.thunderstore.io/package/JereKuusela/World_Edit_Commands/).

# Features

- Build anything. Trees, rocks, creatures... All can be placed with the hammer, with a precise placement!
- Copy anything. Armor stands, chests and item stands with their contents. Even boss altars!
- Make structures indestructible, even the gravity can't bring them down.
- Remove anything. Something unremovable messing up your grand design? No more!
- Build without restrictions. Dungeons and even the start temple become valid for building.
- QoL improvements: Extended range, no resource costs, no item drops, no visual effects and more!

Recommended mods:

- [Server Devcommands](https://valheim.thunderstore.io/package/JereKuusela/Server_devcommands/): Enables key binds, undo and better command support.
- [World Edit Commands](https://valheim.thunderstore.io/package/JereKuusela/World_Edit_Commands/): Enables terrain tools and additional commands.
- [Gizmo](https://valheim.thunderstore.io/package/ComfyMods/Gizmo/): Enables free rotation.
- [More Vanilla Builds](https://valheim.thunderstore.io/package/BippityBoppityBoo/MoreVanillaBuilds/): Adds more objects to the build menu.
- [Structure Tweaks](https://valheim.thunderstore.io/package/JereKuusela/Structure_Tweaks/): Allows scaling every object. Everyone must have this mod installed.

Similar mods:

- [Better Creative](https://valheim.thunderstore.io/package/Heinermann/BetterCreative/): Adds most objects to the build menu and some enhancements.
- [OCDheim](https://valheim.thunderstore.io/package/javadevils/OCDheim/): Lots of enhancements, especially for precise building.

# Selecting objects

The main feature of this mod is the ability to build any object with the hammer.

This is done by using the `hammer` console command which works in four ways:

- `hammer`: Selects (and copies) the hovered object.
- `hammer [object id]`: Selects an object by id ([Item IDs](https://valheim.fandom.com/wiki/Item_IDs)).
- `hammer connect`: Selects the hovered building.
- `hammer circle=[radius]` or `hammer rect=[width,depth] angle=[degrees]`: Selects all nearby objects.

Selecting the hovered object can be quickly done by pressing NumPad5.

Holding alt key will select the entire building. Holding ctrl key picks up the objects instead (copy + delete).

The hammer build menu also contains Pipette and Area select tools.

The `hammer` command has following extra parameters which allow modifying the selected objects:

- `freeze`: Instantly freezes the placement position, allowing precise movement with key binds.
- `pick`: Removes the selected object, to more easily move objects.
- `from=x,z,y`: Overrides the player position when doing an area selection.
- `health=number`: Overrides the health.
- `level=number`: Overrides the creature level (stars + 1).
- `scale=number` or `scale=x,z,y`: Overrides the size (if the object can be scaled).
- `stars=number`: Overrides the creature stars (level - 1).
- `text=string`: Overrides the sign text.
- `type=creature/structure`: Select only certain object types.

For Structure Tweaks mod:

- `collision=false`: Removes collision.
- `fall=off/solid/terrain`: Overrides the fall behavior.
- `growth=big/big_bad/small/small_bad`: Overrides the plant growth.
- `interact=false`: Removes interaction.
- `show=false`: Removes visibility.
- `restrict=false`: Removes portal item restrictions.
- `wear=broken/damaged/healthy`: Overrides the structure wear.

For example `hammer Beech1 scale=2 health=1000` would select a beech tree with a double size and 1000 health.

## Scale

The scale can be directly set with following commands:

- `hammer_scale`: Resets the size.
- `hammer_scale [x,z,y or amount]`: Sets the size.
- `hammer_scale_x [amount]`: Sets only the x-axis.
- `hammer_scale_y [amount]`: Sets only the y-axis.
- `hammer_scale_z [amount]`: Sets only the z-axis.
- `hammer_scale_cmd [x,z,y or amount]`: Sets the size of commands.
- `hammer_scale_x_cmd [amount]`: Sets only the x-axis.
- `hammer_scale_y_cmd [amount]`: Sets only the y-axis.
- `hammer_scale_z_cmd [amount]`: Sets only the z-axis.

## Zoom

Shift + mouse wheel will zoom up/down the selection.

This can be also done with following commands:

- `hammer_zoom [x,z,y or percentage or amount]`: Zooms the size.
- `hammer_zoom_x [percentage or amount]`: Zooms only the x-axis.
- `hammer_zoom_y [percentage or amount]`: Zooms only the y-axis.
- `hammer_zoom_z [percentage or amount]`: Zooms only the z-axis.
- `hammer_zoom_cmd [x,z,y or percentage or amount]`: Zooms the size of commands.
- `hammer_zoom_x_cmd [percentage or amount]`: Zooms only the x-axis.
- `hammer_zoom_y_cmd [percentage or amount]`: Zooms only the y-axis.
- `hammer_zoom_z_cmd [percentage or amount]`: Zooms only the z-axis.

For example `hammer_zoom 5%` changes 100 % to 105 % or 200 % to 210 %. While `hammer_zoom -5%` changes 100 % to 95.2 % or 200 % to 190.5 %.

For example `hammer_zoom 0.1` changes 100 % to 110 % or 200 % to 210 %. While `hammer_zoom -0.1` changes 100 % to 90 % or 200 % to 190 %.

Note: If you notice using these commands frequency, make a [key binding](https://docs.unity3d.com/ScriptReference/KeyCode.html) for them.

## Locations

Locations (or [Points of Interests](https://valheim.fandom.com/wiki/Points_of_Interest_(POI))) usually include multiple objects so copying them with the `hammer` command is not simple.

They also have special behavior like random parts or random damage.

That's why there is another command for creating them: `hammer_location [location_id] [seed=0] [random damage]`

The `seed` parameter can be used to set the random parts. If missing or `0`, then the result is randomized. If set to `all` then all parts are forced to be on which allows building complete locations (which are normally very rare).

By the default, the location doesn't get randomly damaged. To set this on, put any value to the fourth parameter.

For example:

- `hammer_location StoneTowerRuins04` creates a stone fortress with random parts (which change when using the command again).
- `hammer_location StoneTowerRuins04 100` creates a stone fortress with some parts (always the same).
- `hammer_location StoneTowerRuins04 all` creates a stone fortress with all parts.
- `hammer_location StoneTowerRuins04 0 1` creates a stone fortress with random parts and random damage.

## Precise placement

Default key bindings allow freezing the placement by pressing NumPad0. This allows moving around while the placement ghost stays in the same place.

Then the placement can be fine tuned with following keys:

- PageUp / PageDown: Move up/down 0.1 meters.
- LeftArrow / RightArrow: Move left/right 0.1 meters.
- UpArrow / DownARrow: Move forward/backward 0.1 meters.

Holding alt key will move by 1 meter instead of 0.1 meters.

Note: Placement keys also work without freezing.

Following commands exist for this:

- `hammer_move [forward,up,right]`: Moves the placement ghost offset for precise placement. Auto value can be used for the object size.
- `hammer_move_backward [meters or number*auto]`: Moves the placement towards the backward direction.
- `hammer_move_down [meters or number*auto]`: Moves the placement towards the down direction.
- `hammer_move_forward [meters or number*auto]`: Moves the placement towards the forward direction.
- `hammer_move_left [meters or number*auto]`: Moves the placement towards the left direction.
- `hammer_move_right [meters or number*auto]`: Moves the placement towards right left direction.
- `hammer_move_up [meters or number*auto]`: Moves the placement towards the up direction.
- `hammer_offset [forward,up,right]`: Sets the placement ghost offset.
- `hammer_offset_x [value]`: Sets the offset in the right / left direction.
- `hammer_offset_y [value]`: Sets the offset in the up / down direction.
- `hammer_offset_z [value]`: Sets the offset in the forward / backward direction.

## Free rotation

Requires installing either [Comfy Gizmo](https://github.com/redseiko/ValheimMods/releases/latest) or [Gizmo Reloaded](https://valheim.thunderstore.io/package/Tyrenheim/M3TO_Gizmo_Reloaded/). Check their documentation how to use them if needed.

For basic usage that is all you need but extra commands are provided for advanced usage.

Fine-tuning the rotation:

- `hammer_rotate_x [degrees]`
- `hammer_rotate_y [degrees]`
- `hammer_rotate_z [degrees]`

Random rotation (90 or 180 degrees depending on the object shape)

- `hammer_rotate_x random`
- `hammer_rotate_y random`
- `hammer_rotate_z random`

Random rotation with custom precision.

- `hammer_rotate_x [number]*random`
- `hammer_rotate_y [number]*random`
- `hammer_rotate_z [number]*random`

For example `3*random` would randomly rotate 0, 120 or 240 degrees.

## Undo / redo

Requires installing [Server Devcommands](https://valheim.thunderstore.io/package/JereKuusela/Server_devcommands/).

Building and removing objects can be quickly reversed by pressing NumPad7. Reversed actions can be redone with NumPad9.

Note: Any other removal like buildings collapsing due to lack of support or destroyed by enemies is not tracked. Only your direct actions can be undone.

## Multiplacement

The selection can be multiplied with zooping:

- `hammer_zoop_[direction] [step=auto]`

Possible directions are `backward`, `down`, `forward`,`left`, `right` and `up`.

The step sets the distance between cloned objects. By default this is the size of the object.

The recommended way is to set a keybinding in the config (with Server Devcommands mod). Custom bindings can also be used for special cases.

Examples:

- `hammer_zoop_right auto`: Clones the selected object to next to it.
- `hammer_zoop_right 2*auto`: Clones the selected object while leaving a gap (equal to the object size).
- `hammer_zoop_up 5`: Clones the selected object to 5 meters above it.

### Stacking

Stacking is similar to zooping but instantly places the objects:

- `hammer_stack_[direction] [amount or min-max] [step=auto]`

The amount determines how many objects are placed. This can be fine-tuned by giving a range.

Note: Choosing the correction direction can be difficult when rotating the selection. But you can easily try things by reverting the changes with `undo`.

Examples:

- `hammer_stack_up 10`: Places 10 objects on top of each other.
- `hammer_stack_up -5-4`: Places 10 objects with 5 below and 4 on top of the main object.
- `hammer_stack_left 5 10`: Places 5 objects with 10 meters between each of them.
- `hammer_stack_left 5 2*auto`: Places 5 objects while leaving a gap between them (equal to the object size).
- `hammer_stack_left 3-4`: Places 2 objects some distance away from the selected position.

To place objects in a rectangle or a box, use the following command:

- `hammer_stack [forward,up,right or z1-z2,y1-y2,x1-x2] [step=auto,auto,auto]`

## Blueprints

Basic support is provided for BuildShare .vbuild and [PlanBuild](https://valheim.thunderstore.io/package/MathiasDecrock/PlanBuild/) .blueprint files.

### Placing blueprints

The command `hammer_blueprint [file name] [center piece] [scale]` allows placing them. If no files are found, configure the source folder.

The center piece determines which object in the blueprint is used as the bottom center point. Usually this is not needed because either the blueprint has the information or the default value from the config can be used.

Scale allow setting the initial scaling. This is also rarely needed because most objects can't be scaled (unless [Structure Tweaks](https://valheim.thunderstore.io/package/JereKuusela/Structure_Tweaks/) is installed for all clients).

### Creating blueprints

New PlanBuild blueprints can be created with `hammer_save [file name] [center piece]` command.

If the center piece is not given, the default value is used from the config.

Note: Infinity Hammer will also store the object data when creating blueprints. This can significantly increase the file size and cause incompatibility with future PlanBuild versions.

Following data is not copied:

- Object scale (redundant because the blueprint has own fields or the scale).
- Creature spawn coordinates (harmful because creatures try returning to the spawn coordinates when idle).
- Snap points (currently no good way to edit them).

## Utility

Some special commands exist for advaced cases. Usually you want to bind these.

- `hammer_add_piece_components`: Adds the Piece component to every object which allows copying them with PlanBuild mod.
- `hammer_grid [precision] [center=current]`: Restricts possible placement coordinates. Using the same command removes the restriction.
- `hammer_measure`: Measures most objects updating `dimensions` setting.
- `hammer_mirror`: Mirrors the selection.
- `hammer_place`: Allows placing with a key press.
- `hammer_repair`: Selects the repair tool for quickly clearing the selection.

For example with [Server Devcommands](https://valheim.thunderstore.io/package/JereKuusela/Server_devcommands/) you could bind command `hammer_rotate_y random;hammer_place` to build objects with a random rotation.

# Executing commands

The hammer (and hoe) can execute any console commands, which provides a quick and an intuitive way to use them. These commands can be found at the build menu with a configurable position.

Unfortunately adding new commands won't be that simple. Probably the easiest way is editing the config directly. That way you can see how the default commands are done.

Commands also exist for this:

- `hammer_add [command]` or `hoe_add [command]`: Adds a new command. Useful for sharing commands with other players.
- `hammer_list [index to clipboard]` or `hoe_list [index to clipboard]`: Prints added commmand and their index numbers. If indes is given, copies to the command to the clipboard for easier sharing.
- `hammer_remove [index]` or `hoe_remove [index]`: Removes the command with the given index.
- `hammer_remove [command]` or `hoe_remove [command]`: Removes all commands that start with the given parameter.

For example `hammer_add killall` would add a new item to the hammer menu that killed all nearby creatures when clicked.

## Command style

By default all added commands have name "Command" and have the command as the description.

This can be customized with parameters `cmd_name`, `cmd_desc` and `cmd_icon`. Valid icons are object ids, skill ids and status effect ids.

For example `hammer_add cmd_name=Kill cmd_desc=Kills_everything cmd_icon=softdeath killall`

## Modifier keys

Multiple commands can be executed by having ; between them. For example `hammer_add goto 5000;killall` would both teleport to the sky and kill all nearby creatures.

`keys` parameter can be used to decide which commands get executed. For example `hammer_add goto 5000 keys=-leftalt;goto keys=leftalt` would teleport up or down depending on whether the left alt key was pressed.

The config has keybindings for two modifier keys. These can be used in the commands with a value of `cmd_mod1` (default value is left alt) and `cmd_mod2` (default value is left control). For example `hammer_add cmd_name=Teleport cmd_icon=Shelter goto 5000 keys=-cmd_mod1;goto keys=cmd_mod1`.

Modifiers key also work for the `hammer_command` or `hoe_command` explained below.

## Command placement

Some commands have coordinates as their parameters. These can be set with hammer/hoe by using commands `hammer_command` or `hoe_command`.

For example using `hammer_command hammer circle=10 from=x,z,y` would select every object within 10 meters of the hovered position.

The radius can be modified by using shift + mousewheel, when using `hammer_command hammer circle=r from=x,z,y` (10 changed to r). This also makes the mod highlight the affected area! For less typing you can use the alias `hammer_area`.

This command can be added to the menu with `hammer_add hammer_area`. It's recommended to always test commands first before adding them.

## World Edit Commands

The full potential of commands can be unlocked by installing [World Edit Commands](https://valheim.thunderstore.io/package/JereKuusela/World_Edit_Commands/) mod. This automatically adds new commands to the hoe (mainly for terrain modifications).

Full list of parameters:

- `id`: Id of the hovered object.
- `x`: X coordinate.
- `y`: Y coordinate.
- `z`: Z coordinate.
- `a`: Angle. Mostly matters for rectangles.
- `r`: Radius. Enables circle shape.
- `r1-r2`: Radius range. Enables circle and ring shapes.
- `w`: Rectangle width. Enables square shape.
- `w1-w2`: Square range. Enables square and grid shapes.
- `d`: Rectangle depth. Enables rectangle shape.
- `h`: Height.

For example `hoe_command terrain from=x,z,y circle=r rect=w,d angle=a raise=h` raises terrain by a variable height.

To reduce typing, use aliases `hoe_object` and `hoe_terrain`. For example `hoe_object tame` or `hoe_terrain raise=h`.

Similar aliases also exist for the hammer (`hammer_object` and `hammer_terrain`).

Key binds to change values:

- Q: Changes the shape (uses `hammer_shape` command).
- Shift + Mouse wheel: Circle radius and rectangle width.
- Shift + Alt + Mouse wheel: Rectangle depth.
- Shift + Control + Mouse wheel: Height.

The current values are shared between different commands. For example if you changed radius to 20 while leveling then the radius would still be 20 when switching to the raise command.

It's possible to restrict the starting value with parameters `cmd_r`, `cmd_w`, `cmd_d` and `cmd_h`. This is generally not recommended (unless you really need it).

For example `hoe_object tame cmd_r=10-20` would restrict the starting radius between 10 and 20 meters. If the radius was 15 it would stay at 15. But a radius of 5 would become 10 and a radius of 50 would become 20.

### Edge targeted commands

By default, the visualization expects the targeted coordinates to be at the center of the command. Edge targeting can be enabled by switching parameters `x,z,y` to `tx,tz,ty`.

This changes the visualization and causes `r` and `d` parameters to be automatically calculated from the distance between the player and the targeted position.

Otherwise the logic and usage stays the same as before.

To reduce typing, use aliases `hammer_terrain_to` and `hoe_terrain_to` which also automatically switch to the rectangle mode.

For slopes, you can use `hammer_slope [width]` and `hoe_slope [width]`. For example `hoe_slope 5 paint=paved;hoe_terrain_to lower=1 smooth=.25`.

# Configuration

Following powers are available with `hammer_config` command:

- Enabled (default: `true`, key: `enabled`): If disabled, removes most features.
- All objects (default: `true`, key: `all_objects`): Hammer can select and place any object. Any placed object can be removed with the hammer until the area is reloaded.
- Allow in dungeons (default: `true`, key: `allow_in_dungeons`): Building is allowed in dungeons.
- Blueprint folder (default: `PlanBuild`, key: `blueprint_folder`): Folder relative to the config folder. Both profile and base Valheim folders are searched for .blueprint and .vbuild files.
- Build Share folder (default: `BuildShare/Builds`, key: `build_share_folder`): Folder relative to the Valheim.exe.
- Copy state (default: `true`, key: `copy_state`): Object state is copied (for example chest contents or item stand items).
- Dimensions (key: `dimensions`): Measurements for objects.
- Disable loot (default: `false`, key: `disable_loot`): Creatures and structures won't drop loot when destroyed with the hammer.
- Ignore no build (default: `true`, key: `ignore_no_build`): "Mystical power" no longer prevents building.
- Ignore other restrictions (default: `true`, key: `ignore_other_restrictions`): Removes any other restrictions (for example campfires can be built on wood floors).
- Ignore wards (default: `true`, key: `ignore_wards`): Wards no longer prevent building.
- Ignored ids (key: `ignored_ids`): Objects ignored by this mod (ids separated by ,).
- Ignored remove ids (key: `ignored_remove_ids`): Additional ids that are ignored when removing anything.
- Infinite health (default: `false`, key: `infinite_health`): Sets the Overwrite health setting to 1E30.
- No cost (default: `false`, key: `no_cost`): Removes durability, resource and stamina costs.
- No creator (default: `false`, key: `no_creator`): Builds without setting the creator information.
- Overwrite health (default: `0`, key: `overwrite_health`): Sets the health of built or repaired objects (0 reverts to the default max health, except for creatures).
- Range (default: `0`, key: `range`): Range for the hammer (capped at about 50 meters).
- Remove anything (default: `false`, key: `remove_anything`): Allows removing any object.
- Remove area (default: `0`, key: `remove_area`): Removes same objects within the radius.
- Repair anything (default: `false`, key: `repair_anything`): Allows healing or repairing any object.
- Reset offset on unfreeze (default `true`, key: `reset_offset_on_unfreeze`): Removes the placement offset when unfreezing the placement.
- Save blueprints to profile (default: `false`, key: `save_blueprints_to_profile`): If enabled, blueprints are saved to the profile folder instead of the base Valheim folder.
- Save blueprint data (default: `true`, key: `save_blueprint_data`): If enabled, object data values are saved to blueprints.
- Show command values (default: `false`, key: `show_command_values`): Always show the command on tool descriptions.
- Snap points for all objects (default: `false`, key: `snap_points_for_all_objects`):If enabled, multi selection creates snap points for every object.
- Unfreeze on select (default `true`, key: `unfreeze_on_select`): Removes the placement freeze when selecting a new object.
- Unfreeze on unequip (defualt `true`, key: `unfreeze_on_unequip`): Removes the placement freeze when unequipping the hammer.

On servers, above features are disabled without cheat access (except visual changes and offsetting).

## Visual

Visual settings work even without cheat access. These all are disabled by default.

- No effects: Hides visual effects of building, repairing and destroying.
- No health indicator: Hides the piece health bar.
- No placement marker: Hides the yellow placement marker (also affects Gizmo mod).
- No support indicator: Hides the color that shows support.

## Output

Messages from the mod can be configured with following settings:

- `chat_output`: Sends messages to the chat window (when using binds or commands in the chat window).
- `disable_messages`: Disables all messages from the mod (console output not affected).
- `disable_offset_messages`: Disables messages from changing the placement offset.
- `disable_scale_messages`: Disables messages from changing the object scale.
- `disable_select_messages`: Disables messages from selecting objects.

## Key bindings

Some commonly used features have pre-made key binds in the configuration. Internally these use the `bind` command.

This means that [Server Devcommands](https://valheim.thunderstore.io/package/JereKuusela/Server_devcommands/) mod is needed for multi-key binds or for using the mouse wheel.

If you don't wish to use this system you can set all binds to none and make your own bindings with the `bind` command.

# Building

Hammer configuration applies to all building, including the standard structures selected from the build menu.

When selecting an existing object, its size and rotation is copied to the placement tool. The last rotation is always used when using the build window.

Object scaling only works for some objects (mostly trees and rocks). This is restricted by the base game (scaling is not stored in the save file). [Structure Tweaks](https://valheim.thunderstore.io/package/JereKuusela/Structure_Tweaks/) mod can be used to enable scaling for all objects (required for all clients).

If "Overwrite health" is enabled, objects have a specified health (including creatures). For minerocks, the health is applied to the individual parts (the outer shell stays at 1 health). Repairing can be used to modify the shell health if needed.

Setting a very high health (like "1E30") can be used to make object indestructible because the damage taken is rounded down to zero. This also prevents structures collapsing from lack of support.

For creatures, the max health resets when the area is reloaded if the current health equals the max health. For this reason, the current health is set slightly higher than the max health.

"Copy state" only applies when selecting existing objects since structures from the build menu are stateless. However the creator ID is always set based on the "No creator" setting, even for non-standard structures.

"No creator" is currently quite pointless since most structures ignore the value and will get targeted by the enemies regardless of the value. But maybe someone can find some use for it.

Locations (Points of Interest) can also be copied. However only static parts are included in the actual location. For example in the start temple, each boss stone is a separate object and can be copied separately if needed.

# Repairing

By default, only change is that the UI shows how much damage was repaired.

If "Repair anything" is enabled, most destructible objects can be repaired or healed. This includes creatures and players.

For minerocks, if the targeted part is already at full health, a random part is restored instead. This is not very practical but can be used to restore any accidental changes to minerocks.

If "Overwrite health" is enabled, the object is repaired or damaged to the specified health value.

For creatures, the maximum health value is also set. So they will keep their max health even when disabling "Overwrite health". Other objects will revert to the original max health when repaired.

Unfortunately, the max health resets when the area is reloaded if the current health equals the max health. For this reason, the current health is set slightly higher than the max health.

# Destroying

By default, destroying only works for standard structures and placed objects. Placed objects can only be removed temporarily since the required information is lost when the area is reloaded.

If "Destroy anything" is enabled, all objects can be removed.

If "Disable loot" is enabled, destroying creatures or structures won't drop loot. This can be useful to get rid of very high starred creatures that crash the game when killed.

# Infinity Hammer

Bend the rules of building! Copy any object, make structures indestructible, remove all restrictions, destroy anything and more...

Client-side mod that is compatible with unmodded clients.

Check any modding [guide](https://youtu.be/WfvA5a5tNHo) for installation instructions.

Install [Server Devcommands](https://valheim.thunderstore.io/package/JereKuusela/Server_devcommands/) to enable key binds and use this on a server (as an admin).

# Features

- Build anything. Trees, rocks, creatures... All can be placed with the hammer, with a precise placement!
- Copy anything. Armor stands, chests and item stands with their contents. Even boss altars!
- Make structures indestructible, even the gravity can't bring them down.
- Remove anything. Something unremovable messing up your grand design? No more!
- Build without restrictions. Dungeons and even the start temple become valid for building.
- QoL improvements: Extended range, no resouce costs, no item drops, no visual effects and more!
- Tame anything. Powerful creatures, or even bosses can become your protectors. Or just pit them against each other!

# Selecting objects

The main feature of this mod is the ability to build any object with the hammer.

This is done by using the `hammer` console command which works in four ways:

- `hammer`: Selects (and copies) the hovered object.
- `hammer [object id]`: Selects an object by id ([Item IDs](https://valheim.fandom.com/wiki/Item_IDs)).
- `hammer connect`: Selects the hovered building.
- `hammer [number]`: Selects all objects within a radius.

Selecting the hovered object can be quickly done by pressing NumPad5. Holding alt key will select the entire building.

The hammer build menu also contains Pipette and Area select tools.

The `hammer` command has following extra parameters which allow modifying the selected objects:
- `scale=number` or `scale=x,z,y`: Overrides the size (if the object can be scaled).
- `health=number`: Overrides the health.
- `text=string`: Overrides the sign text.
- `level=number`: Overrides the creature level (stars + 1).
- `stars=number`: Overrides the creature stars (level - 1).
- `from=x,z,y`: Overrides the player position when doing area selection.

For example `hammer Beech1 scale=2 health=1000` would select a beech tree with a double size and 1000 health.

## Scaling

Shift + mouse wheel will scale up/down the selection.

This can be also done with following commands:

- `hammer_scale build`: Resets the scale to 100%.
- `hammer_scale build [percentage or amount]`: Scales the size.
- `hammer_scale_x build [percentage or amount]`: Scales only the x-axis.
- `hammer_scale_y build [percentage or amount]`: Scales only the y-axis.
- `hammer_scale_z build [percentage or amount]`: Scales only the z-axis.

For example `hammer_scale build 5%` would scale from 100 % to 105 % or 200 % to 210 %. While `hammer_scale build -5%` would scale from 100 % to 95.2 % or 200 % to 190.5 %. 

For example `hammer_scale build 0.1` would scale from 100 % to 110 % or 200 % to 210 %. While `hammer_scale build -0.1%` would scale from 100 % to 90 % or 200 % to 190 %. 

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

Building and removing objects can be quickly reversed by pressing NumPad7. Reversed actions can be redone with NumPad9.

Note: Any other removal like buildings collapsing due to lack of support or destroyed by enemies is not tracked. Only your direct actions can be undone.

Internally this is done by using commands `hammer_undo` and `hammer_redo`.

If [Server Devcommands](https://valheim.thunderstore.io/package/JereKuusela/Server_devcommands/) mod is installed, this integrates with its undo system (unless overridden in the config). So commmands `undo` and `redo` would also work.

## Multiplacement

Objects can be placed multiple times in a row with the command:

- `hammer_stack_[direction] [amount or min-max] [step=auto]`

Possible directions are `backward`, `down`, `forward`,`left`, `right` and `up`.

The amount simply determines how many objects are placed. This can be fine-tuned by giving a range.

The step sets the distance between placed objects. By default this is the size of the object. 

Note: Choosing the correction direction can be difficult when rotating the selection. But you can easily try things by reverting the changes with `hammer_undo`.

Note: Determining the size of the object is not fully reliable. Be prepared to give the distance manually.

Examples:
-  `hammer_stack_up 10`: Places 10 objects on top of each other.
-  `hammer_stack_up -5-4`: Places 10 objects with 5 below and 4 on top of the main object.
-  `hammer_stack_left 5 10`: Places 5 objects with 10 meters between each of them.
-  `hammer_stack_left 5 2*auto`: Places 5 objects while leaving a gap between them (equal to the object size).
-  `hammer_stack_left 3-4`: Places 2 objects some distance away from the selected position.

To place objects in a rectangle or a box, use the following command:

- `hammer_stack [forward,up,right or z1-z2,y1-y2,x1-x2] [step=auto,auto,auto]`

## Blueprints

Basic support is provided for BuildShare .vbuild and [PlanBuild](https://valheim.thunderstore.io/package/MathiasDecrock/PlanBuild/) .blueprint files.

The command `hammer_blueprint [file name]` allows placing them. If no files are found, configure the source folder.

New PlanBuild blueprints can also be created with `hammer_save [file name]` command.

Note: Infinity Hammer will also store the object data when creating blueprints. This can significantly increase the file size and cause incompatibility with future PlanBuild versions.

## Utility

Some special commands exist for advaced cases. Usually you want to bind these.

- `hammer_grid [precision] [center=current]`: Restricts possible placement coordinates. Using the same command removes the restriction.
- `hammer_place`: Allows placing with a key press.
- `hammer_repair`: Selects the repair tool for quickly clearing the selection.
- `hammer_mirror`: Mirrors the selection.
- `hammer_add_piece_components`: Adds the Piece component to every object which allows copying them with PlanBuild mod.

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

## Command placement

Some commands have coordinates as their parameters. These can be set with hammer/hoe by using commands `hammer_command` or `hoe_command`.

For example using `hammer_command hammer 10 from=x,z,y` would select every object within 10 meters of the hovered position.

The radius can be modified by using shift + mousewheel, when using `hammer_command hammer r from=x,z,y` (10 changed to r). This also makes the mod highlight the affected area! For less typing you can use the lias `hammer_area`.

This command can be added to the menu with `hammer_add hammer_area`. It's recommended to always test commands first before adding them.

## World Edit Commands

The full potential of commands can be unlocked by installing [World Edit Commands](https://valheim.thunderstore.io/package/JereKuusela/World_Edit_Commands/) mod. This automatically adds new commands to the hoe (mainly for terrain modifications).

Full list of parameters:
- `x`: X coordinate.
- `y`: Y coordinate.
- `z`: Z coordinate.
- `a`: Angle. Mostly matters for rectangles.
- `r`: Radius. Enables circle shape.
- `w`: Rectangle width. Enables square shape.
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

# Configuration

Following powers are available with `hammer_config` command:

- Enabled (default: `true`, key: `enabled`): If disabled, removes most features.
- All objects (default: `true`, key: `all_objects`): Hammer can select and place any object. Any placed object can be removed with the hammer until the area is reloaded.
- Allow in dungeons (default: `true`, key: `allow_in_dungeons`): Building is allowed in dungeons.
- Build range (default: `0`, key: `build_range`): Range for building (capped at about 50 meters).
- Build Share folder (default: `BuildShare/Builds`, key: `build_share_folder`): Folder relative to the Valheim.exe.
- Copy rotation (default: `true`, key: `copy_rotation`): Copies rotation of the selected object.
- Copy state (default: `true`, key: `copy_state`): Object state is copied (for example chest contents or item stand items).
- Custom binds (default: ` `, key: `custom_binds`): Sets binds at the game start up. Any existing binds are cleared from those keys.
- Disable loot (default: `false`, key: `disable_loot`): Creatures and structures won't drop loot when destroyed with the hammer.
- Disable marker (default: `false`, key: `disable_marker`): Whether the placement ghost is visualized.
- Enable undo (default: `true`, key: `enable_undo`): Whether the undo/redo feature is enabled.
- Ignore no build (default: `true`, key: `ignore_no_build`): "Mystical power" no longer prevents building.
- Ignore other restrictions (default: `true`, key: `ignore_other_restrictions`): Removes any other restrictions (for example campfires can be built on wood floors).
- Ignore wards (default: `true`, key: `ignore_wards`): Wards no longer prevent building.
- Infinite health (default: `false`, key: `infinite_health`): Sets the Overwrite health setting to 10E30.
- No build cost (default: `true`, key: `no_build_cost`): Removes resource cost and crafting station requirement.
- No creator (default: `false`, key: `no_creator`): Builds without setting the creator information.
- No durability loss (default: `true`, key: `no_durability_loss`): Hammer auto-repairs used durability.
- No stamina cost (default: `true`, key: `no_stamina_cost`): Hammer auto-regens used stamina.
- Overwrite health (default: `0`, key: `overwrite_health`): Sets the health of built or repaired objects (0 reverts to the default max health, except for creatures).
- Plan Build folder (default: `BepInEx/config/PlanBuild`, key: `plan_build_folder`): Folder relative to the Valheim.exe.
- Remove anything (default: `false`, key: `remove_anything`): Allows removing any object.
- Remove area (default: `0`, key: `remove_area`): Removes same objects within the radius.
- Remove blacklist (key: `remove_blacklist`): Allows disabling remove for given objects (ids separated by ,). Only works if remove anything is enabled.
- Remove effects (default: `false`, key: `remove_effects`): Removes visual effects of building, repairing and destroying.
- Remove range (default: `0`, key: `remove_range`): Range for removing (capped at about 50 meters).
- Repair anything (default: `false`, key: `repair_anything`): Allows healing or repairing any object.
- Repair range (default: `0`, key: `repair_range`): Range for repairing (capped at about 50 meters).
- Repair taming (default: `false`, key: `repair_taming`): Repairing full health creatures will tame/untame them (works for all creatures).
- Reset offset on unfreeze (default `true`, key: `reset_offset_on_unfreeze`): Removes the placement offset when unfreezing the placement.
- Select blacklist (key: `select_blacklist`): Allows disabling select for given objects (ids separated by ,).
- Select range (default: `0`, key: `select_range`): Range for selecting (capped at about 50 meters).
- Server Devcommands undo (default: `true`, key: `server_devcommands_undo`): If disabled, uses Infinity Hammer's own undo system even if Server Devcommands is installed.
- Unfreeze on select (default `false`, key: `unfreeze_on_select`): Removes the placement freeze when selecting a new object.
- Unfreeze on unequip (defualt `true`, key: `unfreeze_on_unequip`): Removes the placement freeze when unequipping the hammer.

On servers, above features are disabled without cheat access (except Copy rotate, No placement marker, Remove effects, Select range and offsetting).

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

Binds can also be shared with others with the `custom_binds` setting but this isn't really recommended.

The format is `keycode1 command1;keycode2 command2`. For example `keypad0 hammer;keypad7 hammer_undo;keypad9 hammer_redo`.


# Building

Hammer configuration applies to all building, including the standard structures selected from the build menu.

When selecting an existing object, its size and rotation is copied to the placement tool. If "Copy rotation" is disabled then the selection tool keeps the last rotation. The last rotation is always used when using the build window.

Object scaling only works for some objects (mostly trees and rocks). This is restricted by the base game (scaling is not stored in the save file).

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

Blacklist can be used to avoid destroying critical objects like locations. For example `hammer_config remove_blacklist LocationProxy`.

# Changelog

- v1.18
  - Adds new settings `move_amount` and `move_amount_large` to configure default bindings.
	- Adds new command aliases `hammer_area`, `hammer_object` and `hammer_terrain`.
	- Changes most default bindings only work when the hammer or hoe is equipped.
	- Changes the command description to always show the actual command.

- v1.17
	- Fixes `hammer_config` command not working.

- v1.16
	- Adds a new setting section for pre-defined key binds.
	- Adds new settings to add custom commands to the build menus.
	- Adds a new setting `server_devcommands_undo` to allow using Infinity Hammer's own undo system even with Server Devcommands installed (default `true`).
	- Adds a new parameter to the `hammer` command which allows selecting all nearby objects.
	- Adds a new parameter `connect` to the `hammer` command which allows selecting the hovered object and connected pieces.
	- Adds a new parameter `health` to the `hammer` command which allows overriding the object health.
	- Adds new parameters `level` and `stars` to the `hammer` command which allows overriding the creature level.
	- Adds a new parameter `text` to the `hammer` command which allows setting the sign text.
	- Adds new commands `hammer_command` and `hoe_command` that allow executing console commands.
	- Adds new commands `hammer_add` and `hoe_add` that allow adding new commands to the build menu.
	- Adds new commands `hammer_remove` and `hoe_remove` that allow removing commands to the build menu.
	- Adds new commands `hammer_list` and `hoe_list` that allow listing commands on the build menu.
	- Adds a new command `hammer_mirror` to mirror the selection.
	- Adds new commands `hammer_scale_x`, `hammer_scale_y` and `hammer_scale_z` to scale up/down a single axis.
	- Changes the `hammer_scale` command to scale up/down instead of setting the scale directly.
	- Improves PlanBuild compatibility.
	- Renames the `hammer_scale` command to `hammer_set_scale`.
	- Removes the setting `scaling_step` as obsolete.
	- Removes the commands `hammer_scale_up` and `hammer_scale_down` as obsolete (existing binds automatically migrate to use `hammer_scale`).
	- Removes the setting `auto_equip` as obsolete (now always on so that the mod works properly).
	- Fixes the setting `copy_state` not working.
	- Fixes the `scale` parameter not working on the `hammer` command.

- v1.15
	- Adds a new command `hammer_save` to create blueprints with data.
	- Adds support for PlanBuild scaling.
	- Adds a new setting `chat_output` to control is the output show on the chat window (default `false`).
	- Removes the setting `max_undo_steps` as obsolete (usually Server Devcommands is installed).

- v1.14
	- Adds support for PlanBuild snappoints.
	- Adds support for placing blueprints even when some objects are missing.
	- Adds a new command `hammer_grid` to restrict the possible placement coordinates.
	- Improves error handling.
	- Fixes the snapping disconnecting with the precise placement (non-freeze).

- v1.13
	- Improves compatibility with automatic repair mods.

- v1.12
	- Adds a new command `hammer_blueprint` to place new instances of Build Share and Plan Build blueprints.
	- Adds a new command `hammer_location` to place new instances of locations.
	- Adds a new command `hammer_freeze` to prevent mouse position affecting the placement (allows moving around).
	- Adds a new setting `build_share_folder` to configure the blueprint folder.
	- Adds a new setting `plan_build_folder` to configure the blueprint folder.
	- Adds a new setting `unfreeze_on_unequip` to automatically unfreeze the placement when unequipping the hammer (default `true`).
	- Adds a new setting `unfreeze_on_select` to automatically unfreeze the placement when selecting a new piece (default `false`).
	- Adds a new setting `reset_offset_on_unfreeze` to automatically reset the placement offset when a freeze is removed (default `true`).
	- Adds a new setting `infinite_health` to set a very high Overwrite health (default `false`).
	- Adds more supported truthy/falsy values for the `hammer_config` command.
	- Improves how the placement rule are checked with the placement offset.
	- Removes the `hammer_setup_binds` as obsolete (probably just caused conflicts for most people).
	- Fixes item drop data not being copied.

- v1.11
	- Adds compatibility with Gizmo Reloaded.
	- Adds a new setting `binds` to automatically set binds at the game start up.
	- Adds support for random rotation to `hammer_rotate_*` commands.

- v1.10
	- Renames and splits commands `hammer_move_*` to be more clear about the direction.
	- Renames and splits commands `hammer_stack_*` to be more clear about the direction.
	- Improves autocomplete and output for most commands.
	- Fixes the undo feature breaking hoe usage.

- v1.9
	- Fixes `hammer_move_*` commands not working properly.
	- Fixes `hammer_stack_*` not working with rotated objects.

- v1.8
	- Adds compatibility with Comfy Gizmo.
	- Adds supports for `number*auto` value to the commands `hammer_move_x`, `hammer_move_y` and `hammer_move_z` (automatically sets the step size). 
	- Adds a new direction parameter to the commands `hammer_move_x`, `hammer_move_y` and `hammer_move_z` for Server Devcommands mouse wheel binding.
	- Adds a new command `hammer_repair` to select the repair tool.
	- Adds a new command `hammer_place` to place pieces with commands.
	- Adds new commands `hammer_rotate_x`, `hammer_rotate_y` and `hammer_rotate_z` to change rotation with commands.
	- Adds new commands `hammer_stack_x`, `hammer_stack_y`, `hammer_stack_z` and `hammer_stack` to place multiple objects next to each other.
	- Adds new settings `disable_messages`, `disable_offset_messages`, `disable_scale_messages` and `disable_select_messages` to configure the output.
	- Adds a new setting `remove_area` for removing the same objects within a radius.
	- Improves autocomplete with Server Devcommands.
	- Improves the `hammer_config` command to allow directly setting flags with values 1 and 0.
	- Improves the `hammer_config` command to work better when giving multiple values to some commands.
	- Improves the `hammer_config` command to print the current value for non-flags if no parameter is given.
	- Fixes repair range not working for creatures and other non-piece objects.
	- Fixes repair taming.

- v1.7
	- Adds new setting `remove_blacklist` that allows disabling remove for some objects.
	- Adds new setting `select_blacklist` that allows disabling select for some objects.
	- Changes [Server Devcommands](https://valheim.thunderstore.io/package/JereKuusela/Server_devcommands/) compatibility to work with the newest version (old versions won't work anymore as the name was changed).
	- Fixes `ignore_other_restrictions` being able to ignore no build zones, etc.
	- Fixes `ignore_other_restrictions` allowing placement to arbitrary position when the placement ghost is not active.
	- Fixes undo/redo not working for locations and creature stars.
	- Fixes taming and untaming not working with the repair.
	- Fixes `hammer_add_piece_components` affecting players.
	- Fixes incompatibility with some remove mods.

- v1.6:
	- Adds a new setting `auto_equip` to automatically equip the hammer when selecting an object (enabled by default).
	- Adds a version number check to the Server Devcommands mod compatibility.
	- Fixes `disable_marker` setting also disabling the Gizmo visual or the Plan Build visual.
	- Fixes scale not being set when selecting objects from the build menu (better compatibility with some mods).
	- Fixes Piece components being added to the object library when selecting objects (no known issues).

- v1.5
	- Adds support for the undo system of Server Devcommands mod (if installed).
	- Adds a new command `hammer_add_piece_components` to allow copying anything with PlanBuild.
	- Removes the `hammer_setup_binds_DEV` command as obsolete (Server Devcommands mod is used automatically, if installed).
	- Fixes some error messages appearing when placing spawners and other objects.
	- Fixes error messages when using the hoe.

- v1.4
	- Adds new commands to offset the placement to precisely set the position.
	- Adds new commands to set recommended key bindings.
	- Adds new setting to disable the placement marker.
	- Adds new parameter to hammer command to set the initial scale.
	- Adds messages for undo and redo actions.
	- Changes overwrite health to set the current health slightly higher than the maximum (makes it less likely to reset).
	- Fixes scale being applied to objects that don't support it.
	- Fixes tamed status not being copied for creatures.
	- Fixes hammer_scale not working with different scales per axis.
	- Fixes "Select range" setting not working.
	- Fixes "Remove range" setting not working.

- v1.3
	- Fixes health not being copied for creatures (got overwritten by stars).

- v1.2
	- Adds object names to the build overlay.
	- Adds setting to disable build, repair and destroy effects.
	- Adds setting to tame/untame creatures with repair.
	- Adds setting to disable creature and structure loot when destroyed with the hammer.
	- Fixes creature stars not getting copied.
	- Fixes error when copying creatures.
	- Fixed "creator" data being added to non-piece objects.
	- Fixes structures having a higher destroy priority even with "Destroy anything" enabled.
	- Fixes "Overwrite health" not working when selecting a piece from the build menu.

- v1.1
	- Adds new setting to overwrite the health of built and repaired objects (including creatures).
	- Adds no stamina and durability cost to also affect repairing.
	- Adds new setting to change repair range.
	- Adds new setting to repair anything (including creatures).
	- Adds support for non-uniform scaling with hammer_scale command.
	- Changes Auto rotate setting to Copy rotation.
	- Changes messages to have a high priority (fixes scaling messages lagging behind).
	- Fixes size being shown for objects that don't support changing it.
	- Fixes creator not being set for copied objects (unless "No Creator" is on).
	- Fixes selection keeping the hover color when selecting a structure.
	- Fixes selection being removed when the selected objects is destroyed.

- v1.0
	- Initial release.

Thanks for Azumatt for creating the mod icon!

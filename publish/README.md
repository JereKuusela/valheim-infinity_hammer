# Infinity Hammer

Bend the rules of building! Copy any object, make structures indestructible, remove all restrictions, destroy anything and more...

Client-side mod that is compatible with unmodded clients.

# Features

- Build anything. Trees, rocks, creatures... All can be placed with the hammer, with a precise placement!
- Copy anything. Armor stands, chests and item stands with their contents. Even boss altars!
- Make structures indestructible, even the gravity can't bring them down.
- Remove anything. Something unremovable messing up your grand design? No more!
- Build without restrictions. Dungeons and even the start temple become valid for building.
- QoL improvements: Extended range, no resouce costs, no item drops, no visual effects and more!
- Tame anything. Powerful creatures, or even bosses can become your protectors. Or just pit them against each other!

# Manual Installation:

1. Install the [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim).
2. Download the latest zip.
3. Extract it in the \<GameDirectory\>\BepInEx\plugins\ folder.
4. Recommended to also install [Comfy Gizmo](https://github.com/redseiko/ValheimMods/releases/latest) or [Gizmo Reloaded](https://valheim.thunderstore.io/package/Tyrenheim/M3TO_Gizmo_Reloaded/) for better rotating.
5. Optionally also install the [Configuration manager](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/tag/v16.4) to configure the hammer more easily.
6. Recommended to also install [Server Devcommands](https://valheim.thunderstore.io/package/JereKuusela/Server_devcommands/) for improved autocomplete and to use it as an admin on servers.

# Commands

- `hammer`: Selects the hovered object to be placed.
- `hammer [item id]`: Selects an object by id ([Item IDs](https://valheim.fandom.com/wiki/Item_IDs)) to be placed.
- `hammer connect=piece`: Selects the hovered object and all connected pieces.
- `hammer radius=number`: Selects all nearby objects.
- `hammer ... scale=number`: Overrides the initial scale (if the object can be scaled). Number or x,z,y.
- `hammer ... health=number`: Overrides the object health.
- `hammer ... text=string`: Overrides the sign text.
- `hammer ... level=number`: Overrides the creature level (stars + 1).
- `hammer ... stars=number`: Overrides the creature stars (level - 1).
- `hammer ... from=x,z,y`: Overrides the player position when doing area selection.
- `hammer ... connect`: Selects all connected pieces.
- `hammer_add_piece_components`: Adds the Piece component to every object which allows copying them with PlanBuild mod.
- `hammer_blueprint [file name]`: Selects a Build Share or a Plan Build blueprint located on your computer.
- `hammer_command [command]`: Executes the given command. Replaces command values with coordinates, angle and scale.
- `hammer_config [key] [value]`: Toggles or sets configuration values. For lists, the given value is toggled on or off (`remove_blacklist` or `select_blacklist`).
- `hammer_freeze`: Toggles whether the mouse affects placement position. Allows moving around while the object's position is frozen.
- `hammer_grid [precision] [center=current]`: Restricts possible placement coordinates. Using the same command removes the restriction.
- `hammer_location [location_id] [seed=0] [random damage]`: Selects a location by id. Allows setting the random result with seed ("all" value enables all child objects).
- `hammer_mirror`: Mirrors the selection.
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
- `hammer_place`: Allows placing with a key press (for command bindings).
- `hammer_redo`: Restores reverted actions. Uses Server Devcommands undo system if installed.
- `hammer_repair`: Selects the repair tool. Useful for clearing the selection.
- `hammer_rotate_x [degrees]`: Rotates around the X axis.
- `hammer_rotate_x [number*random]`: Randomly rotates around the X axis with a given step size. For example `3*random` would randomly rotate 0, 120 or 240 degrees.
- `hammer_rotate_x [random]`: Randomly rotates around the X axis depending on the object shape (90 or 180 degrees precision).
- `hammer_rotate_y [degrees/number*random/random]`: Rotates around the Y axis.
- `hammer_rotate_y [number*random]`: Randomly rotates around the Y axis with a given step size. For example `3*random` would randomly rotate 0, 120 or 240 degrees.
- `hammer_rotate_y [random]`: Randomly rotates around the Y axis depending on the object shape (90 or 180 degrees precision).
- `hammer_rotate_z [degrees/number*random/random]`: Rotates around the Z axis.
- `hammer_rotate_z [number*random]`: Randomly rotates around the Z axis with a given step size. For example `3*random` would randomly rotate 0, 120 or 240 degrees.
- `hammer_rotate_z [random]`: Randomly rotates around the Z axis depending on the object shape (90 or 180 degrees precision).
- `hammer_save [file name]`: Saves the selection to a blueprint.
- `hammer_scale`: Resets the scale to 100%.
- `hammer_scale [percentage]`: Scales the selection (if the object supports it).
- `hammer_scale_x [percentage]`: Scales the x-axis (if the object supports it).
- `hammer_scale_y [percentage]`: Scales the y-axis (if the object supports it).
- `hammer_scale_z [percentage]`: Scales the z-axis (if the object supports it).
- `hammer_stack [forward,up,right or z1-z2,y1-y2,x1-x2] [step=auto,auto,auto]`: Places multiple objects next to each other.
- `hammer_stack_backward [amount or min-max] [step=auto]`: Places multiple objects towards the backward direction.
- `hammer_stack_down [amount or min-max] [step=auto]`: Places multiple objects towards the down direction.
- `hammer_stack_forward [amount or min-max] [step=auto]`: Places multiple objects towards the forward direction.
- `hammer_stack_left [amount or min-max] [step=auto]`: Places multiple objects towards the left direction.
- `hammer_stack_right [amount or min-max] [step=auto]`: Places multiple objects towards the right direction.
- `hammer_stack_up [amount or min-max] [step=auto]`: Places multiple objects towards the up direction.
- `hammer_undo`: Reverts placing or removing. Uses Server Devcommands undo system if installed.

Note: Some interactions are quite complicated so please report any issues!

Note: Some commands have a hidden direction parameter at the end. This are intended for mouse wheel binding (so that scroll up and down work differently).


# Key bindings

Bind frequently used commands to ([key codes](https://docs.unity3d.com/ScriptReference/KeyCode.html)).

When sharing the config file, put bindings to the `binds` setting with format `keycode1 command1;keycode2 command2`. For example `keypad0 hammer;keypad7 hammer_undo;keypad9 hammer_redo`.

It's recommended to install Server Devcommands mod which allows using modifier keys on bindings.

Remember that you can copy-paste commands to the console.

## General usage

Quickly selects the hovered object and undo/redo:
- `bind keypad5 hammer`
- `bind keypad7 hammer_undo`
- `bind keypad9 hammer_redo`

Object scaling and reset: 
- `bind keypad1 hammer_scale_down`
- `bind keypad2 hammer_scale`
- `bind keypad3 hammer_scale_up`

Toggles all features on/off (if even needed):
- `bind keypad8 hammer_config enabled`

## Precise placement / placement offset

Bind freezing or offset reset near arrow keys:
- `bind keypad0 hammer_freeze` or `bind keypad0 hammer_offset`

Then bind movement to arrow keys:
- `bind rightarrow hammer_move_right 0.1`
- `bind leftarrow hammer_move_left 0.1`
- `bind downarrow hammer_move_down 0.1`
- `bind uparrow hammer_move_up 0.1`

With Server Devcommands you can use modifier keys for the forward/backward direction:
- `bind rightarrow hammer_move_right 0.1`
- `bind leftarrow hammer_move_left 0.1`
- `bind downarrow hammer_move_down 0.1`
- `bind uparrow hammer_move_up 0.1`
- `bind downarrow,leftcontrol hammer_move_backward 0.1`
- `bind uparrow,leftcontrol hammer_move_forward 0.1`

You can also use another modifier key for a bigger offset:
- `bind rightarrow hammer_move_right 0.1`
- `bind leftarrow hammer_move_left 0.1`
- `bind downarrow hammer_move_down 0.1`
- `bind uparrow hammer_move_up 0.1`
- `bind downarrow,leftcontrol hammer_move_backward 0.1`
- `bind uparrow,leftcontrol hammer_move_forward 0.1`
- `bind rightarrow,leftalt hammer_move_right 1`
- `bind leftarrow,leftalt hammer_move_left 1`
- `bind downarrow,leftalt hammer_move_down 1`
- `bind uparrow,leftalt hammer_move_up 1`
- `bind downarrow,leftalt,leftcontrol hammer_move_backward 1`
- `bind uparrow,leftalt,leftcontrol hammer_move_forward 1`

# Configuration

Following powers are available with `hammer_config` command:

- Enabled (default: `true`, key: `enabled`): If false, disabled most features.
- All objects (default: `true`, key: `all_objects`): Hammer can select and place any object. Any placed object can be removed with the hammer until the area is reloaded.
- Allow in dungeons (default: `true`, key: `allow_in_dungeons`): Building is allowed in dungeons.
- Auto equip (default: `true`, key: `auto_equip`): Automatically equips the hammer when selecting an object.
- Binds (default: ` `, key: `binds`): Sets binds at the game start up. Any existing binds are cleared from those keys.
- Build range (default: `0`, key: `build_range`): Range for building (capped at about 50 meters).
- Build Share folder (default: `BuildShare/Builds`, key: `build_share_folder`): Folder relative to the Valheim.exe.
- Copy rotation (default: `true`, key: `copy_rotation`): Copies rotation of the selected object.
- Copy state (default: `true`, key: `copy_state`): Object state is copied (for example chest contents or item stand items).
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
- Unfreeze on select (default `false`, key: `unfreeze_on_select`): Removes the placement freeze when selecting a new object.
- Unfreeze on unequip (defualt `true`, key: `unfreeze_on_unequip`): Removes the placement freeze when unequipping the hammer.

On servers, above features are disabled without cheat access (except Copy rotate, No placement marker, Remove effects, Select range and offsetting).

Messages from the mod can be configured with following settings:

- `chat_output`: Sends messages to the chat window (when using binds or commands in the chat window).
- `disable_messages`: Disables all messages from the mod (console output not affected).
- `disable_offset_messages`: Disables messages from changing the placement offset.
- `disable_scale_messages`: Disables messages from changing the object scale.
- `disable_select_messages`: Disables messages from selecting objects.

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

- v1.16
	- Adds a new setting to add custom commands to the build menu.
	- Adds a new parameter `radius` to the `hammer` command which allows selecting all nearby objects.
	- Adds a new parameter `connect=piece` to the `hammer` command which allows selecting the hovered object and connected pieces.
	- Adds a new parameter `health` to the `hammer` command which allows overriding the object health.
	- Adds new parameters `level` and `stars` to the `hammer` command which allows overriding the creature level.
	- Adds a new parameter `text` to the `hammer` command which allows setting the sign text.
	- Adds a new command `hammer_command` that allows executing console commands.
	- Adds a new command `hammer_mirror` to mirror the selection.
	- Adds new commands `hammer_scale_x`, `hammer_scale_y` and `hammer_scale_z` to scale up/down a single axis.
	- Changes the `hammer_scale` command to scale up/down instead of setting the scale directly.
	- Fixes the setting `copy_state` not working.
	- Fixes the `scale` parameter not working on the `hammer` command.
	- Removes the setting `scaling_step` as obsolete.
	- Removes the commands `hammer_scale_up` and `hammer_scale_down` as obsolete.

- v1.15
	- Adds a new command `hammer_save` to create blueprints with data.
	- Adds support for PlanBuild scaling.
	- Adds a new setting `chat_output` to control is the output show on the chat window (default `false`).
	- Removes the setting `max_undo_steps` as obsolete (usually Server Devcommands installed).

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
	- Changes Server devcommands compatibility to work with the newest version (old versions won't work anymore as the name was changed).
	- Fixes `ignore_other_restrictions` being able to ignore no build zones, etc.
	- Fixes `ignore_other_restrictions` allowing placement to arbitrary position when the placement ghost is not active.
	- Fixes undo/redo not working for locations and creature stars.
	- Fixes taming and untaming not working with the repair.
	- Fixes `hammer_add_piece_components` affecting players.
	- Fixes incompatibility with some remove mods.

- v1.6:
	- Adds a new setting `auto_equip` to automatically equip the hammer when selecting an object (enabled by default).
	- Adds a version number check to the Server devcommands mod compatibility.
	- Fixes `disable_marker` setting also disabling the Gizmo visual or the Plan Build visual.
	- Fixes scale not being set when selecting objects from the build menu (better compatibility with some mods).
	- Fixes Piece components being added to the object library when selecting objects (no known issues).

- v1.5
	- Adds support for the undo system of Server devcommands mod (if installed).
	- Adds a new command `hammer_add_piece_components` to allow copying anything with PlanBuild.
	- Removes the `hammer_setup_binds_DEV` command as obsolete (Server devcommands mod is used automatically, if installed).
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

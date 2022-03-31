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

Note: Some commands have a direction parameter. These are intended for mouse wheel binding and are something you probably don't have to use.

- `hammer`: Selects the hovered object to be placed.
- `hammer [item id]`: Selects an object by id ([Item IDs](https://valheim.fandom.com/wiki/Item_IDs)) to be placed.
- `hammer [item id] [scale=1]`: Selects an object by id while setting the initial scale (if the object can be scaled). Number or x,y,z.
- `hammer_add_piece_components`: Adds the Piece component to every object which allows copying them with PlanBuild mod.
- `hammer_config [key] [value]`: Toggles or sets configuration values. For lists, the given value is toggled on or off (`remove_blacklist` or `select_blacklist`).
- `hammer_move [forward,up,right]`: Moves the placement ghost offset for precise placement. Auto value can be used for the object size.
- `hammer_move_backward [meters or number*auto] [direction=1]`: Moves the placement towards the backward direction.
- `hammer_move_down [meters or number*auto] [direction=1]`: Moves the placement towards the down direction.
- `hammer_move_forward [meters or number*auto] [direction=1]`: Moves the placement towards the forward direction.
- `hammer_move_left [meters or number*auto] [direction=1]`: Moves the placement towards the left direction.
- `hammer_move_right [meters or number*auto] [direction=1]`: Moves the placement towards right left direction.
- `hammer_move_up [meters or number*auto] [direction=1]`: Moves the placement towards the up direction.
- `hammer_offset [forward,up,right]`: Sets the placement ghost offset.
- `hammer_offset_x [value]`: Sets the offset in the right / left direction.
- `hammer_offset_y [value]`: Sets the offset in the up / down direction.
- `hammer_offset_z [value]`: Sets the offset in the forward / backward direction.
- `hammer_place`: Allows placing with a key press (for command bindings).
- `hammer_redo`: Restores reverted actions. Uses Server Devcommands undo system if installed.
- `hammer_repair`: Selects the repair tool. Useful for clearing the selection.
- `hammer_rotate_x [degrees] [direction=1]`: Rotates around the X axis.
- `hammer_rotate_x [number*random] [direction=1]`: Randomly rotates around the X axis with a given step size. For example `3*random` would randomly rotate 0, 120 or 240 degrees.
- `hammer_rotate_x [random] [direction=1]`: Randomly rotates around the X axis depending on the object shape (90 or 180 degrees precision).
- `hammer_rotate_y [degrees/number*random/random] [direction=1]`: Rotates around the Y axis.
- `hammer_rotate_y [number*random] [direction=1]`: Randomly rotates around the Y axis with a given step size. For example `3*random` would randomly rotate 0, 120 or 240 degrees.
- `hammer_rotate_y [random] [direction=1]`: Randomly rotates around the Y axis depending on the object shape (90 or 180 degrees precision).
- `hammer_rotate_z [degrees/number*random/random] [direction=1]`: Rotates around the Z axis.
- `hammer_rotate_z [number*random] [direction=1]`: Randomly rotates around the Z axis with a given step size. For example `3*random` would randomly rotate 0, 120 or 240 degrees.
- `hammer_rotate_z [random] [direction=1]`: Randomly rotates around the Z axis depending on the object shape (90 or 180 degrees precision).
- `hammer_scale [scale=1]`: Sets the object scale (if the object can be scaled). Number or x,y,z.
- `hammer_scale_up`: Scales up the object (if the object can be scaled).
- `hammer_scale_down`: Scales down the object (if the object can be scaled).
- `hammer_setup_binds`: Sets some recommended key bindings.
- `hammer_stack [forward,up,right or z1-z2,y1-y2,x1-x2] [step=auto,auto,auto] [direction=1]`: Places multiple objects next to each other.
- `hammer_stack_backward [amount or min-max] [step=auto] [direction=1]`: Places multiple objects towards the backward direction.
- `hammer_stack_down [amount or min-max] [step=auto] [direction=1]`: Places multiple objects towards the down direction.
- `hammer_stack_forward [amount or min-max] [step=auto] [direction=1]`: Places multiple objects towards the forward direction.
- `hammer_stack_left [amount or min-max] [step=auto] [direction=1]`: Places multiple objects towards the left direction.
- `hammer_stack_right [amount or min-max] [step=auto] [direction=1]`: Places multiple objects towards the right direction.
- `hammer_stack_up [amount or min-max] [step=auto] [direction=1]`: Places multiple objects towards the up direction.
- `hammer_undo`: Reverts placing or removing. Uses Server Devcommands undo system if installed.

Note: Some interactions are quite complicated so please report any issues!

## Key bindings

It's recommended to make own bindings ([key codes](https://docs.unity3d.com/ScriptReference/KeyCode.html)).

If you are sharing the config file, put bindings to the `binds` setting with format `keycode1 command1;keycode2 commant2`. For example `keypad0 hammer;keypad7 hammer_undo;keypad9 hammer_redo`.

`hammer_setup_binds` command can be used to quickly set some key bindings that work with the Gizmo mod.

Following bindings are added:

- `bind keypad0 hammer`: Quickly selects the hovered object.
- `bind keypad1 hammer_scale_down`
- `bind keypad2 hammer_scale`: Resets the scaling.
- `bind keypad3 hammer_scale_up`
- `bind keypad7 hammer_undo`
- `bind keypad8 hammer_config enabled`: Toggles all features on/off.
- `bind rightcontrol hammer_redo`
- `bind keypad9 hammer_offset`: Resets the offset.
- `bind rightarrow hammer_move_right 0.1`
- `bind leftarrow hammer_move_left 0.1`
- `bind downarrow hammer_move_down 0.1`
- `bind uparrow hammer_move_up 0.1`

If you have Server Devcommands installed, following binds are added instead (to provide a different offset when Alt-key is down):

- `bind rightarrow hammer_move_right 0.1 keys=-leftalt`
- `bind rightarrow hammer_move_right 1 keys=leftalt`
- `bind leftarrow hammer_move_left 0.1 keys=-leftalt`
- `bind leftarrow hammer_move_left 1 keys=leftalt`
- `bind downarrow hammer_move_down 0.1 keys=-leftalt`
- `bind downarrow hammer_move_down 1 keys=leftalt`
- `bind uparrow hammer_move_up 0.1 keys=-leftalt`
- `bind uparrow hammer_move_up 1 keys=leftalt`

# Configuration

Following powers are available with `hammer_config` command:

- Enabled (default: `true`, key: `enabled`): If false, disabled most features.
- All objects (default: `true`, key: `all_objects`): Hammer can select and place any object. Any placed object can be removed with the hammer until the area is reloaded.
- Allow in dungeons (default: `true`, key: `allow_in_dungeons`): Building is allowed in dungeons.
- Auto equip (default: `true`, key: `auto_equip`): Automatically equips the hammer when selecting an object.
- Binds (default: ` `, key: `binds`): Sets binds at the game start up. Any existing binds are cleared from those keys.
- Build range (default: `0`, key: `build_range`): Range for building (capped at about 50 meters).
- Copy rotation (default: `true`, key: `copy_rotation`): Copies rotation of the selected object.
- Copy state (default: `true`, key: `copy_state`): Object state is copied (for example chest contents or item stand items).
- Disable loot (default: `false`, key: `disable_loot`): Creatures and structures won't drop loot when destroyed with the hammer.
- Disable marker (default: `false`, key: `disable_marker`): Whether the placement ghost is visualized.
- Enable undo (default: `true`, key: `enable_undo`): Whether the undo/redo feature is enabled.
- Ignore no build (default: `true`, key: `ignore_no_build`): "Mystical power" no longer prevents building.
- Ignore other restrictions (default: `true`, key: `ignore_other_restrictions`): Removes any other restrictions (for example campfires can be built on wood floors).
- Ignore wards (default: `true`, key: `ignore_wards`): Wards no longer prevent building.
- Max undo steps (default: `50`, key: `max_undo_steps`): How many undo actions are stored (ignored if Server Devcommands is installed).
- No build cost (default: `true`, key: `no_build_cost`): Removes resource cost and crafting station requirement.
- No creator (default: `false`, key: `no_creator`): Builds without setting the creator information.
- No durability loss (default: `true`, key: `no_durability_loss`): Hammer auto-repairs used durability.
- No stamina cost (default: `true`, key: `no_stamina_cost`): Hammer auto-regens used stamina.
- Overwrite health (default: `0`, key: `overwrite_health`): Sets the health of built or repaired objects (0 reverts to the default max health, except for creatures).
- Remove anything (default: `false`, key: `remove_anything`): Allows removing any object.
- Remove area (default: `0`, key: `remove_area`): Removes same objects within the radius.
- Remove blacklist (default: ` `, key: `remove_blacklist`): Allows disabling remove for given objects (ids separated by ,). Only works if remove anything is enabled.
- Remove effects (default: `false`, key: `remove_effects`): Removes visual effects of building, repairing and destroying.
- Remove range (default: `0`, key: `remove_range`): Range for removing (capped at about 50 meters).
- Repair anything (default: `false`, key: `repair_anything`): Allows healing or repairing any object.
- Repair range (default: `0`, key: `repair_range`): Range for repairing (capped at about 50 meters).
- Repair taming (default: `false`, key: `repair_taming`): Repairing full health creatures will tame/untame them (works for all creatures).
- Scaling step (default: `0.05`, key: `scaling_step`): How much the object is scaled up/down.
- Select blacklist (default: ` `, key: `select_blacklist`): Allows disabling select for given objects (ids separated by ,).
- Select range (default: `0`, key: `select_range`): Range for selecting (capped at about 50 meters).

On servers, above features are disabled without cheat access (except Copy rotate, No placement marker, Remove effects, Select range and offsetting).

Messages from the mod can be configured with following settings:

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

- v1.11:
	- Adds compatibility with Gizmo Reloaded.
	- Adds a new setting `binds` to automatically set binds at the game start up.
	- Adds support for random rotation to `hammer_rotate_*` commands.

- v1.10:
	- Renames and splits commands `hammer_move_*` to be more clear about the direction.
	- Renames and splits commands `hammer_stack_*` to be more clear about the direction.
	- Improves autocomplete and output for most commands.
	- Fixes the undo feature breaking hoe usage.

- v1.9:
	- Fixes `hammer_move_*` commands not working properly.
	- Fixes `hammer_stack_*` not working with rotated objects.

- v1.8:
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

- v1.7:
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

- v1.5:
	- Adds support for the undo system of Server devcommands mod (if installed).
	- Adds a new command `hammer_add_piece_components` to allow copying anything with PlanBuild.
	- Removes the `hammer_setup_binds_DEV` command as obsolete (Server devcommands mod is used automatically, if installed).
	- Fixes some error messages appearing when placing spawners and other objects.
	- Fixes error messages when using the hoe.

- v1.4:
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

- v1.3:
	- Fixes health not being copied for creatures (got overwritten by stars).

- v1.2:
	- Adds object names to the build overlay.
	- Adds setting to disable build, repair and destroy effects.
	- Adds setting to tame/untame creatures with repair.
	- Adds setting to disable creature and structure loot when destroyed with the hammer.
	- Fixes creature stars not getting copied.
	- Fixes error when copying creatures.
	- Fixed "creator" data being added to non-piece objects.
	- Fixes structures having a higher destroy priority even with "Destroy anything" enabled.
	- Fixes "Overwrite health" not working when selecting a piece from the build menu.

- v1.1: 
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

- v1.0: 
	- Initial release.

Thanks for Azumatt for creating the mod icon!

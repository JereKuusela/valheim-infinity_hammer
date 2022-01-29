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
4. Recommended to also install [Gizmo Reloaded](https://www.nexusmods.com/valheim/mods/1293) for better rotating.
5. Optionally also install the [Configuration manager](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/tag/v16.4) to configure the hammer more easily.
6. For servers, install [Dedicated server devcommands](https://valheim.thunderstore.io/package/JereKuusela/Dedicated_server_devcommands/) to use it as an admin.

# Usage

- `hammer`: Selects the hovered object.
- `hammer [item id]`: Selects an object by id ([Item IDs](https://valheim.fandom.com/wiki/Item_IDs)).
- `hammer [item id] [scale=1]`: Selects an object by id while setting the initial scale (if supported). Number or x,y,z.
- `hammer_undo`: Reverts placing or removing.
- `hammer_redo`: Restores reverted actions.
- `hammer_scale [scale=1]`: Sets the object scale (if supported). Number or x,y,z.
- `hammer_scale_up`: Scales up the object (if supported).
- `hammer_scale_down`: Scales down the object (if supported).
- `hammer_move_x [value]`: Moves the X offset.
- `hammer_move_Y [value]`: Moves the Y offset.
- `hammer_move_Z [value]`: Moves the Z offset.
- `hammer_move [x,y,z]`: Moves the offset.
- `hammer_offset_x [value]`: Sets the X offset.
- `hammer_offset_Y [value]`: Sets the Y offset.
- `hammer_offset_Z [value]`: Sets the Z offset.
- `hammer_offset [x,y,z]`: Sets the offset.
- `hammer_setup_binds`: Sets recommended key bindings.
- `hammer_setup_binds_DEV`: Sets recommended key bindings (with Dedicated Server Devcommands mod).
- `hammer_config [key] [value]`: Toggles or sets configuration values.

Note: Some interactions are quite complicated so please report any issues!

## Recommended bindings

`hammer_setup_binds` command can be used to quickly set recommended key bindings that work with the Gizmo mod.

For advanced users, it's recommended to make own bindings ([key codes](https://docs.unity3d.com/ScriptReference/KeyCode.html)).

Following bindings are added:

- `bind keypad0 hammer`: Quickly selects the hovered object.
- `bind keypad1 hammer_scale_down`
- `bind keypad2 hammer_scale`: Resets the scaling.
- `bind keypad3 hammer_scale_up`
- `bind keypad7 hammer_undo`
- `bind keypad8 hammer_config enabled`: Toggles all features on/off.
- `bind rightcontrol hammer_redo`
- `bind KeyPad9 hammer_offset 0,0,0`: Resets the offset.
- `bind rightarrow hammer_move_z -0.1`
- `bind leftarrow hammer_move_z 0.1`
- `bind downarrow hammer_move_y -0.1`
- `bind uparrow hammer_move_y 0.1`

If you have Dedicated Server Devcommands installed, you should use `hammer_setup_binds_DEV` instead. This provides a different offset amount when holding the LeftAlt key down:

- `bind rightarrow hammer_move_z -0.1 keys=-leftalt`
- `bind rightarrow hammer_move_z -1 keys=leftalt`
- `bind leftarrow hammer_move_z 0.1 keys=-leftalt`
- `bind leftarrow hammer_move_z 1 keys=leftalt`
- `bind downarrow hammer_move_y -0.1 keys=-leftalt`
- `bind downarrow hammer_move_y -1 keys=leftalt`
- `bind uparrow hammer_move_y 0.1 keys=-leftalt`
- `bind uparrow hammer_move_y 1 keys=leftalt`

# Configuration

Following powers are available:

- All objects: Hammer can select and place any object. Any placed object can be removed with the hammer until the area is reloaded.
- Allow in dungeons: Building is allowed in dungeons.
- Copy rotation: Copies rotation of the selected object.
- Build range: Range for building (capped at about 50 meters).
- Copy state: Object state is copied (for example chest contents or item stand items).
- Disable loot: Creatures and structures won't drop loot when destroyed with the hammer.
- Ignore no build: "Mystical power" no longer prevents building.
- Ignore other restrictions: Removes any other restrictions (for example campfires can be built on wood floors).
- Ignore wards: Wards no longer prevent building.
- Max undo steps: How many undo actions are stored.
- No build cost: Removes resource cost and crafting station requirement.
- No creator: Builds without setting the creator information. Disabled by default.
- No durability loss: Hammer auto-repairs used durability.
- No placement marker: Removes the placement marker (and Gizmo too). Disabled by default.
- No stamina cost: Hammer auto-regens used stamina.
- Overwrite health: Sets the health of built or repaired objects (0 reverts to the default max health, except for creatures).
- Remove anything: Allows removing any object. Disabled by default.
- Remove effects: Removes visual effects of building, repairing and destroying.
- Remove range: Range for removing (capped at about 50 meters).
- Repair anything: Allows healing or repairing any object. Disabled by default.
- Repair range: Range for repairing (capped at about 50 meters).
- Repair taming: Repairing full health creatures will tame/untame them (works for all creatures).
- Scaling step: How much the object is scaled up/down.
- Select range: Range for selecting (capped at about 50 meters).

On servers, above features are disabled without cheat access (except Copy rotate, No placement marker, Remove effects, Select range and offsetting).

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

IF "Disable loot" is enabled, destroying creatures or structures won't drop loot. This can be useful to get rid of very high starred creatures that crash the game when killed.

# Changelog

- v1.5:
	- Fixed some error messages appearing when placing spawners and other objects.

- v1.4:
	- New commands to offset the placement to precisely set the position.
	- New commands to set recommended key bindings.
	- Added new setting to disable the placement marker.
	- Added new parameter to hammer command to set the initial scale.
	- Added messages for undo and redo actions.
	- Overwrite health changed to set the current health slightly higher than the maximum (makes it less likely to reset).
	- Fixed scale being applied to objects that don't support it.
	- Fixed tamed status not being copied for creatures.
	- Fixed hammer_scale not working with different scales per axis.
	- Fixed "Select range" setting not working.
	- Fixed "Remove range" setting not working.

- v1.3:
	- Fixed health not being copied for creatures (got overwritten by stars).

- v1.2:
	- Added object names to the build overlay.
	- Added setting to disable build, repair and destroy effects.
	- Added setting to tame/untame creatures with repair.
	- Added setting to disable creature and structure loot when destroyed with the hammer.
	- Fixed creature stars not getting copied.
	- Fixed error when copying creatures.
	- Fixed "creator" data being added to non-piece objects.
	- Fixed structures having a higher destroy priority even with "Destroy anything" enabled.
	- Fixed "Overwrite health" not working when selecting a piece from the build menu.

- v1.1: 
	- Size is no longer shown for objects that don't support changing it.
	- Messages now have a high priority (fixes scaling messages lagging behind).
	- Creator is now properly set for copied objects (unless "No Creator" is on).
	- Auto rotate setting renamed to Copy rotation.
	- Added new setting to overwrite the health of built and repaired objects (including creatures).
	- Added no stamina and durability cost to also affect repairing.
	- Added new setting to change repair range.
	- Added new setting to repair anything (including creatures).
	- Added support for non-uniform scaling with hammer_scale command.
	- Fixed selection keeping the hover color when selecting a structure.
	- Fixed selection being removed when the selected objects is destroyed.

- v1.0: 
	- Initial release

Thanks for Azumatt for creating the mod icon!

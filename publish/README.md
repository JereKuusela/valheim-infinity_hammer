# Infinity Hammer

Bend the rules of building! Construct any object anywhere, destroy anything and more...

Client-side mod that is compatible with unmodded clients.

# Manual Installation:

1. Install the [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim).
2. Download the latest zip.
3. Extract it in the \<GameDirectory\>\BepInEx\plugins\ folder.
4. Recommended to also install [Gizmo Reloaded](https://www.nexusmods.com/valheim/mods/1293) for better rotating.
5. Optionally also install the [Configuration manager](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/tag/v16.4) to configure the hammer more easily.
6. For servers, install [Dedicated server devcommands](https://valheim.thunderstore.io/package/JereKuusela/Dedicated_server_devcommands/) to use it as an admin.

# Usage

- hammer: Selects the currently hovered object.
- hammer [item id]: Selects an object by id ([Item IDs](https://valheim.fandom.com/wiki/Item_IDs)).
- hammer_undo: Reverts building or removing actions.
- hammer_redo: Restores reverted actions.
- hammer_scale [value=1]: Scales the selection (only for supported objects). Number or x,y,z.
- hammer_scale_up: Scales up the selection (only for supported objects).
- hammer_scale_down: Scales down the selection (only for supported objects).
- hammer_config [key] [value]: Toggles or sets configuration values.

Note: Some interactions are quite complicated so plesae report any issues!

Bind commands to [keys](https://docs.unity3d.com/ScriptReference/KeyCode.html).

For example:

- bind KeyPad0 hammer
- bind KeyPad1 hammer_scale_down
- bind KeyPad2 hammer_scale
- bind KeyPad3 hammer_scale_up
- bind KeyPad7 hammer_undo
- bind KeyPad8 hammer_config enabled
- bind KeyPad9 hammer_redo

# Configuration

Following powers are available and can be disabled from the config file:

- All objects: Hammer can select and place any object. Placed objects can be removed with the hammer until the area is reloaded.
- Allow in dungeons: Building is allowed in dungeons.
- Copy rotation: Copies rotation of the selected object.
- Build range: Range for building (capped at about 50 meters).
- Copy state: Object state is copied (for example chest contents or itemstand items).
- Ignore no build: "Mystical power" no longer prevents building.
- Ignore other restrictions: Removes any other restrictions (for example campfires can be built on wood floors).
- Ignore wards: Wards no longer prevent building.
- Max undo steps: How many undo actions are stored.
- No build cost: Removes resource cost and crafting station requirement.
- No creator: Builds without setting the creator information (won't be targeted by the enemies). Disabled by default.
- No durability loss: Hammer autorepairs used durability.
- No stamina cost: Hammer auto-regens used stamina.
- Overwrite health: Sets the health of built or repaired objects (0 reverts to the default max health, except for creatures).
- Remove anything: Allows removing any object. Disabled by default.
- Remove range: Range for removing (capped at about 50 meters).
- Repair anything: Allows repairing any object. Disabled by default.
- Repair range: Range for repairing (capped at about 50 meters).
- Scaling step: How much the selection scales up/down.
- Select range: Range for selecting (capped at about 50 meters).

On servers, above features are disabled without cheat access (except Auto rotate and Select range).

# Building

Hammer configuration applies to all building, including the standard structures selected from the build menu.

When selecting an existing object, its size and rotation is copied to the placement tool. If "Copy rotation" is disabled then the selection tool keeps the last rotation. The last rotation is always used when using the build window.

Object scaling only works for some objects (mostly trees and rocks). This is restricted by the base game (scaling is not stored in the save file).

If "Overwrite health" is enabled, objects have a specified health (including creatures). For minerocks, the health is applied to the individual parts (the outer shell stays at 1 health). Repairing can be used to modify the shell health if needed.

"Copy state" only applies when selecting existing objects since structures from the build menu are stateless. However the creator ID is always set bsaed on the "No creator" setting, even for non-standard structures.

# Repairing

By default, only change is that the UI shows how much damage was repaired.

If "Repair anything" is enabled, most destructible objects can be repaired or healed. This includes creatures.

For minerocks, if the targeted part is already at full health, a random part is restored instead. This is not very practical but can be used to restore any accidental changes to minerocks.

If "Overwrite health" is enabled, the object is repaired or damaged to the specified health value.

For creatures, the maximum health value is also set. So they will keep their max health even when disabling "Overwrite health". Other objects will revert to the original max health when repaired.

# Destroying

By default, destroying only works for standard structures and placed objects. Placed objects can only be removed temporarily since the required informatin is lost when the area is reloaded.

If "Destroy anything" is enabled, all objects can be removed. Removing non-standard objects will instantly destroy them without triggering any effects like drops.


# Changelog

- v1.1.0: 
	- Size is no longer shown for objects that don't support changing it.
	- Messages now have a high priority (fixes scaling messages lagging behind).
	- Slightly better support when selecting pieces from the build window.
	- Creator is now properly set for copied objects (unless "No Creator" is on).
	- Auto rotate setting renamed to Copy rotation.
	- Added new setting to overwrite the health of built and repaired objects (including creatures).
	- Added no stamina and durability cost to also affect repairing.
	- Added new setting to change repair range.
	- Added new setting to repair anything (including creatures).
	- Added support for non-uniform scaling with hammer_scale command.

- v1.0.0: 
	- Initial release

Thanks for Azumatt for creating the mod icon!

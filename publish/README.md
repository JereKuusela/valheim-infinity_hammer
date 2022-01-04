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
- hammer_scale [value=1]: Scales the selection (only for supported objects).
- hammer_scale_up: Scales up the selection (only for supported objects).
- hammer_scale_down: Scales down the selection (only for supported objects).
- hammer_config [key] [value]: Toggles or sets configuration values.

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
- Auto rotate: Selected object is automatically rotated.
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
- Remove anything: Allows removing any object with the hammer. Disabled by default.
- Remove range: Range for removing (capped at about 50 meters).
- Scaling step: How much the selection scales up/down.
- Select range: Range for selecting (capped at about 50 meters).

On servers, above features are disabled without cheat access (except Auto rotate and Select range).

# Changelog

- v1.0.0: 
	- Initial release

Thanks for Azumatt for creating the mod icon!

# Infinity Hammer

Bend the rules of building by constructing any object anywhere.

# Manual Installation:

1. Install the [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim)
2. Download the latest zip
3. Extract it in the \<GameDirectory\>\BepInEx\plugins\ folder.
4. Optionally also install the [Configuration manager](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/tag/v16.4) to configure the hammer more easily.
5. Recommended to also install [Gizmo Reloaded](https://www.nexusmods.com/valheim/mods/1293) for better rotating.
6. For servers, install [Dedicated server devcommands](https://valheim.thunderstore.io/package/JereKuusela/Dedicated_server_devcommands/) to use it as an admin.

# Usage

Bind the hammer command to a preferred [key](https://docs.unity3d.com/ScriptReference/KeyCode.html). For example "/bind Mouse5 hammer" in the chat window.

Pressing the key will copy the hovered object and allow placing it with the hammer tool.

You can also use the command manually "hammer [item id]" to select a given item. Check [wiki](https://valheim.fandom.com/wiki/Item_IDs) for possible item ids.

# Configuration

Following powers are available and can be disabled from the config file:

- All objects: Hammer can select and place any object. Placed objects can be removed with the hammer until the area is reloaded.
- Allow in dungeons: Building is allowed in dungeons.
- Copy state: Object state is copied (for example attached item of itemstands).
- Ignore no build: "Mystical power" no longer prevents building.
- Ignore other restrictions: Removes any other restrictions (for example campfires can be built on wood floors).
- Ignore wards: Wards no longer prevent building.
- No build cost: Removes resource cost and crafting station requirement.
- No durability loss: Hammer autorepairs used durability.
- No stamina cost: Hammer auto-regens used stamina.
- Remove anything: Allows removing any object with the hammer. Disabled by default.

On servers, above features are disabled without cheat access.

# Changelog

- v1.0.0: 
	- Initial release
# Infinity Hammer

Bend the rules of building! Copy any object, make structures indestructible, remove all restrictions, destroy anything and more...

Install on the admin client (modding [guide](https://youtu.be/L9ljm2eKLrk)).

Install [Server Devcommands](https://valheim.thunderstore.io/package/JereKuusela/Server_devcommands/).

Install [World Edit Commands](https://valheim.thunderstore.io/package/JereKuusela/World_Edit_Commands/).

Recommended mods:

- [Gizmo](https://valheim.thunderstore.io/package/ComfyMods/Gizmo/): Enables free rotation.
- [More Vanilla Build Prefabs](https://valheim.thunderstore.io/package/Searica/More_Vanilla_Build_Prefabs/): Adds more objects to the build menu.

This mod has a massive amount of features and setting. For ease of use, the documentation is split into three parts.

## Building

- Build and remove anything.
- Copy existing pieces with their contents.
- Quality of life: Extended range, no resource costs, no item drops, no visual effects.
- Precise placement (offset + freeze).
- Remove build restrictions.
- Unbreakable structures.
- Undo/redo system.

See [hammer.md](hammer.md) for more information.

## Blueprints

- Load [PlanBuild](https://valheim.thunderstore.io/package/MathiasDecrock/PlanBuild/) .blueprints files.
- Load BuildShare .vbuild files.
- Save new PlanBuild .blueprints files.
- Automatic snap points.

See [blueprints.md](blueprints.md) for more information.

## Custom hammer

- Using command `hammer_menu` gives you a custom Infinity Hammer.
- New hammer allows selecting any piece, location, room or tool from the build menu.

See [custom_hammer.md](custom_hammer.md) for more information.

## Tools

- Add console commands to the build menu.
- Execute console commands with the hammer.
- Easily change the shape and size of the affected area.
- Precise terrain changes.

See [tools.md](tools.md) for more information.

### Configuration

There are lots of customization available. The simplest way is to install Configuration Manager mod but you can modify the setting file directly too.

For advanced use, there is a command `hammer_config` which you can bind to change settings more easily.

For example `bind u hammer_config tools_enabled` to toggle tools in the build menu.

For example `bind u hammer_config set_invulnerability On/Off` to toggle piece invulnerability.

For example `bind u hammer_config overwrite_health 0/100/1000` for different amount of health.

### Key bindings

Some commonly used features have pre-made key binds in the configuration. Internally these use the `bind` command.

If you don't wish to use this system you can set all binds to none and make your own bindings with the `bind` command.

Some mouse wheel binds can't be unbound. In this case, set them some unused key combination.

## Credits

Thanks for Azumatt for creating the mod icon!

Sources: [GitHub](https://github.com/JereKuusela/valheim-infinity_hammer)

Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)

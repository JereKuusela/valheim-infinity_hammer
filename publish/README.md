# Infinity Hammer

Bend the rules of building! Copy any object, make structures indestructible, remove all restrictions, destroy anything and more...

Install on the admin client (modding [guide](https://youtu.be/L9ljm2eKLrk)).

Install also [Server Devcommands](https://valheim.thunderstore.io/package/JereKuusela/Server_devcommands/) to enable key binds and use this on a server (as an admin).

Install also [World Edit Commands](https://valheim.thunderstore.io/package/JereKuusela/World_Edit_Commands/) for terrain tools.

# Usage

See [documentation](https://github.com/JereKuusela/valheim-expand_world/blob/main/README.md).

# Credits

Thanks for Azumatt for creating the mod icon!

Sources: [GitHub](https://github.com/JereKuusela/valheim-infinity_hammer)
Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)

# Changelog

- v1.22
	- Adds a new parameter `id` to commands for using the hovered object id.
	- Adds a new parameter `pick` to the `hammer` command.
	- Adds a new parameter `freeze` to the `hammer` command.
	- Adds new bind settings.
	- Adds yaml file for commands.
	- Adds "InfinityHammer" category to saved blueprints.
	- Adds cheat checks to scaling and zoom commands.
	- Improves default commands/tools.
	- Improves support for picking up and freezing at the same time.
	- Improves object copying to work better with modded objects.
	- Fixes dimensions of hanging_hairstrands, wood_beam_26, wood_beam_45, wood_wall_roof_top and wood_wall_roof_top_45 (reset the setting to get the new values).
	- Fixes the area select tool breaking up.
	- Fixes object count being saved in blueprints.
	- Fixes snapping not working on blueprints.
	- Fixes freezing sometimes making the selection disappear.
	- Fixes commands with modifier keys not working if a modifier key was unbound.
	- Fixes snappoints messing up blueprint data saving.
	- Fixes the `unfreeze_on_select` setting not always working.

- v1.21
	- Adds new commands `hammer_zoop_[direction]` to multiply the selection.
	- Adds a new setting `dimensions` to define object dimensions.
	- Adds a new command `hammer_measure` to measure most objects.
	- Adds automatic snap points to multi selection.

- v1.20
	- Adds new parameters `fall` and `restrict` to the `hammer` command for Structure Tweaks mod.
	- Adds new aliases `hammer_terrain_to`, `hoe_terrain_to`, `hammer_slope` and `hoe_slope` for World Edit Commands mod.
	- Adds visualization for edge targeted commands (instead of the usual center targeted).
	- Adds new key bindings for picking the hovered object and picking the whole building.
	- Adds a new key binding for resetting offset (unbound by default).
	- Adds a new key binding for stacking (unbound by default).
	- Adds a new setting `show_command_values` to always show the command on tool descriptions.
	- Adds new key bindings for modifier keys.
	- Adds amount of selected objects to the multiselect.
	- Adds settings for changing the modifier keys for tools.
	- Changes Pipette and Area Pipette tools to support picking up objects.
	- Fixes multiselect being always scalable. Now only scales if all child objects can be scaled.
	- Fixes multiselect not copying the child scale.
	- Fixes remove effects not working for building.
	- Improves multicommand support.
	- Improves instant command support.

- v1.19
	- Adds a new paramater `type` to the `hammer` command to filter by object type.
	- Adds new paramaters to the `hammer` command for Structure Tweaks mod.
	- Renames commands `hammer_scale* build` to `hammer_zoom*`.
	- Renames commands `hammer_scale* command` to `hammer_zoom_cmd*`.
	- Adds new commands `hammer_scale*` and  `hammer_´scale_cmd*` to directly set the scale.
	- Fixes the mod not working with modded tools like PlanBuild rune.
	- Fixes PlanBuild snap points not working.
	- Fixes selection being shared across all tools.
	- Fixes starred creatures having wrong color/size on the placement ghost.

- v1.18
  - Adds new settings `move_amount` and `move_amount_large` to configure default bindings.
	- Adds new command aliases `hammer_area`, `hammer_object` and `hammer_terrain`.
	- Adds rectangle shape to `hammer_area`.
	- Changes most default bindings only work when the hammer or hoe is equipped.
	- Changes the command description to always show the actual command.
	- Fixes clicking an empty slot on build menus causing an error message.
	- Fixes the `no_creator` setting not working.
	- Removes the rotation normalize from multiselects (didn't really work at all).

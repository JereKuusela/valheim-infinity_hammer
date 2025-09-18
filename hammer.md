
# Building

## Selecting objects

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
- `type=component1,component2,...`: Select only certain object types.
- `ignore=id1,id2,...`: Ignores certain object ids.
  - These ids are combined with the setting "Ignored ids".
- `include=id1,id2,...`: Includes only certain object ids.

For Structure Tweaks mod:

- `collision=false`: Removes collision.
- `fall=off/solid/terrain`: Overrides the fall behavior.
- `growth=big/big_bad/small/small_bad`: Overrides the plant growth.
- `interact=false`: Removes interaction.
- `show=false`: Removes visibility.
- `restrict=false`: Removes portal item restrictions.
- `wear=broken/damaged/healthy`: Overrides the structure wear.

For example `hammer Beech1 scale=2 health=1000` would select a beech tree with a double size and 1000 health.

## Marking

Individual pieces can be marked for selection with command `hammer_mark`.

When a selection is made with `hammer` command, all marked pieces are included in the selection.

## Scale

The scale can be directly set with following commands:

- `hammer_scale`: Resets the size.
- `hammer_scale [x,z,y or amount]`: Sets the size.
- `hammer_scale_x [amount]`: Sets only the x-axis.
- `hammer_scale_y [amount]`: Sets only the y-axis.
- `hammer_scale_z [amount]`: Sets only the z-axis.

## Zoom

Shift + mouse wheel will zoom up/down the selection.

This can be also done with following commands:

- `hammer_zoom [x,z,y or percentage or amount]`: Zooms the size.
- `hammer_zoom_x [percentage or amount]`: Zooms only the x-axis.
- `hammer_zoom_y [percentage or amount]`: Zooms only the y-axis.
- `hammer_zoom_z [percentage or amount]`: Zooms only the z-axis.

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

Building and removing objects can be quickly reversed by pressing NumPad7. Reversed actions can be redone with NumPad9.

Note: Any other removal like buildings collapsing due to lack of support or destroyed by enemies is not tracked. Only your direct actions can be undone.

## Multiplacement

The selection can be multiplied with zooping:

- `hammer_zoop_[direction] [step=auto]`

Possible directions are `backward`, `down`, `forward`,`left`, `right` and `up`.

The step sets the distance between cloned objects. By default this is the size of the object.

The recommended way is to set a keybinding in the config. Custom bindings can also be used for special cases.

Current zoop can be reset with command `hammer_zoop_reset`.

Examples:

- `hammer_zoop_right auto`: Clones the selected object to next to it.
- `hammer_zoop_right 2*auto`: Clones the selected object while leaving a gap (equal to the object size).
- `hammer_zoop_up 5`: Clones the selected object to 5 meters above it.

Setting "Zoop magic mode" can be used to make the zooping to behave erratically. Mild version automatically centers the selection which causes next zoops being put to wrong places. Wild version centers to random piece which causes the zoops to scatter around.

### Stacking

Stacking is similar to zooping but instantly places the objects:

- `hammer_stack_[direction] [amount or min-max] [step=auto]`

The amount determines how many objects are placed. This can be fine-tuned by giving a range.

If nothing is selected, stacking is applied to the hovered piece.

Note: Choosing the correction direction can be difficult when rotating the selection. But you can easily try things by reverting the changes with `undo`.

Examples:

- `hammer_stack_up 10`: Places 10 objects on top of each other.
- `hammer_stack_up -5-4`: Places 10 objects with 5 below and 4 on top of the main object.
- `hammer_stack_left 5 10`: Places 5 objects with 10 meters between each of them.
- `hammer_stack_left 5 2*auto`: Places 5 objects while leaving a gap between them (equal to the object size).
- `hammer_stack_left 3-4`: Places 2 objects some distance away from the selected position.

To place objects in a rectangle or a box, use the following command:

- `hammer_stack [forward,up,right or z1-z2,y1-y2,x1-x2] [step=auto,auto,auto]`

## Dungeons

Existing dungeons can be copied or picked up.

When remove anything is enabled, individual rooms can be removed from dungeons. When the last room is removed, the dungeon generator object is also removed.

The command `hammer_room [room id] [empty_room]` allows selecting a single room. These can be added to existing dungeons.

The parameter "empty_room" determines if the room is placed empty or with the default contents. If the parameter is missing, the default value comes from "Place empty rooms" setting.

## Utility

Some special commands exist for advaced cases. Usually you want to bind these.

- `hammer_add_piece_components`: Adds the Piece component to every object which allows copying them with PlanBuild mod.
- `hammer_grid [precision] [center=current]`: Restricts possible placement coordinates. Using the same command removes the restriction.
- `hammer_measure`: Measures most objects updating `dimensions` setting.
- `hammer_mirror`: Mirrors the selection.
- `hammer_place`: Allows placing with a key press.
- `hammer_repair`: Selects the repair tool for quickly clearing the selection.

For example you could bind command `hammer_rotate_y random;hammer_place` to build objects with a random rotation.

## Configuration

- Enabled (default: `true`): If disabled, removes most features.
- Allow in dungeons (default: `true`): Building is allowed in dungeons.
- Dimensions: Measurements for objects.
- Disable loot (default: `false`): Creatures and structures won't drop loot when destroyed with the hammer.
- Ignore no build (default: `true`): "Mystical power" no longer prevents building.
- Ignore other restrictions (default: `true`): Removes any other restrictions (for example campfires can be built on wood floors).
- Ignore wards (default: `true`): Wards no longer prevent building.
- Ignored ids: Objects ignored by this mod (ids separated by ,).
- Ignored remove ids: Additional ids that are ignored when removing anything.
- Infinite health (default: `false`): Sets the Overwrite health setting to 1E30.
- No cost (default: `false`): Removes durability, resource and stamina costs.
- No creator (default: `false`): Reduces save data by not setting the creator id.
- No primary target (default: `false`): Removes the primary target status.
  - Requires World Edit Commands mod on the server, otherwise the change is removed on world load.
- No secondary target (default: `false`): Removes the secondary target status.
  - Requires World Edit Commands mod on the server, otherwise the change is removed on world load.
- Overwrite health (default: `0`): Sets the health of built or repaired objects (0 reverts to the default max health, except for creatures).
- Range (default: `0`): Range for the hammer (capped at about 50 meters).
- Remove anything (default: `false`): Allows removing any object.
- Remove area (default: `0`): Removes same objects within the radius.
- Repair anything (default: `false`): Allows healing or repairing any object.
- Reset offset on unfreeze (default `true`): Removes the placement offset when unfreezing the placement.
- Set invulnerability (default: `Off`): Built objects can't take any damage.
  - The exact mechanic depends on the object type:
    - Creatures: Very high health causes damage to be rounded down to zero.
    - Destructibles, mine rocks and trees: Very high tool tier makes them immune to damage.
    - Structures: Negative health (-1) prevents them from taking any damage.
  - Options:
    - Off: Removes existing invulnerability.
    - On: Enables invulnerability.
    - Damaged: Only applies to structures. Sets -2 max health results in damaged look.
    - Worn: Only applies to structures. Sets -4 max health that results in worn look.
    - Legacy: Sets a very high health (1E30) for all object types.
- Preserve wear levels" (default: `false`): Prevents wear level from being overridden by invulnerability setting.
  - Structures that are already invulnerable will keep their wear level.
  - Structures that are damaged will get the matching invulnerability level.
- Show command values (default: `false`): Always show the command on tool descriptions.
- Snap points for all objects (default: `false`):If enabled, multi selection creates snap points for every object.
- Unfreeze on select (default `true`): Removes the placement freeze when selecting a new object.
- Unfreeze on unequip (defualt `true`): Removes the placement freeze when unequipping the hammer.

On servers, above features are disabled without cheat access (except visual changes and offsetting).

### Visual

Visual settings work even without cheat access. These all are disabled by default.

- No effects: Hides visual effects of building, repairing and destroying.
- No health indicator: Hides the piece health bar.
- No placement marker: Hides the yellow placement marker (also affects Gizmo mod).
- No support indicator: Hides the color that shows support.

### Output

Messages from the mod can be configured with following settings:

- `disable_messages`: Disables all messages from the mod (console output not affected).
- `disable_offset_messages`: Disables messages from changing the placement offset.
- `disable_scale_messages`: Disables messages from changing the object scale.
- `disable_select_messages`: Disables messages from selecting objects.

## Info

### General

Hammer configuration applies to all building, including the standard structures selected from the build menu.

When selecting an existing object, its size and rotation is copied to the placement tool. The last rotation is always used when using the build window.

Object scaling only works for some objects (mostly trees and rocks). This is restricted by the base game (scaling is not stored in the save file). [Structure Tweaks](https://valheim.thunderstore.io/package/JereKuusela/Structure_Tweaks/) mod can be used to enable scaling for all objects (required for all clients).

If "Overwrite health" is enabled, objects have a specified health (including creatures). For minerocks, the health is applied to the individual parts.

Setting a very high health (like "1E30") can be used to make object indestructible because the damage taken is rounded down to zero. This also prevents structures collapsing from lack of support. However this will increase network traffic because of constant health updates. Recommended to use the "Set invulnerability" setting instead which doesn't have this issue.

For creatures, the max health resets when the area is reloaded if the current health equals the max health. For this reason, the current health is set slightly higher than the max health.

"No creator" is mainly for reducing the save file size.

Locations (Points of Interest) can also be copied. However only static parts are included in the actual location. For example in the start temple, each boss stone is a separate object and can be copied separately if needed.

### Repairing

When "Repair anything" is enabled, most destructible objects can be repaired or healed. This includes creatures and players.

For minerocks, if the targeted part is already at full health, a random part is restored instead. This is not very practical but can be used to restore accidental changes to minerocks.

When "Overwrite health" is set, the object is repaired or damaged to the specified health value.

For creatures, the maximum health value is also set. So they will keep their max health even when disabling "Overwrite health". Other objects will revert to the original max health when repaired.

### Destroying

By default, destroying only works for standard structures and placed objects. Placed objects can only be removed temporarily since the required information is lost when the area is reloaded.

If "Destroy anything" is enabled, all objects can be removed.

If "Disable loot" is enabled, destroying creatures or structures won't drop loot. This can be useful to get rid of very high starred creatures that crash the game when killed.

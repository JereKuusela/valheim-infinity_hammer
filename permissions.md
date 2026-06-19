# Infinity Hammer permissions

Infinity Hammer supports permission system from Server Devcommands mod.

Section name is `infinityhammer`

Setting names are:

- `no_cost`: no resource, durability, and stamina costs.
- `ignore_wards`: bypassing ward restrictions.
- `ignore_no_build`: bypassing no-build areas.
- `allow_in_dungeons`: building where pieces are normally disallowed in dungeons.
- `ignore_other_restrictions`: bypassing material, biome, and similar build restrictions.
- `remove_anything`: removing any object type.
- `disable_loot`: preventing loot drops when removing.
- `repair_anything`: repairing normally non-repairable objects.
- `no_creator`: not assigning creator id to built pieces.
- `no_target`: preventing pieces from being enemy targets.
- `no_physics`: static placement behavior.
- `no_remove`: preventing piece removal.
- `overwrite_health`: allowing overwrite health values to be applied.
- `invulnerability`: allowing invulnerability mode application.
- `range`: allowing custom range override usage.
- `tools_enabled`: adding tools into the build menu.

Also keep in mind that permission can be granted for specific console commands.

This can also be used to turn off specific tools (as they internally use console commands).

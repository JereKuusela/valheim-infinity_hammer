## 1.80.4

- Select hoe/cultivator tools from uncategorized piece tables.
- Keep menu tool identities stable across availability refreshes; clean them up on tool reload and logout.
- Clear command previews when selecting native pieces, and prevent menu action buttons from becoming placement previews.
- Refresh the native menu when navigating; repeat a menu command to close it, or use `hammer_menu close`.
- Find inactive projector children and suspend ruler work while the build menu is open.

## 1.80.3

- Stop RandomMaterialValues polling on placement previews, including inactive children.
- Skip projector/ruler work while previews are hidden and avoid duplicate CircleProjector updates.
- Release native-created preview materials when their ghost is destroyed; preserve shared assets.
- Clear the active selection reference on destruction.

- v1.80.1 (Deep North 1.0.7 prototype, iteration 1)
  - Updates build-menu collections, equipment hashes, stand blueprint data, hammer creation and creator attribution for game 1.0.7.
  - Fixes the removal transpiler, placement return branches, location damage override ordering and temporary-state cleanup.
  - Requires the accompanying WEC 1.74.1 and patched SDC 1.109.1. In-game validation pending.

- v1.80
  - Adds support for the permission system from Server Devcommands mod. This allows granularly granting access to specific commands and features.
  - Adds current blueprint names to the autocomplete of `hammer_save` command.
  - Adds text input when `hammer_save` command is used without a name.
  - Adds support for capturing terrain data and saving it to a blueprint. Thanks sighsorry!
  - Improves compatibility with PlanBuild mod (terrain data is ignored). Thanks Haloa!
  - Fixes some prefabs disappearing from selection (Ragdoll, LocationProxy, MapTable, ArcheryTarget and Plant components are now removed). Thanks Haloa!
  - Fixes instant tools always using player coordinates instead of the hovered coordinates.

- v1.79
  - Improves compatibility with PlanBuild mod (extra info, item drops, item stand orientation). Thanks Haloa!
  - Fixes some prefabs disappearing from selection (TimedDestruction and Vine components are now removed). Thanks Haloa!

- v1.78
  - Adds a new menu for vegetation in the Infinity Hammer build menu.
  - Adds support for receiving server side location and vegetation IDs from Expand World Data mod.

- v1.77
  - Adds a new setting "No target" to replace both "No primary target" and "No secondary target" settings.
  - Adds a new setting "No remove" to prevent placed pieces from being removed by players.
  - Adds a new setting "No physics" to disable fall and push up properties.
  - Fixes prefabs starting with "vx_" not appearing in visuals menu.
  - Fixes workbench radius projector not updating during placement.

- v1.76
  - Fixes some issues with build tables.

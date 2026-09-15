- v1.84
  - Adds new setting to snap zoom scaling at specific increments.
  - Fixes favorites + tools + keep equip on death combination causing null reference exception.

- v1.83
  - Fixes location placing not working.
  - Fixes selecting repar hammer causing error.

- v1.82
  - More fixes. Thanks JPValheim!

- v1.81
  - Fixes for the new game update. Some things might not work properly.

- v1.80
  - Adds support for the permission system from Server Devcommands mod. This allows granularly granting access to specific commands and features.
  - Adds current blueprint names to the autocomplete of `hammer_save` command.
  - Adds text input when `hammer_save` command is used without a name.
  - Adds support for capturing terrain data and saving it to a blueprint. Thanks sighsorry!
  - Improves compatibility with PlanBuild mod (terrain data is ignored). Thanks Haloa!
  - Fixes some prefabs disappearing from selection (Ragdoll, LocationProxy, MapTable, ArcheryTarget and Plant components are now removed). Thanks Haloa!
  - Fixes instant tools always using player coordinates instead of the hovered coordinates.

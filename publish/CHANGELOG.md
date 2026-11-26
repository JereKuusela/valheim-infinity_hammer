- v1.96
  - Adds new field to tool to specify in which build menu materials they appear.
  - Adds negative value support to tool index.
  - Adds piece name support to tool index.
  - Changes Infinity Hammer build menu to have flatter structure.
  - Fixes tool index not working (put always at end).

- v1.95
  - Adds new field to tools to specify in which build menu categories they appear.
  - Fixes objects with Trap component not working in blueprints or area selections.
  - Fixed for the new update.

- v1.94
  - Fixes compatibility issue with signs from Odins Kingdom mod.

- v1.93
  - Adds support for alternative objects in blueprints (chance with a group like `0.5_a`) when `Use blueprint chance` is enabled.
  - Changes blueprint loading to use the shared code of World Edit Commands mod.
  - Changes blueprints without `#Center` to be centered on `piece_bpcenterpoint` (if the config `Blueprint center piece` is empty).
  - Changes invalid terrain in a blueprint to only skip the terrain instead of failing the whole blueprint.
  - Changes blueprint comments (`# text`) and unknown sections to follow the shared format.
  - Fixes commas being replaced in the blueprint info text.
  - Adds new setting "Prefer correct tools" for the new behavior (so that people can use the old way).
  - Fixes compatibility issue with the MasterBuilderHeeler mod.

- v1.92
  - Changes commands to prefer equipping the right tool for the task.

- v1.91
  - Fixes scaling not working properly for blueprints. Thanks Zeall!
  - Fixes "Repair anything" not fixing rocks properly.

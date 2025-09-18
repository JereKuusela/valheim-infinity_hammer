# Custom Hammer

Experimental feature.

New build hammer can be acquired with command `hammer_menu`.

For players without this mod, the hammer is just a regular hammer.

Each menu can be activated with a console command for an easy access.

- `hammer_menu`: Opens the main menu with access to other build tools and menus.
- `hammer_menu binds`: Shows key bindings and allows executing their commands.
- `hammer_menu blueprints`: Shows blueprints and allows selecting them.
  - By default blueprints are sorted by their folder. This can be changed in settings.
  - Sorting by blueprint category is slower because it requires reading the file contents.
  - Sorting by just name can be useful if the folder structure is not organized.
- `hammer_menu builds`: Shows regular build tools.
  - This button is not displayed in the main menu because each build tool is displayed separately.
  - `hammer_menu builds NAME` can be used to directly open a specific build tool.
- `hammer_menu locations`: Shows locations and allows selecting them.
- `hammer_menu objects`: Shows most objects sorted by name.
  - Objects starting with "fx_", "sfx_" or "vfx_" are not shown.
  - Each letter becomes own category.
  - You can hover the button to see some object names in that category.
- `hammer_menu rooms`: Shows dungeon rooms and allows selecting them from dungeon building.
- `hammer_menu sounds`: Shows objects starting with "sfx_".
- `hammer_menu tools`: Shows custom command tools and allows executing them.
- `hammer_menu types`: Shows objects sorted by their component types.
  - Each component type becomes own category.
  - Each object can be in multiple categories.
  - You can hover the button to see some object names in that category.
- `hammer_menu visuals`: Shows objects starting with "fx_" or "vfx_".

## Filtering

Some menus have huge amounts of categories and objects.

Additional categories are created when a category exceeds 90 items (default limit). A navigation layer is added if there are more than 5 categories (default limit).

These limits can be changed in settings, for example when using a mod that increases the build menu size.

Second parameter can be used to filter the results, which can prevent the need for navigation layers.

For example `hammer_menu types Tree` shows only component types that start with "Tree".

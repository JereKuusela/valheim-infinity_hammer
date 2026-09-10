# Deep North menu regression checks

Run `python tests/deepnorth/run.py --sdk /path/to/dotnet-sdk` from this repository.

These compile production menu source against small Unity/UI substitutes. They cover uncategorized tables, same-slot native selection, local/remote isolation, stable button identities, cleanup, menu navigation/toggling, and first-run tool loading. They do not execute Harmony, Unity rendering, input dispatch or multiplayer terrain synchronization.

The larger source handoff includes the inventory, preview and native assembly audits. Live terrain/outline placement still requires Valheim.

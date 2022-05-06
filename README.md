# valheim-infinity_hammer

This project is a creative building mod to game called Valheim.

To compile, you need to manually add correct libraries to the parent Libs folder. Check project file for needed ones. This is also where the output DLL will be copied.

- Download BepInEx for Valheim and use DLLs from core and unstripped_corlib folders.
- Valheim DLLs can be found from Valheim_Data/Manageed folder.
- DLLS which end with _publicized require using https://github.com/CabbageCrow/AssemblyPublicizer.

## Design

The whole object placement logic is rather convoluted as it is done in multiple steps. Following tiers are used:

- Ghost prefab: Overrides the selected build piece. This is needed to setup the placement ghost. This needs to be inactive at all times to prevent scripts from running.
- Placement ghost: The visual aid. No customization needed.
- Build piece: The ghost prefab isn't suitable for creating new objects because it can have a state. That's why default build pieces are used instead.

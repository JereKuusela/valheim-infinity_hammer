using System.ComponentModel;

namespace InfinityHammer;

public class ToolData
{
  [DefaultValue("")]
  public string name = "";
  [DefaultValue("")]
  public string description = "";
  [DefaultValue("")]
  public string icon = "";
  public string command = "";
  [DefaultValue("")]
  public string continuous = "";
  [DefaultValue("")]
  public string initialHeight = "";
  [DefaultValue("")]
  public string initialSize = "";
  [DefaultValue("")]
  public string snapGround = "";
  [DefaultValue("")]
  public string playerLevel = "";
  [DefaultValue(false)]
  public bool instant = false;
  [DefaultValue(null)]
  public int? tabIndex;
  [DefaultValue(null)]
  public int? index;
}

public class InitialData
{
  public static string Get()
  {
    return @"hammer:
- name: Pipette
  description: |-
    Press cmd mod1 to pick up.
    Press cmd mod2 to freeze.
  icon: hammer
  command: hammer pick=cmd_mod1 freeze=cmd_mod2
  index: 1
- name: Building pipette
  description: |-
    Select entire buildings.
    Press cmd mod1 to pick up.
    Press cmd mod2 to freeze.
  icon: hammer
  command: hammer connect pick=cmd_mod1 freeze=cmd_mod2
- name: Area pipette
  description: |-
    Select multiple objects.
    Press cmd mod1 to pick up.
    Press cmd mod2 to freeze.
  icon: hammer
  command: hammer_area pick=cmd_mod1 freeze=cmd_mod2
  initialHeight: 0
hoe:
- name: Level
  description: |-
    Flattens terrain.
    Hold cmd mod1 to smooth.
  icon: mud_road
  command: hammer_terrain level smooth=cmd_mod1?.5:0
  index: 10
- name: Raise
  description: |-
    Raises terrain.
    Hold cmd mod1 to smooth.
  icon: raise
  command: hammer_terrain raise=#h smooth=cmd_mod1?.5:0
  initialHeight: 0
- name: Pave
  description: |-
    Paves terrain.
    Hold cmd mod1 for single use.
  icon: paved_road
  command: hammer_terrain paint=paved
  continuous: -cmd_mod1
- name: Grass
  description: |-
    Grass.
    Hold cmd mod1 for single use.
  icon: replant
  command: hammer_terrain paint=grass
  continuous: -cmd_mod1
- name: Dirt
  description: |-
    Dirt.
    Hold cmd mod1 for single use.
  icon: Hoe
  command: hammer_terrain paint=dirt
  continuous: -cmd_mod1
- name: Cultivate
  description: |-
    Cultivates terrain.
    Hold cmd mod1 for single use.
  icon: cultivate
  command: hammer_terrain Area paint=cultivated
  continuous: -cmd_mod1
- name: DarkGrass
  description: |-
    Dark Grass.
    Hold cmd mod1 for single use.
  icon: trophyabomination
  command: hammer_terrain paint=grass_dark
  continuous: -cmd_mod1
- name: PatchyGrass
  description: |-
    Patchy Grass.
    Hold cmd mod1 for single use.
  icon: iron_wall_2x2
  command: hammer_terrain paint=patches
  continuous: -cmd_mod1
- name: MossyPaving
  description: |-
    Paving with moss.
    Hold cmd mod1 for single use.
  icon: trophygreydwarfshaman
  command: hammer_terrain paint=paved_moss
  continuous: -cmd_mod1
- name: DarkPaving
  description: |-
    Dark Paving.
    Hold cmd mod1 for single use.
  icon: tar
  command: hammer_terrain paint=paved_dark
  continuous: -cmd_mod1
- name: Reset
  description: |-
    Resets terrain.
    Hold cmd mod1 for single use.
  icon: Hoe
  command: hammer_terrain reset
  continuous: -cmd_mod1
- name: Slope
  description: Slope between you and aim point.
  icon: wood_wall_roof_45
  command: hammer_slope
- name: Remove
  description: |-
    Removes objects.
    Hold cmd mod1 to also reset the terrain.
  icon: softdeath
  command: hammer_object remove id=*;hammer_terrain keys=cmd_mod1 reset
- name: Tame
  description: |-
    Tames creatures.
    Hold cmd mod1 to untame
  icon: Carrot
  command: hammer_object tame keys=-cmd_mod1;hammer_object wild keys=cmd_mod1
";
  }
}

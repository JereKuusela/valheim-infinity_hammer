using System.ComponentModel;

namespace InfinityHammer;

public class ToolsData
{
  public ToolData[] hammer = new ToolData[0];
  public ToolData[] hoe = new ToolData[0];
}
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
  [DefaultValue(false)]
  public bool instant = false;
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
  command: hoe_terrain level smooth=cmd_mod1?.5:0
- name: Raise
  description: |-
    Raises terrain.
    Hold cmd mod1 to smooth.
  icon: raise
  command: hoe_terrain raise=#h smooth=cmd_mod1?.5:0
  initialHeight: 0
- name: Pave
  description: |-
    Paves terrain.
    Hold cmd mod1 for single use.
  icon: paved_road
  command: hoe_terrain paint=paved
  continuous: -cmd_mod1
- name: Grass
  description: |-
    Grass.
    Hold cmd mod1 for single use.
  icon: replant
  command: hoe_terrain paint=grass
  continuous: -cmd_mod1
- name: Dirt
  description: |-
    Dirt.
    Hold cmd mod1 for single use.
  icon: Hoe
  command: hoe_terrain paint=dirt
  continuous: -cmd_mod1
- name: Cultivate
  description: |-
    Cultivates terrain.
    Hold cmd mod1 for single use.
  icon: cultivate
  command: hoe_terrain Area paint=cultivated
  continuous: -cmd_mod1
- name: DarkGrass
  description: |-
    Dark Grass.
    Hold cmd mod1 for single use.
  icon: trophyabomination
  command: hoe_terrain paint=grass_dark
  continuous: -cmd_mod1
- name: PatchyGrass
  description: |-
    Patchy Grass.
    Hold cmd mod1 for single use.
  icon: iron_wall_2x2
  command: hoe_terrain paint=patches
  continuous: -cmd_mod1
- name: MossyPaving
  description: |-
    Paving with moss.
    Hold cmd mod1 for single use.
  icon: trophygreydwarfshaman
  command: hoe_terrain paint=paved_moss
  continuous: -cmd_mod1
- name: DarkPaving
  description: |-
    Dark Paving.
    Hold cmd mod1 for single use.
  icon: tar
  command: hoe_terrain paint=paved_dark
  continuous: -cmd_mod1
- name: Reset
  description: |-
    Resets terrain.
    Hold cmd mod1 for single use.
  icon: Hoe
  command: hoe_terrain reset
  continuous: -cmd_mod1
- name: Slope
  description: Slope between you and aim point.
  icon: wood_wall_roof_45
  command: hoe_slope
- name: Remove
  description: |-
    Removes objects.
    Hold cmd mod1 to also reset the terrain.
  icon: softdeath
  command: hoe_object remove id=*;hoe_terrain keys=cmd_mod1 reset
- name: Tame
  description: |-
    Tames creatures.
    Hold cmd mod1 to untame
  icon: Carrot
  command: hoe_object tame keys=-cmd_mod1;hoe_object wild keys=cmd_mod1
";
  }
}

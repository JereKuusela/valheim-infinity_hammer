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
  public string initialWidth = "";
  [DefaultValue("")]
  public string initialDepth = "";
  [DefaultValue("")]
  public string initialRadius = "";

  public void Execute()
  {

  }
}

public class InitialData
{
  private const string N = CommandParameters.CmdName;
  private const string D = CommandParameters.CmdDesc;
  private const string I = CommandParameters.CmdIcon;
  private const string CONT = CommandParameters.CmdContinuous + "=-" + M1;
  private const string M1 = CommandParameters.CmdMod1;
  private const string M2 = CommandParameters.CmdMod2;
  private const string K = "keys";
  private const string P = "paint";
  private const string HC = "hammer_command";
  private const string HT = "hoe_terrain";
  private const string HO = "hoe_object";
  private const string HS = "hoe_slope";
  private const string SMOOTH = $"smooth={M1}?.5:0";
  private const string INIITIAL_H = $"{CommandParameters.CmdH}=0";
  private const string SELECT = $"{HC} {I}=hammer {N}=Pipette {D}=Select_object. hammer";
  private const string PICK = $"{HC} {I}=hammer {N}=Pick {D}=Pick_object. hammer pick";
  private const string SELECT_DEVCOMMANDS = $"{HC} {I}=hammer {N}=Pipette {D}=Press_{M1}_to_pick_up.\nPress_{M2}_to_freeze. hammer pick={M1} freeze={M2}";
  private const string CONNECT_DEVCOMMANDS = $"{HC} {I}=hammer {N}=Building_pipette {D}=Select_entire_buildings.\nPress_{M1}_to_pick_up.\nPress_{M2}_to_freeze. hammer connect pick={M1} freeze={M2}";
  private const string AREA_SELECT = $"hammer_area {I}=hammer {N}=Area_pipette {INIITIAL_H} {D}=Select_multiple_objects.";
  private const string AREA_SELECT_DEVCOMMANDS = $"hammer_area {I}=hammer {N}=Area_pipette {INIITIAL_H} {D}=Select_multiple_objects.\nPress_{M1}_to_pick_up.\nPress_{M2}_to_freeze. pick={M1} freeze={M2}";
  public static string[] Hammer()
  {
    if (CommandWrapper.ServerDevcommands == null)
      return new[] { SELECT, PICK, AREA_SELECT };
    return new[] { SELECT_DEVCOMMANDS, CONNECT_DEVCOMMANDS, AREA_SELECT_DEVCOMMANDS };
  }
  public static string[] Hoe()
  {
    if (CommandWrapper.WorldEditCommands == null)
      return new string[0];
    return new[] {
      $"{HT} {I}=mud_road {N}=Level {D}=Flattens_terrain.\nHold_{M1}_to_smooth. level {SMOOTH}",
      $"{HT} {I}=raise {N}=Raise {INIITIAL_H} {D}=Raises_terrain.\nHold_{M1}_to_smooth. raise=h {SMOOTH}",
      $"{HT} {I}=paved_road {N}=Pave {D}=Paves_terrain.\nHold_{M1}_for_single_use. {P}=paved {CONT}",
      $"{HT} {I}=replant {N}=Grass {D}=Grass.\nHold_{M1}_for_single_use. {P}=grass {CONT}",
      $"{HT} {I}=Hoe {N}=Dirt {D}=Dirt.\nHold_{M1}_for_single_use. {P}=dirt {CONT}",
      $"{HT} {I}=cultivate {N}=Cultivate Area {D}=Cultivates_terrain.\nHold_{M1}_for_single_use. {P}=cultivated {CONT}",
      $"{HT} {I}=trophyabomination {N}=DarkGrass {D}=Dark_Grass.\nHold_{M1}_for_single_use. {P}=grass_dark {CONT}",
      $"{HT} {I}=iron_wall_2x2 {N}=PatchyGrass {D}=Patchy_Grass.\nHold_{M1}_for_single_use. {P}=patches {CONT}",
      $"{HT} {I}=trophygreydwarfshaman {N}=MossyPaving {D}=Paving_with_moss.\nHold_{M1}_for_single_use. {P}=paved_moss {CONT}",
      $"{HT} {I}=tar {N}=DarkPaving {D}=Dark_Paving.\nHold_{M1}_for_single_use. {P}=paved_dark {CONT}",
      $"{HT} {I}=Hoe {N}=Reset {D}=Resets_terrain.\nHold_{M1}_for_single_use. reset {CONT}",
      $"{HS} {I}=wood_wall_roof_45 {N}=Slope {D}=Slope_between_you_and_aim_point.",
      $"{HO} {I}=softdeath {N}=Remove {D}=Removes_objects.\nHold_{M1}_to_also_reset_the_terrain. remove id=*;{HT} keys={M1} reset",
      $"{HO} {I}=Carrot {N}=Tame {D}=Tames_creatures.\nHold_{M1}_to_untame tame {K}=-{M1};{HO} wild {K}={M1}",
    };
  }
}
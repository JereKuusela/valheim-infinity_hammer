using System.ComponentModel;

namespace InfinityHammer;

public class CommandsData {
  public CommandData[] hammer = new CommandData[0];
  public CommandData[] hoe = new CommandData[0];
}
public class CommandData {
  [DefaultValue("")]
  public string name = "";
  [DefaultValue("")]
  public string description = "";
  [DefaultValue("")]
  public string icon = "";
  public string command = "";
  [DefaultValue(true)]
  public bool prepend_tool_name = true;
}

public class InitialData {
  private const string SELECT = "hammer_command cmd_icon=hammer cmd_name=Pipette cmd_desc=Select_object. hammer";
  private const string PICK = "hammer_command cmd_icon=hammer cmd_name=Pick cmd_desc=Pick_object. hammer pick";
  private const string SELECT_DEVCOMMANDS = "hammer_command cmd_icon=hammer cmd_name=Pipette cmd_desc=Press_cmd_mod1_to_pick_up.\nPress_cmd_mod2_to_freeze. hammer pick=cmd_mod1 freeze=cmd_mod2";
  private const string CONNECT_DEVCOMMANDS = "hammer_command cmd_icon=hammer cmd_name=Building_pipette cmd_desc=Select_entire_buildings.\nPress_cmd_mod1_to_pick_up.\nPress_cmd_mod2_to_freeze. hammer connect pick=cmd_mod1 freeze=cmd_mod2";
  private const string AREA_SELECT = "hammer_area cmd_icon=hammer cmd_name=Area_pipette cmd_desc=Select_multiple_objects.";
  private const string AREA_SELECT_DEVCOMMANDS = "hammer_area cmd_icon=hammer cmd_name=Area_pipette cmd_desc=Select_multiple_objects.\nPress_cmd_mod1_to_pick_up.\nPress_cmd_mod2_to_freeze. pick=cmd_mod1 freeze=cmd_mod2";
  public static string[] Hammer() {
    if (CommandWrapper.ServerDevcommands == null)
      return new[] { SELECT, PICK, AREA_SELECT };
    return new[] { SELECT_DEVCOMMANDS, CONNECT_DEVCOMMANDS, AREA_SELECT_DEVCOMMANDS };
  }
  public static string[] Hoe() {
    if (CommandWrapper.WorldEditCommands == null)
      return new string[0];
    return new[] {
      "hoe_terrain cmd_icon=mud_road cmd_name=Level cmd_desc=Flattens_terrain. level",
      "hoe_terrain cmd_icon=mud_road cmd_name=Smooth_level cmd_desc=Smoothly_flattens_terrain. level smooth=.5",
      "hoe_terrain cmd_icon=raise cmd_name=Raise cmd_desc=Raises_terrain. raise=h",
      "hoe_terrain cmd_icon=paved_road cmd_name=Pave cmd_desc=Paves_terrain. paint=paved",
      "hoe_terrain cmd_icon=replant cmd_name=Grass cmd_desc=Grass. paint=grass",
      "hoe_terrain cmd_icon=Hoe cmd_name=Dirt cmd_desc=Dirt. paint=dirt",
      "hoe_terrain cmd_icon=cultivate cmd_name=Cultivate Area cmd_desc=Cultivates_terrain. paint=cultivated",
      "hoe_terrain cmd_icon=trophyabomination cmd_name=DarkGrass cmd_desc=Dark_Grass. paint=grass_dark",
      "hoe_terrain cmd_icon=iron_wall_2x2 cmd_name=PatchyGrass cmd_desc=Patchy_Grass. paint=patches",
      "hoe_terrain cmd_icon=trophygreydwarfshaman cmd_name=MossyPaving cmd_desc=Paving_with_moss. paint=paved_moss",
      "hoe_terrain cmd_icon=tar cmd_name=DarkPaving cmd_desc=Dark_Paving. paint=paved_dark",
      "hoe_terrain cmd_icon=Hoe cmd_name=Reset cmd_desc=Resets_terrain. reset",
      "hoe_slope cmd_icon=wood_wall_roof_45 cmd_name=Slope cmd_desc=Smooth_slope_between_you_and_aim_point.",
      "hoe_object cmd_icon=softdeath cmd_name=Remove cmd_desc=Removes_objects.\nPress_cmd_mod1_to_also_reset_the_terrain. remove id=*;hoe_terrain keys=cmd_mod1 reset",
      "hoe_object cmd_icon=Carrot cmd_name=Tame cmd_desc=Tames_creatures. tame",
    };
  }
}
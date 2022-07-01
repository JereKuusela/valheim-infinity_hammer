using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Service;
namespace InfinityHammer;

public partial class Configuration {
#nullable disable
  public static ConfigEntry<string> configCustomBinds;
  public static string CustomBinds => configCustomBinds.Value;
  public static ConfigEntry<string> configHammerCommands;
  public static List<string> HammerCommands = new();
  public static ConfigEntry<string> configHoeCommands;
  public static List<string> HoeCommands = new();
  public static ConfigEntry<int> configHammerMenuTab;
  public static int HammerMenuTab => configHammerMenuTab.Value;
  public static ConfigEntry<int> configHammerMenuIndex;
  public static int HammerMenuIndex => configHammerMenuIndex.Value;
  public static ConfigEntry<int> configHoeMenuTab;
  public static int HoeMenuTab => configHoeMenuTab.Value;
  public static ConfigEntry<int> configHoeMenuIndex;
  public static int HoeMenuIndex => configHoeMenuIndex.Value;
  public static ConfigEntry<string> configCommandDefaultSize;
  public static float CommandDefaultSize => ConfigWrapper.TryParseFloat(configCommandDefaultSize);
#nullable enable
  public static void AddCommand(Tool tool, string command) {
    GetCommands(tool).Add(command);
    UpdateCommand(tool);
  }
  public static void RemoveCommand(Tool tool, int index) {
    GetCommands(tool).RemoveAt(index);
    UpdateCommand(tool);
  }
  public static int RemoveCommand(Tool tool, string command) {
    var removed = GetCommands(tool).RemoveAll(cmd => cmd.StartsWith(command, StringComparison.OrdinalIgnoreCase));
    UpdateCommand(tool);
    return removed;
  }
  public static string GetCommand(Tool tool, int index) {
    if (index < 0 || GetCommands(tool).Count <= index) throw new InvalidOperationException($"No command at index {index}.");
    return GetCommands(tool)[index];
  }
  public static List<string> GetCommands(Tool tool) => tool == Tool.Hammer ? HammerCommands : HoeCommands;
  private static void UpdateCommand(Tool tool) {
    if (tool == Tool.Hammer)
      configHammerCommands.Value = string.Join("|", HammerCommands);
    if (tool == Tool.Hoe)
      configHoeCommands.Value = string.Join("|", HoeCommands);
  }
  private static List<string> ParseCommands(string value) => value.Split('|').Select(s => s.Trim()).ToList();

  private static void InitCommands(ConfigWrapper wrapper) {
    var defaultHammerCommands = new[] {
      "hammer_command cmd_icon=hammer cmd_name=Pipette cmd_desc=Shift_to_select_entire_buildings. hammer keys=-leftshift;hammer keys=leftshift connect",
      "hammer_command cmd_icon=hammer cmd_name=Area_pipette cmd_desc=Select_multiple_objects. hammer r from=x,z,y",
    };
    var defaultHoeCommands = new[] {
      "hoe_terrain cmd_icon=mud_road cmd_name=Level cmd_desc=Flattens_terrain. circle=r rect=w,d level",
      "hoe_terrain cmd_icon=raise cmd_name=Raise cmd_desc=Raises_terrain. circle=r rect=w,d raise=h",
      "hoe_terrain cmd_icon=paved_road cmd_name=Pave cmd_desc=Paves_terrain. circle=r rect=w,d paint=paved",
      "hoe_terrain cmd_icon=replan cmd_name=Grass cmd_desc=Grass. circle=r rect=w,d paint=grass",
      "hoe_terrain cmd_icon=KnifeBlackMetal cmd_name=Reset cmd_desc=Resets_terrain. circle=r rect=w,d reset",
      "hoe_object cmd_icon=softdeath cmd_name=Remove cmd_desc=Removes_objects.\nPress_shift_to_also_reset_the_terrain. radius=r remove id=*;terrain keys=leftshift circle=r from=x,z,y reset",
      "hoe_object cmd_icon=Burning cmd_name=Tame cmd_desc=Tames_creatures. radius=r tame",
    };
    var section = "6. Commands";
    configCustomBinds = wrapper.BindList(section, "Custom binds", "", "Binds separated by ; that are set on the game start.");
    configCommandDefaultSize = wrapper.Bind(section, "Command default size", "10", "Default size for commands.");
    configCommandDefaultSize.SettingChanged += (s, e) => {
      Scaling.Command.SetScaleX(CommandDefaultSize);
      Scaling.Command.SetScaleZ(CommandDefaultSize);
    };
    Scaling.Command.SetScaleX(CommandDefaultSize);
    Scaling.Command.SetScaleZ(CommandDefaultSize);

    configHammerCommands = wrapper.Bind(section, "Hammer commands", string.Join("|", defaultHammerCommands), "Available commands.");
    configHammerCommands.SettingChanged += (s, e) => HammerCommands = ParseCommands(configHammerCommands.Value);
    HammerCommands = ParseCommands(configHammerCommands.Value);
    configHammerMenuTab = wrapper.Bind(section, "Hammer menu tab", 0, "Index of the menu tab.");
    configHammerMenuIndex = wrapper.Bind(section, "Hammer menu index", 1, "Index on the menu.");
    configHoeCommands = wrapper.Bind(section, "Hoe commands", string.Join("|", defaultHoeCommands), "Available commands.");
    configHoeCommands.SettingChanged += (s, e) => HoeCommands = ParseCommands(configHoeCommands.Value);
    HoeCommands = ParseCommands(configHoeCommands.Value);
    configHoeMenuTab = wrapper.Bind(section, "Hoe menu tab", 0, "Index of the menu tab.");
    configHoeMenuIndex = wrapper.Bind(section, "Hoe menu index", 5, "Index on the menu.");
  }
}
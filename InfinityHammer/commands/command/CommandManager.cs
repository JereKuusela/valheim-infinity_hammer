using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
namespace InfinityHammer;

[HarmonyPatch]
public class CommandManager {
  public static string FileName = "infinity_hammer.yaml";
  public static string FilePath = Path.Combine(Paths.ConfigPath, FileName);
  public static string Pattern = "infinity_hammer*.yaml";
  private static Dictionary<string, GameObject> Prefabs = new();

  public static string FromData(CommandData data, Tool tool) {
    var command = data.command;
    var toolStr = tool == Tool.Hammer ? "hammer_" : "hoe_";
    if (data.prepend_tool_name)
      command = String.Join(";", command.Split(';').Select(cmd => toolStr + cmd));
    if (data.name != "")
      command += $" cmd_name={data.name.Replace(" ", "_")}";
    if (data.description != "")
      command += $" cmd_desc={data.description.Replace(" ", "_")}";
    if (data.icon != "")
      command += $" cmd_icon={data.icon}";
    return command;
  }
  public static CommandData ToData(string command) {
    CommandParameters pars = new(command, false, false);
    CommandData data = new();
    data.command = pars.Command;
    data.prepend_tool_name = false;
    var cmds = data.command.Split(';');
    if (cmds.All(cmd => cmd.StartsWith("hammer_", StringComparison.OrdinalIgnoreCase))) {
      data.prepend_tool_name = true;
      data.command = String.Join(";", cmds.Select(cmd => cmd.Substring(7)));
    }
    if (cmds.All(cmd => cmd.StartsWith("hoe_", StringComparison.OrdinalIgnoreCase))) {
      data.prepend_tool_name = true;
      data.command = String.Join(";", cmds.Select(cmd => cmd.Substring(4)));
    }
    data.description = pars.Description;
    data.name = pars.Name;
    data.icon = pars.IconName;
    return data;
  }
  public static void CreateFile() {
    if (File.Exists(FilePath)) return;
    InitialData initial = new();
    var hammerData = InitialData.Hammer().Select(ToData).ToArray();
    var hoeData = InitialData.Hoe().Select(ToData).ToArray();
    CommandsData data = new() { hammer = hammerData, hoe = hoeData };
    var yaml = Data.Serializer().Serialize(data);
    File.WriteAllText(FilePath, yaml);
  }
  public static void ToFile() {
    var hammerData = HammerCommands.Select(ToData).ToArray();
    var hoeData = HoeCommands.Select(ToData).ToArray();
    CommandsData data = new() { hammer = hammerData, hoe = hoeData };
    var yaml = Data.Serializer().Serialize(data);
    File.WriteAllText(FilePath, yaml);
  }
  public static List<string> Get(Tool tool) => tool == Tool.Hammer ? HammerCommands : HoeCommands;

  public static void Add(Tool tool, string command) {
    Get(tool).Add(command);
    CommandManager.ToFile();
  }
  public static void Remove(Tool tool, int index) {
    Get(tool).RemoveAt(index);
    CommandManager.ToFile();
  }
  public static int Remove(Tool tool, string command) {
    var removed = Get(tool).RemoveAll(cmd => cmd.StartsWith(command, StringComparison.OrdinalIgnoreCase));
    CommandManager.ToFile();
    return removed;
  }
  public static string Get(Tool tool, int index) {
    if (index < 0 || Get(tool).Count <= index) throw new InvalidOperationException($"No command at index {index}.");
    return Get(tool)[index];
  }
  public static List<string> HammerCommands = new();
  public static List<string> HoeCommands = new();
  public static void FromFile() {
    var yaml = Data.Read(Pattern);
    if (yaml == "") return;
    try {
      var data = Data.Deserialize<CommandsData>(yaml, FileName);
      var count = data.hammer.Length + data.hoe.Length;
      if (count == 0) {
        InfinityHammer.Log.LogWarning($"Failed to load any command data.");
        return;
      }
      HammerCommands = data.hammer.Select(cmd => FromData(cmd, Tool.Hammer)).ToList();
      HoeCommands = data.hoe.Select(cmd => FromData(cmd, Tool.Hoe)).ToList();
      InfinityHammer.Log.LogInfo($"Reloading {count} command data.");
    } catch (Exception e) {
      InfinityHammer.Log.LogError(e.StackTrace);
    }
  }
  public static void SetupWatcher() {
    Data.SetupWatcher(Pattern, FromFile);
  }
}

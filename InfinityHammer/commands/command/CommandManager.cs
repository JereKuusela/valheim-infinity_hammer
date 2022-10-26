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
    if (data.name != "")
      command += $" {CommandParameters.CmdName}={data.name.Replace(" ", "_")}";
    if (data.description != "")
      command += $" {CommandParameters.CmdDesc}={data.description.Replace(" ", "_")}";
    if (data.icon != "")
      command += $" {CommandParameters.CmdIcon}={data.icon}";
    if (data.initialDepth != "")
      command += $" {CommandParameters.CmdD}={data.initialDepth}";
    if (data.initialHeight != "")
      command += $" {CommandParameters.CmdH}={data.initialHeight}";
    if (data.initialRadius != "")
      command += $" {CommandParameters.CmdR}={data.initialRadius}";
    if (data.initialWidth != "")
      command += $" {CommandParameters.CmdW}={data.initialWidth}";
    if (data.continuous != "") {
      if (data.continuous == "true")
        command += $" {CommandParameters.CmdContinuous}";
      else
        command += $" {CommandParameters.CmdContinuous}={data.continuous}";
    }
    return command;
  }
  public static CommandData ToData(string command) {
    CommandParameters pars = new(command, false, false);
    CommandData data = new();
    data.command = pars.Command;
    data.description = pars.Description;
    data.name = pars.Name;
    data.icon = pars.IconValue;
    data.continuous = pars.Continuous;
    data.initialDepth = pars.DepthCap.ToString();
    data.initialHeight = pars.HeightCap.ToString();
    data.initialRadius = pars.RadiusCap.ToString();
    data.initialWidth = pars.WidthCap.ToString();
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
    ToFile();
  }
  public static void Remove(Tool tool, int index) {
    Get(tool).RemoveAt(index);
    ToFile();
  }
  public static int Remove(Tool tool, string command) {
    var removed = Get(tool).RemoveAll(cmd => cmd.StartsWith(command, StringComparison.OrdinalIgnoreCase));
    ToFile();
    return removed;
  }
  public static string Get(Tool tool, int index) {
    if (index < 0 || Get(tool).Count <= index) throw new InvalidOperationException($"No command at index {index}.");
    return Get(tool)[index];
  }
  public static List<string> HammerCommands = new();
  public static List<string> HoeCommands = new();
  public static void FromFile() {
    var data = Data.Read(Pattern, Data.Deserialize<CommandsData>);
    if (data.Length == 0) {
      CreateFile();
      return;
    }
    try {
      var hammer = data.SelectMany(value => value.hammer).ToArray();
      var hoe = data.SelectMany(value => value.hoe).ToArray();
      var count = hammer.Length + hoe.Length;
      if (count == 0) {
        InfinityHammer.Log.LogWarning($"Failed to load any command data.");
        return;
      }
      HammerCommands = hammer.Select(cmd => FromData(cmd, Tool.Hammer)).ToList();
      HoeCommands = hoe.Select(cmd => FromData(cmd, Tool.Hoe)).ToList();
      InfinityHammer.Log.LogInfo($"Reloading {count} command data.");
      Player.m_localPlayer?.UpdateAvailablePiecesList();
    } catch (Exception e) {
      InfinityHammer.Log.LogError(e.StackTrace);
    }
  }
  public static void SetupWatcher() {
    Data.SetupWatcher(Pattern, FromFile);
  }
}

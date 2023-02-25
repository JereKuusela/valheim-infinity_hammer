using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
namespace InfinityHammer;

[HarmonyPatch]
public class CommandManager
{
  public static string FileName = "infinity_hammer.yaml";
  public static string FilePath = Path.Combine(Paths.ConfigPath, FileName);
  public static string Pattern = "infinity_hammer*.yaml";
  private static Dictionary<string, GameObject> Prefabs = new();

  public static string FromData(ToolData data, Equipment tool)
  {
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
    if (data.continuous != "")
    {
      if (data.continuous == "true")
        command += $" {CommandParameters.CmdContinuous}";
      else
        command += $" {CommandParameters.CmdContinuous}={data.continuous}";
    }
    return command;
  }
  public static ToolData ToData(string command)
  {
    CommandParameters pars = new(command, false, false);
    ToolData data = new();
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
  public static void CreateFile()
  {
    if (File.Exists(FilePath)) return;
    InitialData initial = new();
    var hammerData = InitialData.Hammer().Select(ToData).ToArray();
    var hoeData = InitialData.Hoe().Select(ToData).ToArray();
    ToolsData data = new() { hammer = hammerData, hoe = hoeData };
    var yaml = Data.Serializer().Serialize(data);
    File.WriteAllText(FilePath, yaml);
  }
  public static void ToFile()
  {
    var hammerData = HammerTools.ToArray();
    var hoeData = HoeTools.ToArray();
    ToolsData data = new() { hammer = hammerData, hoe = hoeData };
    var yaml = Data.Serializer().Serialize(data);
    File.WriteAllText(FilePath, yaml);
  }
  public static List<ToolData> Get(Equipment tool) => tool == Equipment.Hammer ? HammerTools : HoeTools;

  public static void Import(Equipment tool, string command)
  {
    var yaml = command.Replace("\\n", "\n");
    var cmd = Data.Deserialize<ToolData>(yaml, "Import");
    Get(tool).Add(cmd);
    ToFile();
  }
  public static void Add(Equipment equipment, string command)
  {
    ToolData tool = new();
    tool.name = command.Substring(0, 10);
    tool.command = command;
    Get(equipment).Add(tool);
    ToFile();
  }
  public static void Remove(Equipment equipment, int index)
  {
    Get(equipment).RemoveAt(index);
    ToFile();
  }
  public static int Remove(Equipment equipment, string command)
  {
    var removed = Get(equipment).RemoveAll(tool => tool.command.StartsWith(command, StringComparison.OrdinalIgnoreCase));
    ToFile();
    return removed;
  }
  public static ToolData Get(Equipment equipment, int index)
  {
    if (index < 0 || Get(equipment).Count <= index) throw new InvalidOperationException($"No command at index {index}.");
    return Get(equipment)[index];
  }

  public static string Export(Equipment equipment, int index)
  {
    var tool = Get(equipment, index);
    var yaml = Data.Serializer().Serialize(tool).Replace("\n", "\\n");
    return yaml;
  }
  public static List<ToolData> HammerTools = new();
  public static List<ToolData> HoeTools = new();
  public static void FromFile()
  {
    var tools = Data.Read(Pattern, Data.Deserialize<ToolsData>);
    if (tools.Length == 0)
    {
      CreateFile();
      return;
    }
    try
    {
      var hammer = tools.SelectMany(value => value.hammer).ToList();
      var hoe = tools.SelectMany(value => value.hoe).ToList();
      var count = hammer.Count + hoe.Count;
      if (count == 0)
      {
        InfinityHammer.Log.LogWarning($"Failed to load any tools.");
        return;
      }
      HammerTools = hammer;
      HoeTools = hoe;
      InfinityHammer.Log.LogInfo($"Reloading {count} tools.");
      Player.m_localPlayer?.UpdateAvailablePiecesList();
    }
    catch (Exception e)
    {
      InfinityHammer.Log.LogError(e.StackTrace);
    }
  }
  public static void SetupWatcher()
  {
    Data.SetupWatcher(Pattern, FromFile);
  }
}

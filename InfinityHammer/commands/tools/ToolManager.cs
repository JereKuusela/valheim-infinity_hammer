using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
namespace InfinityHammer;

[HarmonyPatch]
public class ToolManager
{
  public const string CmdMod1 = "cmd_mod1";
  public const string CmdMod2 = "cmd_mod2";
  public static string FileName = "infinity_hammer.yaml";
  public static string FilePath = Path.Combine(Paths.ConfigPath, FileName);
  public static string Pattern = "infinity_hammer*.yaml";
  private static Dictionary<string, GameObject> Prefabs = new();


  public static void CreateFile()
  {
    if (File.Exists(FilePath)) return;
    File.WriteAllText(FilePath, InitialData.Get());
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
  public static bool TryGetTool(Equipment equipment, string name, out ToolData tool)
  {
    tool = null!;
    if (equipment == Equipment.Hammer)
      tool = HammerTools.FirstOrDefault(tool => tool.name == name);
    else
      tool = HoeTools.FirstOrDefault(tool => tool.name == name);
    return tool != null;
  }
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

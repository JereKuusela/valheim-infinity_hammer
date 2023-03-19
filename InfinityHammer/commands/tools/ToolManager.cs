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
    var yaml = Data.Serializer().Serialize(Tools);
    File.WriteAllText(FilePath, yaml);
  }

  public static void Import(string equipment, string tool)
  {
    var yaml = tool.Replace("\\n", "\n");
    var data = Data.Deserialize<ToolData>(yaml, "Import");
    Add(equipment, data);
  }
  public static void Add(string equipment, ToolData tool)
  {
    if (!Tools.TryGetValue(equipment, out var tools))
      Tools.Add(equipment, new());
    Tools[equipment].Add(tool);
    ToFile();
  }
  public static string Export(string equipment, string name)
  {
    if (!TryGetTool(equipment, name, out var tool))
      return "";
    var yaml = Data.Serializer().Serialize(tool).Replace("\n", "\\n");
    return yaml;
  }
  public static Dictionary<string, List<ToolData>> Tools = new();
  public static bool TryGetTool(string equipment, string name, out ToolData tool)
  {
    tool = null!;
    if (Tools.TryGetValue(equipment, out var tools))
      tool = tools.FirstOrDefault(tool => tool.name == name);
    if (tool == null)
      tool = Tools.Values.SelectMany(tool => tool).FirstOrDefault(tool => tool.name == name);
    return tool != null;
  }
  public static List<ToolData> Get(string equipment)
  {
    if (Tools.TryGetValue(equipment, out var tools))
      return tools;
    return new();
  }
  public static void FromFile()
  {
    var tools = Data.Read(Pattern, Data.Deserialize<Dictionary<string, ToolData[]>>);
    if (tools.Count == 0)
    {
      CreateFile();
      return;
    }
    try
    {
      var count = tools.Values.SelectMany(x => x).Count();
      if (count == 0)
      {
        InfinityHammer.Log.LogWarning($"Failed to load any tools.");
        return;
      }
      Tools = tools.ToDictionary(kvp => kvp.Key.ToLower(), kvp => kvp.Value);
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

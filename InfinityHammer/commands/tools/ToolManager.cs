using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
namespace InfinityHammer;

[HarmonyPatch]
public class ToolManager
{
  public const string CmdMod1 = "cmd_mod1";
  public const string CmdMod2 = "cmd_mod2";
  public static string FileName = "infinity_hammer.yaml";
  public static string FilePath = Path.Combine(Paths.ConfigPath, FileName);
  public static string Pattern = "infinity_hammer*.yaml";


  public static void CreateFile()
  {
    if (File.Exists(FilePath)) return;
    File.WriteAllText(FilePath, InitialData.Get());
  }
  public static void ToFile()
  {
    var yaml = Yaml.Serializer().Serialize(ToolData);
    File.WriteAllText(FilePath, yaml);
  }

  public static void Import(string equipment, string tool)
  {
    var yaml = tool.Replace("\\n", "\n");
    var data = Yaml.Deserialize<ToolData>(yaml, "Import");
    Add(equipment, data);
  }
  public static void Add(string equipment, ToolData tool)
  {
    if (!ToolData.ContainsKey(equipment))
      ToolData.Add(equipment, []);
    ToolData[equipment].Add(tool);
    ToFile();
  }
  public static string Export(string equipment, string name)
  {
    if (!TryGetTool(equipment, name, out var tool))
      return "";
    var yaml = Yaml.Serializer().Serialize(tool).Replace("\n", "\\n");
    return yaml;
  }
  private static Dictionary<string, List<ToolData>> ToolData = [];
  public static Dictionary<string, List<Tool>> Tools = [];
  public static bool TryGetTool(string equipment, string name, out Tool tool)
  {
    tool = null!;
    if (Tools.TryGetValue(equipment, out var tools))
      tool = tools.FirstOrDefault(tool => tool.Name == name);
    if (tool == null)
      tool = Tools.Values.SelectMany(tool => tool).FirstOrDefault(tool => tool.Name == name);
    return tool != null;
  }
  public static List<Tool> Get(string equipment) => Tools.TryGetValue(equipment, out var tools) ? tools : [];
  public static void FromFile()
  {
    var tools = Yaml.Read(Pattern, Yaml.Deserialize<Dictionary<string, ToolData[]>>);
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
      ToolData = tools.ToDictionary(kvp => kvp.Key.ToLower(), kvp => kvp.Value);
      Tools = tools.ToDictionary(kvp => kvp.Key.ToLower(), kvp => kvp.Value.Select(s => new Tool(s)).ToList());
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
    Yaml.SetupWatcher(Pattern, FromFile);
  }
}
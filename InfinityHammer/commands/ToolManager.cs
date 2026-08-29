using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using ServerDevcommands;
using Service;
namespace InfinityTools;

[HarmonyPatch]
public class ToolManager
{
  public const string CmdMod1 = "<mod1>";
  public const string CmdMod2 = "<mod2>";
  public const string CmdAlt = "<alt>";
  public static string DefaultFile = Path.Combine(Paths.ConfigPath, "infinity_tools.yaml");
  public static string Folder = "tools";
  public static string Pattern = "infinity_tools*.yaml";

  public static void Initialize()
  {
    CreateAlias();
    CreateFile();
    SetupWatcher();
    FromFile();
  }

  private static void CreateAlias()
  {
    var pars = "from=<x>,<z>,<y> circle=<r>-<r2> angle=<a> rect=<w>-<w2>,<d>";
    var parsSpawn = "from=<x>,<z>,<y> radius=<r>-<r2>";
    var parsTo = "to=<x>,<z>,<y> circle=<r>-<r2> rect=<w>-<w2>,<d>";
    var sub = ServerDevcommands.Settings.Substitution;
    AliasManager.AddAlias("tool_terrain", $"terrain {pars}");
    AliasManager.AddAlias("t_t", "tool tool_terrain");
    AliasManager.AddAlias("tool_object", $"object {pars} height=<h> ignore=<ignore> id=<include>");
    AliasManager.AddAlias("t_o", "tool tool_object");
    AliasManager.AddAlias("tool_spawn", $"spawn_object {sub} {parsSpawn}");
    AliasManager.AddAlias("t_s", "tool tool_spawn");
    AliasManager.AddAlias("tool_terrain_to", $"terrain {parsTo}");
    // Bit pointless but kept for legacy.
    AliasManager.AddAlias("tool_slope", "tool_terrain_to slope");
    AliasManager.AddAlias("tool_area", $"hammer {pars} height=<h> ignore=<ignore> id=<include>");
  }


  public static void CreateFile()
  {
    if (File.Exists(DefaultFile)) return;
    File.WriteAllText(DefaultFile, InitialData.Get());
  }
  public static void ToFile()
  {
    var yaml = Yaml.Serializer().Serialize(ToolData);
    File.WriteAllText(DefaultFile, yaml);
  }

  public static ToolData Import(string equipment, string tool)
  {
    var yaml = tool.Replace("\\n", "\n");
    var data = Yaml.Deserialize<ToolData>(yaml, "Import");
    Add(equipment, data);
    return data;
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
    if (!TryGetToolData(equipment, name, out var tool))
    {
      if (!TryGetToolData(name, out tool, out equipment))
        return "";
    }
    var yaml = Yaml.Serializer().Serialize(tool).Replace("\r\n", "\\n").Replace("\n", "\\n");
    return $"tool_import {equipment} {yaml}";
  }
  private static Dictionary<string, List<ToolData>> ToolData = [];
  public static Dictionary<string, List<Tool>> Tools = [];
  public static bool TryGetTool(string equipment, string name, out Tool tool)
  {
    tool = null!;
    if (Tools.TryGetValue(equipment, out var tools))
      tool = tools.FirstOrDefault(tool => string.Equals(tool.Name, name, StringComparison.OrdinalIgnoreCase));
    if (tool == null)
      tool = Tools.Values.SelectMany(tool => tool).FirstOrDefault(tool => string.Equals(tool.Name, name, StringComparison.OrdinalIgnoreCase));
    return tool != null;
  }
  public static bool TryGetToolData(string equipment, string name, out ToolData tool)
  {
    tool = null!;
    if (ToolData.TryGetValue(equipment, out var tools))
    {
      tool = tools.FirstOrDefault(tool => string.Equals(tool.name, name, StringComparison.OrdinalIgnoreCase));
      return tool != null;
    }
    return tool != null;
  }
  public static bool TryGetToolData(string name, out ToolData tool, out string equipment)
  {
    tool = null!;
    equipment = "";
    foreach (var kvp in ToolData)
    {
      foreach (var toolData in kvp.Value)
      {
        if (string.Equals(toolData.name, name, StringComparison.OrdinalIgnoreCase))
        {
          tool = toolData;
          equipment = kvp.Key;
          return true;
        }
      }
    }
    return false;
  }
  public static List<Tool> Get(string equipment) => Tools.TryGetValue(equipment, out var tools) ? tools : [];
  public static List<Tool> GetAll() => Tools.SelectMany(kvp => kvp.Value).ToList();
  public static void FromFile()
  {
    ToolData.Clear();
    Tools.Clear();
    if (!File.Exists(DefaultFile))
    {
      CreateFile();
      return;
    }
    Yaml.LoadDictFromDirectory<List<ToolData>>(Paths.ConfigPath, Pattern, Folder, LoadTool);
    if (ToolData.Count == 0)
    {
      Log.Warning($"Failed to load any tools.");
      return;
    }
    Tools = ToolData.ToDictionary(kvp => kvp.Key.ToLower(), kvp => kvp.Value.Select(s => new Tool(s)).ToList());
    Log.Info($"Reloading {ToolData.Values.SelectMany(x => x).Count()} tools.");
    Player.m_localPlayer?.UpdateAvailablePiecesList();
  }

  private static void LoadTool(string file, string equipment, List<ToolData> tools)
  {
    if (!ToolData.ContainsKey(equipment))
      ToolData.Add(equipment, []);
    ToolData[equipment].AddRange(tools);
  }

  public static void SetupWatcher()
  {
    Yaml.SetupWatcher(Paths.ConfigPath, Pattern, Folder, FromFile);
  }
}
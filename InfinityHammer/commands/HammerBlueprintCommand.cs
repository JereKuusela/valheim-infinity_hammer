using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
namespace InfinityHammer;
public class BlueprintObject {
  public string Prefab = "";
  public Vector3 Pos;
  public Quaternion Rot;
  public BlueprintObject(string name, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW) {
    Prefab = name;
    Pos = new Vector3(posX, posY, posZ);
    Rot = new Quaternion(rotX, rotY, rotZ, rotW).normalized;
  }
}
public class Blueprint {
  public string Name = "";
  public string Description = "";
  public BlueprintObject[] Objects = new BlueprintObject[0];
}
public class HammerBlueprintCommand {
  private static void PrintSelected(Terminal terminal, Blueprint blueprint) {
    if (Settings.DisableSelectMessages) return;
    Helper.AddMessage(terminal, $"Selected {blueprint.Name}.");
  }
  private static GameObject BuildObject(Blueprint blueprint) {
    var container = new GameObject();
    // Prevents children from disappearing.
    container.SetActive(false);
    container.name = blueprint.Name;
    var piece = container.AddComponent<Piece>();
    piece.m_name = blueprint.Name;
    piece.m_description = blueprint.Description;
    ZNetView.m_forceDisableInit = true;
    foreach (var item in blueprint.Objects) {
      var obj = Helper.SafeInstantiate(item.Prefab, container);
      obj.SetActive(true);
      obj.transform.localPosition = item.Pos;
      obj.transform.localRotation = item.Rot;
    }
    ZNetView.m_forceDisableInit = false;
    return container;
  }
  private static IEnumerable<string> Files() {
    if (!Directory.Exists(Settings.PlanBuildFolder)) Directory.CreateDirectory(Settings.PlanBuildFolder);
    if (!Directory.Exists(Settings.BuildShareFolder)) Directory.CreateDirectory(Settings.BuildShareFolder);
    var planBuild = Directory.EnumerateFiles(Settings.PlanBuildFolder, "*.blueprint", SearchOption.AllDirectories);
    var buildShare = Directory.EnumerateFiles(Settings.BuildShareFolder, "*.vbuild", SearchOption.AllDirectories);
    return planBuild.Concat(buildShare).OrderBy(s => s);
  }
  private static List<string> GetBlueprints() => Files().Select(path => Path.GetFileNameWithoutExtension(path).Replace(" ", "_")).ToList();
  private static Blueprint GetBluePrint(string name) {
    var path = Files().FirstOrDefault(path => Path.GetFileNameWithoutExtension(path).Replace(" ", "_") == name);
    if (path == null) throw new InvalidOperationException("Error: Blueprint not found.");
    var rows = File.ReadAllLines(path);
    return new() {
      Name = rows.Where(row => row.StartsWith("#Name:", StringComparison.Ordinal)).Select(row => row.Split(':')[1]).FirstOrDefault() ?? name,
      Description = rows.Where(row => row.StartsWith("#Description:", StringComparison.Ordinal)).Select(row => row.Split(':')[1]).FirstOrDefault() ?? "",
      Objects = rows.Where(row => !row.StartsWith("#", StringComparison.Ordinal)).Select(row => GetBluePrintObject(Path.GetExtension(path), row)).ToArray()
    };
  }
  private static BlueprintObject GetBluePrintObject(string extension, string row) {
    if (extension == ".vbuild") return GetBuildShareObject(row);
    if (extension == ".blueprint") return GetPlanBuildObject(row);
    throw new InvalidOperationException("Unknown file format.");
  }
  private static BlueprintObject GetPlanBuildObject(string row) {
    if (row.IndexOf(',') > -1) row = row.Replace(',', '.');
    var split = row.Split(';');
    var name = split[0];
    var posX = InvariantFloat(split[2]);
    var posY = InvariantFloat(split[3]);
    var posZ = InvariantFloat(split[4]);
    var rotX = InvariantFloat(split[5]);
    var rotY = InvariantFloat(split[6]);
    var rotZ = InvariantFloat(split[7]);
    var rotW = InvariantFloat(split[8]);
    return new BlueprintObject(name, posX, posY, posZ, rotX, rotY, rotZ, rotW);
  }
  private static BlueprintObject GetBuildShareObject(string row) {
    if (row.IndexOf(',') > -1) row = row.Replace(',', '.');
    var split = row.Split(' ');
    var name = split[0];
    var rotX = InvariantFloat(split[1]);
    var rotY = InvariantFloat(split[2]);
    var rotZ = InvariantFloat(split[3]);
    var rotW = InvariantFloat(split[4]);
    var posX = InvariantFloat(split[5]);
    var posY = InvariantFloat(split[6]);
    var posZ = InvariantFloat(split[7]);
    return new BlueprintObject(name, posX, posY, posZ, rotX, rotY, rotZ, rotW);
  }
  private static float InvariantFloat(string s) {
    if (string.IsNullOrEmpty(s)) return 0f;
    return float.Parse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
  }


  public HammerBlueprintCommand() {
    CommandWrapper.Register("hammer_blueprint", (int index, int subIndex) => GetBlueprints());
    new Terminal.ConsoleCommand("hammer_blueprint", "[blueprint file] - Selects the blueprint to be placed.", (Terminal.ConsoleEventArgs args) => {
      if (!Player.m_localPlayer) return;
      if (!Settings.Enabled) return;
      if (args.Length < 2) return;
      Hammer.Equip();
      try {
        var blueprint = GetBluePrint(string.Join("_", args.Args.Skip(1)));
        var obj = BuildObject(blueprint);
        if (Hammer.SetBlueprint(Player.m_localPlayer, obj))
          PrintSelected(args.Context, blueprint);
      } catch (InvalidOperationException e) {
        Helper.AddMessage(args.Context, e.Message);
      }
    }, optionsFetcher: GetBlueprints);
  }
}

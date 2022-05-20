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
  public List<BlueprintObject> Objects = new();
  public List<Vector3> SnapPoints = new();
}
public class HammerBlueprintCommand {
  private static void PrintSelected(Terminal terminal, Blueprint blueprint) {
    if (Settings.DisableSelectMessages) return;
    Helper.AddMessage(terminal, $"Selected {blueprint.Name}.");
  }

  private static GameObject BuildObject(Terminal terminal, Blueprint blueprint) {
    var container = new GameObject();
    // Prevents children from disappearing.
    container.SetActive(false);
    container.name = blueprint.Name;
    var piece = container.AddComponent<Piece>();
    piece.m_name = blueprint.Name;
    piece.m_description = blueprint.Description;
    ZNetView.m_forceDisableInit = true;
    foreach (var item in blueprint.Objects) {
      try {
        var obj = Helper.SafeInstantiate(item.Prefab, container);
        obj.SetActive(true);
        obj.transform.localPosition = item.Pos;
        obj.transform.localRotation = item.Rot;

      } catch (InvalidOperationException e) {
        Helper.AddMessage(terminal, $"Warning: {e.Message}");
      }
    }
    foreach (var position in blueprint.SnapPoints) {
      GameObject obj = new() {
        name = "_snappoint",
        layer = LayerMask.NameToLayer("piece"),
        tag = "snappoint"
      };
      obj.SetActive(false);
      obj.transform.SetParent(obj.transform);
      obj.transform.localPosition = position;
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
    if (path == null) throw new InvalidOperationException("Blueprint not found.");
    var rows = File.ReadAllLines(path);
    var extension = Path.GetExtension(path);
    Blueprint bp = new() { Name = name };
    if (extension == ".vbuild") return GetBuildShare(bp, rows);
    if (extension == ".blueprint") return GetPlanBuild(bp, rows);
    throw new InvalidOperationException("Unknown file format.");
  }
  private static Blueprint GetPlanBuild(Blueprint bp, string[] rows) {
    var piece = true;
    foreach (var row in rows) {
      if (row.StartsWith("#name:", StringComparison.OrdinalIgnoreCase))
        bp.Name = row.Split(':')[1];
      else if (row.StartsWith("#description:", StringComparison.OrdinalIgnoreCase))
        bp.Description = row.Split(':')[1];
      else if (row.StartsWith("#snappoints", StringComparison.OrdinalIgnoreCase))
        piece = false;
      else if (row.StartsWith("#pieces", StringComparison.OrdinalIgnoreCase))
        piece = true;
      else if (row.StartsWith("#", StringComparison.Ordinal))
        continue;
      else if (piece)
        bp.Objects.Add(GetPlanBuildObject(row));
      else
        bp.SnapPoints.Add(GetPlanBuildSnapPoint(row));
    }
    return bp;
  }
  private static BlueprintObject GetPlanBuildObject(string row) {
    if (row.IndexOf(',') > -1) row = row.Replace(',', '.');
    var split = row.Split(';');
    var name = split[0];
    var posX = InvariantFloat(split, 2);
    var posY = InvariantFloat(split, 3);
    var posZ = InvariantFloat(split, 4);
    var rotX = InvariantFloat(split, 5);
    var rotY = InvariantFloat(split, 6);
    var rotZ = InvariantFloat(split, 7);
    var rotW = InvariantFloat(split, 8);
    return new BlueprintObject(name, posX, posY, posZ, rotX, rotY, rotZ, rotW);
  }
  private static Vector3 GetPlanBuildSnapPoint(string row) {
    if (row.IndexOf(',') > -1) row = row.Replace(',', '.');
    var split = row.Split(';');
    var x = InvariantFloat(split, 0);
    var y = InvariantFloat(split, 1);
    var z = InvariantFloat(split, 2);
    return new Vector3(x, y, z);
  }
  private static Blueprint GetBuildShare(Blueprint bp, string[] rows) {
    bp.Objects = rows.Select(GetBuildShareObject).ToList();
    return bp;
  }
  private static BlueprintObject GetBuildShareObject(string row) {
    if (row.IndexOf(',') > -1) row = row.Replace(',', '.');
    var split = row.Split(' ');
    var name = split[0];
    var rotX = InvariantFloat(split, 1);
    var rotY = InvariantFloat(split, 2);
    var rotZ = InvariantFloat(split, 3);
    var rotW = InvariantFloat(split, 4);
    var posX = InvariantFloat(split, 5);
    var posY = InvariantFloat(split, 6);
    var posZ = InvariantFloat(split, 7);
    return new BlueprintObject(name, posX, posY, posZ, rotX, rotY, rotZ, rotW);
  }
  private static float InvariantFloat(string[] row, int index) {
    if (index >= row.Length) return 0f;
    var s = row[index];
    if (string.IsNullOrEmpty(s)) return 0f;
    return float.Parse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
  }

  public HammerBlueprintCommand() {
    CommandWrapper.Register("hammer_blueprint", (int index, int subIndex) => GetBlueprints());
    Helper.Command("hammer_blueprint", "[blueprint file] - Selects the blueprint to be placed.", (args) => {
      Helper.CheatCheck();
      Helper.ArgsCheck(args, 2, "Blueprint name is missing.");
      Hammer.Equip();
      var blueprint = GetBluePrint(string.Join("_", args.Args.Skip(1)));
      var obj = BuildObject(args.Context, blueprint);
      if (Hammer.SetBlueprint(Player.m_localPlayer, obj))
        PrintSelected(args.Context, blueprint);

    }, GetBlueprints);
  }
}

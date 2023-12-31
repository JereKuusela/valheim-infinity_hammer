using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ServerDevcommands;
using UnityEngine;
namespace InfinityHammer;
#pragma warning disable IDE0046
public class HammerBlueprintCommand
{
  private static void PrintSelected(Terminal terminal, string name)
  {
    if (Configuration.DisableSelectMessages) return;
    Helper.AddMessage(terminal, $"Selected {name}.");
  }
  private static IEnumerable<string> LoadFiles(string folder, IEnumerable<string> bps)
  {
    if (Directory.Exists(folder))
    {
      var blueprints = Directory.EnumerateFiles(folder, "*.blueprint", SearchOption.AllDirectories);
      var vbuilds = Directory.EnumerateFiles(folder, "*.vbuild", SearchOption.AllDirectories);
      return bps.Concat(blueprints).Concat(vbuilds);
    }
    return bps;
  }
  private static IEnumerable<string> Files()
  {
    IEnumerable<string> bps = new List<string>();
    bps = LoadFiles(Configuration.BlueprintGlobalFolder, bps);
    if (Path.GetFullPath(Configuration.BlueprintLocalFolder) != Path.GetFullPath(Configuration.BlueprintGlobalFolder))
      bps = LoadFiles(Configuration.BlueprintLocalFolder, bps);
    return bps.Distinct().OrderBy(s => s);
  }
  private static List<string> GetBlueprints() => Files().Select(path => Path.GetFileNameWithoutExtension(path).Replace(" ", "_")).ToList();
  private static Blueprint GetBluePrint(string name)
  {
    var path = Files().FirstOrDefault(path => Path.GetFileNameWithoutExtension(path).Replace(" ", "_") == name)
      ?? throw new InvalidOperationException("Blueprint not found.");
    var rows = File.ReadAllLines(path);
    var extension = Path.GetExtension(path);
    Blueprint bp = new() { Name = name };
    if (extension == ".vbuild") return GetBuildShare(bp, rows);
    if (extension == ".blueprint") return GetPlanBuild(bp, rows);
    throw new InvalidOperationException("Unknown file format.");
  }
  private static Blueprint GetPlanBuild(Blueprint bp, string[] rows)
  {
    var piece = true;
    foreach (var row in rows)
    {
      if (row.StartsWith("#name:", StringComparison.OrdinalIgnoreCase))
        bp.Name = row.Split(':')[1];
      else if (row.StartsWith("#description:", StringComparison.OrdinalIgnoreCase))
        bp.Description = row.Split(':')[1];
      else if (row.StartsWith("#center:", StringComparison.OrdinalIgnoreCase))
        bp.CenterPiece = row.Split(':')[1];
      else if (row.StartsWith("#coordinates:", StringComparison.OrdinalIgnoreCase))
        bp.Coordinates = Parse.VectorXZY(row.Split(':')[1]);
      else if (row.StartsWith("#rotation:", StringComparison.OrdinalIgnoreCase))
        bp.Rotation = Parse.VectorXZY(row.Split(':')[1]);
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

  private static BlueprintObject GetPlanBuildObject(string row)
  {
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
    var info = split.Length > 9 ? split[9] : "";
    var scaleX = InvariantFloat(split, 10, 1f);
    var scaleY = InvariantFloat(split, 11, 1f);
    var scaleZ = InvariantFloat(split, 12, 1f);
    var data = split.Length > 13 ? split[13] : "";
    return new BlueprintObject(name, new(posX, posY, posZ), new(rotX, rotY, rotZ, rotW), new(scaleX, scaleY, scaleZ), info, Deserialize(data));
  }
  public static ZPackage? Deserialize(string data) => data == "" ? null : new(data);
  private static Vector3 GetPlanBuildSnapPoint(string row)
  {
    if (row.IndexOf(',') > -1) row = row.Replace(',', '.');
    var split = row.Split(';');
    var x = InvariantFloat(split, 0);
    var y = InvariantFloat(split, 1);
    var z = InvariantFloat(split, 2);
    return new Vector3(x, y, z);
  }
  private static Blueprint GetBuildShare(Blueprint bp, string[] rows)
  {
    bp.Objects = rows.Select(GetBuildShareObject).ToList();
    return bp;
  }
  private static BlueprintObject GetBuildShareObject(string row)
  {
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
    var data = split.Length > 8 ? split[8] : "";
    return new BlueprintObject(name, new(posX, posY, posZ), new(rotX, rotY, rotZ, rotW), Vector3.one, "", Deserialize(data));
  }
  private static float InvariantFloat(string[] row, int index, float defaultValue = 0f)
  {
    if (index >= row.Length) return defaultValue;
    var s = row[index];
    if (string.IsNullOrEmpty(s)) return defaultValue;
    return float.Parse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
  }

  public HammerBlueprintCommand()
  {
    AutoComplete.Register("hammer_blueprint", (int index, int subIndex) =>
    {
      if (index == 0) return GetBlueprints();
      if (index == 1) return ParameterInfo.ObjectIds;
      if (index == 2) return ParameterInfo.Scale("scale", "Size of the object (if the object can be scaled).", subIndex);
      return null;
    });
    Helper.Command("hammer_blueprint", "[blueprint file] [center piece] [scale] - Selects the blueprint to be placed.", (args) =>
    {
      HammerHelper.CheatCheck();
      Helper.ArgsCheck(args, 2, "Blueprint name is missing.");
      Hammer.Equip();
      var name = args[1];
      var centerPiece = args.Length > 2 ? args[2] : "";
      var scale = args.Length > 3 ? Parse.Scale(Parse.Split(args[3])) : Vector3.one;
      var bp = GetBluePrint(name);
      bp.Center(centerPiece);
      var obj = Selection.Create(new ObjectSelection(args.Context, bp, scale));
      PrintSelected(args.Context, bp.Name);
    });

    AutoComplete.Register("hammer_restore", (int index, int subIndex) =>
    {
      if (index == 0) return GetBlueprints();
      if (index == 1) return ParameterInfo.Scale("scale", "Size of the object (if the object can be scaled).", subIndex);
      return null;
    });
    Helper.Command("hammer_restore", "[blueprint file] [scale] - Restores the blueprint at its saved position.", (args) =>
    {
      HammerHelper.CheatCheck();
      Helper.ArgsCheck(args, 2, "Blueprint name is missing.");
      Hammer.Equip();
      var name = args[1];
      var scale = args.Length > 2 ? Parse.Scale(Parse.Split(args[2])) : Vector3.one;
      var bp = GetBluePrint(name);
      bp.Center("");
      var obj = Selection.Create(new ObjectSelection(args.Context, bp, scale));
      Position.Override = bp.Coordinates;
      PlaceRotation.Set(Quaternion.Euler(bp.Rotation));
      PrintSelected(args.Context, bp.Name);
    });
  }
}

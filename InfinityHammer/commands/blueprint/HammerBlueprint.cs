using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data;
using ServerDevcommands;
using UnityEngine;
using Service;
namespace InfinityHammer;

#pragma warning disable IDE0046
public class HammerBlueprintCommand : TextReceiver
{
  private static void PrintSelected(Terminal terminal, string name)
  {
    if (Configuration.DisableSelectMessages) return;
    HammerHelper.Message(terminal, $"Selected {name}.");
  }
  // The file name is the identifier, the title from the file is only for display.
  public static string GetDisplayName(Blueprint bp) => bp.Title != "" ? bp.Title : bp.Name;

  public static List<string> GetBlueprints() => [.. Blueprints.Files().Select(path => Path.GetFileNameWithoutExtension(path).Replace(" ", "_"))];

  public static List<Blueprint> GetBlueprintsByCategory() => [.. Blueprints.Files().Select(GetBluePrintCategory)];

  public static List<Blueprint> GetBlueprintsByFolder() => [.. Blueprints.Files().Select(GetBluePrintWithFolder)];

  public static List<Blueprint> GetBlueprintsByCategoryAndFolder() => [.. Blueprints.Files().Select(GetBluePrintWithCategoryAndFolder)];

  // Menus only need the header, so the pieces are not parsed.
  private static Blueprint GetBluePrintCategory(string filePath)
  {
    var name = Path.GetFileNameWithoutExtension(filePath).Replace(" ", "_");
    var rows = File.ReadAllLines(filePath);
    var extension = Path.GetExtension(filePath);
    Blueprint bp = new(name);
    if (extension == ".vbuild") return bp;
    if (extension == ".blueprint") return GetPlanBuildCategory(bp, rows);
    throw new InvalidOperationException("Unknown file format.");
  }

  private static Blueprint GetBluePrintWithFolder(string filePath)
  {
    var name = Path.GetFileNameWithoutExtension(filePath).Replace(" ", "_");
    var category = GetFolderNameFromPath(filePath);
    return new(name) { Category = category };
  }

  private static Blueprint GetBluePrintWithCategoryAndFolder(string filePath)
  {
    var rows = File.ReadAllLines(filePath);
    var extension = Path.GetExtension(filePath);

    Blueprint bp = GetBluePrintWithFolder(filePath);
    if (extension == ".vbuild") return bp;
    if (extension == ".blueprint") return GetPlanBuildCategory(bp, rows);
    throw new InvalidOperationException("Unknown file format.");
  }

  private static string GetFolderNameFromPath(string filePath)
  {
    var path = Path.GetDirectoryName(filePath);
    if (string.IsNullOrEmpty(path))
      return "Blueprints";
    var name = Path.GetFileName(path);
    return string.IsNullOrEmpty(name) ? "Blueprints" : name;
  }

  private static Blueprint GetBluePrint(string name, bool loadData)
    => Blueprints.GetBluePrint(name, loadData) ?? throw new InvalidOperationException($"Blueprint {name} was not found or is invalid. See the log for details.");

  private static Blueprint GetPlanBuildCategory(Blueprint bp, string[] rows)
  {
    foreach (var row in rows)
    {
      if (row.StartsWith("#name:", StringComparison.OrdinalIgnoreCase))
        bp.Name = row.Split(':')[1];
      else if (row.StartsWith("#category:", StringComparison.OrdinalIgnoreCase))
        bp.Category = row.Split(':')[1];
      else if (row.StartsWith("#pieces", StringComparison.OrdinalIgnoreCase))
        return bp;
    }
    return bp;
  }

  // The configured center piece is the default for files that don't define one.
  public static Vector3 CenterBlueprint(Blueprint bp, string centerPiece)
  {
    if (centerPiece == "" && Configuration.BlueprintCenterPiece != "" && bp.CenterPiece == Blueprint.DefaultCenterPiece)
      centerPiece = Configuration.BlueprintCenterPiece;
    return bp.Center(centerPiece, true);
  }

  private void SelectBlueprint(Terminal.ConsoleEventArgs args, HammerBlueprintPars pars)
  {
    if (pars.Name == "") return;
    Hammer.Equip();
    var bp = GetBluePrint(pars.Name, pars.LoadData);
    CenterBlueprint(bp, pars.CenterPiece);
    if (pars.SnapPiece != "")
    {
      foreach (var snap in bp.SnapPoints)
        bp.Objects.Add(new(pars.SnapPiece, snap.Pos, Quaternion.identity, Vector3.one, null, 1f));
    }
    if (bp.TerrainHeight != null)
      bp.TerrainHeight.Smooth = pars.HeightSmooth ?? Configuration.BlueprintTerrainHeightSmooth;
    if (bp.TerrainPaint != null)
      bp.TerrainPaint.Smooth = pars.PaintSmooth ?? Configuration.BlueprintTerrainPaintSmooth;

    Selection.CreateGhost(new ObjectSelection(args.Context, bp, pars.Scale));
    PrintSelected(args.Context, GetDisplayName(bp));
  }

  private static void RegisterAutoComplete(string command)
  {
    AutoComplete.Register(command, (index, subIndex) =>
    {
      if (index == 0) return GetBlueprints();
      return ["c", "center", "d", "data", "sc", "scale", "s", "snap", "sm", "smooth"];
    },
    new() {
      { "scale", index => ParameterInfo.Scale("scale", "Size of the object (if the object can be scaled).", index) },
      { "sc", index => ParameterInfo.Scale("scale", "Size of the object (if the object can be scaled).", index) },
      { "center", index => ParameterInfo.ObjectIds},
      { "c", index => ParameterInfo.ObjectIds},
      { "snap", index => ParameterInfo.ObjectIds},
      { "s", index => ParameterInfo.ObjectIds},
      { "data", index => ["true", "false"]},
      { "d", index => ["true", "false"]},
      { "smooth", index => ParameterInfo.Create("Terrain smooth (0.0-1.0) for heigh or height,paint.") },
      { "sm", index => ParameterInfo.Create("Terrain smooth (0.0-1.0) for height or or height,paint.") },
    });
  }

  public HammerBlueprintCommand()
  {
    RegisterAutoComplete("hammer_blueprint");
    Helper.Command("hammer_blueprint", "[blueprint file] [center=piece] [snap=piece] [scale=x,z,y] [data=true/false] - Selects the blueprint to be placed.", (args) =>
    {
      Args = args;
      Pars = new(args);
      if (Pars.Name != "")
      {
        SelectBlueprint(args, Pars);
        Pars = null;
        Args = null;
      }
      else
        TextInput.instance.RequestText(this, "Name", 1000);
    });

    RegisterAutoComplete("hammer_restore");
    Helper.Command("hammer_restore", "[blueprint file] [center=piece] [snap=piece] [scale=x,z,y] [data=true/false] - Restores the blueprint at its saved position.", (args) =>
    {
      Helper.ArgsCheck(args, 2, "Blueprint name is missing.");
      Hammer.Equip();
      var pars = new HammerBlueprintPars(args);
      var bp = GetBluePrint(pars.Name, pars.LoadData);
      // The saved position/rotation are anchored to the blueprint's own center, so
      // any additional offset from a custom center piece must be added back here.
      var centerOffset = CenterBlueprint(bp, pars.CenterPiece);
      if (pars.SnapPiece != "")
      {
        foreach (var snap in bp.SnapPoints)
          bp.Objects.Add(new(pars.SnapPiece, snap.Pos, Quaternion.identity, Vector3.one, null, 1f));
      }
      if (bp.TerrainHeight != null)
        bp.TerrainHeight.Smooth = pars.HeightSmooth ?? Configuration.BlueprintTerrainHeightSmooth;
      if (bp.TerrainPaint != null)
        bp.TerrainPaint.Smooth = pars.PaintSmooth ?? Configuration.BlueprintTerrainPaintSmooth;
      var obj = Selection.CreateGhost(new ObjectSelection(args.Context, bp, pars.Scale));
      Position.Override = bp.Coordinates + Quaternion.Euler(bp.Rotation) * centerOffset;
      PlaceRotation.Set(Quaternion.Euler(bp.Rotation));
      PrintSelected(args.Context, GetDisplayName(bp));
    });
  }

  HammerBlueprintPars? Pars = null;
  Terminal.ConsoleEventArgs? Args = null;

  public string GetText() => "";

  public void SetText(string text)
  {
    if (Pars == null || Args == null)
      return;
    Pars.Name = text;
    SelectBlueprint(Args, Pars);
    Pars = null;
    Args = null;
  }
}

public class HammerBlueprintPars
{
  public string Name = "";
  public string CenterPiece = "";
  public string SnapPiece = "";
  public bool LoadData = true;
  public Vector3 Scale = Vector3.one;
  public float? HeightSmooth;
  public float? PaintSmooth;

  private static float ClampSmooth(string value)
  {
    var parsed = Parse.Float(value);
    if (parsed < 0f) return 0f;
    if (parsed > 1f) return 1f;
    return parsed;
  }

  public HammerBlueprintPars(Terminal.ConsoleEventArgs args)
  {
    var pars = args.Args.Skip(1).ToArray();
    for (int i = 0; i < pars.Length; i++)
    {
      var par = pars[i];
      var split = par.Split('=');
      if (split.Length < 2)
      {
        // Legacy support.
        if (i == 0) Name = par;
        if (i == 1) CenterPiece = par;
        if (i == 2) Scale = Parse.Scale(Parse.Split(par));
        continue;
      }
      if (split[0] == "center" || split[0] == "c")
        CenterPiece = split[1];
      if (split[0] == "snap" || split[0] == "s")
        SnapPiece = split[1];
      if (split[0] == "data" || split[0] == "d")
        LoadData = Parse.BoolNull(split[1]) ?? true;
      if (split[0] == "scale" || split[0] == "sc")
        Scale = Parse.Scale(Parse.Split(split[1]));
      if (split[0] == "smooth" || split[0] == "sm")
      {
        var values = split[1].Split(',');
        if (values.Length > 0 && !string.IsNullOrWhiteSpace(values[0]))
          HeightSmooth = ClampSmooth(values[0]);
        if (values.Length > 1 && !string.IsNullOrWhiteSpace(values[1]))
          PaintSmooth = ClampSmooth(values[1]);
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ServerDevcommands;
using UnityEngine;
using Service;
namespace InfinityHammer;

#pragma warning disable IDE0046
public class HammerBlueprintCommand : TextReceiver
{
  private enum PlanBuildSection
  {
    Skip,
    Pieces,
    SnapPoints,
    TerrainHeight,
    TerrainPaint
  }

  private static void PrintSelected(Terminal terminal, string name)
  {
    if (Configuration.DisableSelectMessages) return;
    HammerHelper.Message(terminal, $"Selected {name}.");
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
    IEnumerable<string> bps = [];
    bps = LoadFiles(Configuration.BlueprintGlobalFolder, bps);
    if (Path.GetFullPath(Configuration.BlueprintLocalFolder) != Path.GetFullPath(Configuration.BlueprintGlobalFolder))
      bps = LoadFiles(Configuration.BlueprintLocalFolder, bps);
    return bps.Distinct().OrderBy(s => s);
  }
  public static List<string> GetBlueprints() => [.. Files().Select(path => Path.GetFileNameWithoutExtension(path).Replace(" ", "_"))];

  public static List<Blueprint> GetBlueprintsByCategory() => [.. Files().Select(GetBluePrintCategory)];

  public static List<Blueprint> GetBlueprintsByFolder() => [.. Files().Select(GetBluePrintWithFolder)];

  public static List<Blueprint> GetBlueprintsByCategoryAndFolder() => [.. Files().Select(GetBluePrintWithCategoryAndFolder)];

  private static Blueprint GetBluePrintCategory(string filePath)
  {
    var name = Path.GetFileNameWithoutExtension(filePath).Replace(" ", "_");
    var rows = File.ReadAllLines(filePath);
    var extension = Path.GetExtension(filePath);
    Blueprint bp = new() { Name = name };
    if (extension == ".vbuild") return bp;
    if (extension == ".blueprint") return GetPlanBuildCategory(bp, rows);
    throw new InvalidOperationException("Unknown file format.");
  }

  private static Blueprint GetBluePrintWithFolder(string filePath)
  {
    var name = Path.GetFileNameWithoutExtension(filePath).Replace(" ", "_");
    var category = GetFolderNameFromPath(filePath);
    return new() { Name = name, Category = category };
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
  {
    var path = Files().FirstOrDefault(path => Path.GetFileNameWithoutExtension(path).Replace(" ", "_") == name)
      ?? throw new InvalidOperationException("Blueprint not found.");
    var rows = File.ReadAllLines(path);
    var extension = Path.GetExtension(path);
    Blueprint bp = new() { Name = name };
    if (extension == ".vbuild") return GetBuildShare(bp, rows, loadData);
    if (extension == ".blueprint") return GetPlanBuild(bp, rows, loadData);
    throw new InvalidOperationException("Unknown file format.");
  }
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
  internal static Blueprint GetPlanBuild(Blueprint bp, string[] rows, bool loadData)
  {
    var section = PlanBuildSection.Pieces;
    TerrainHeight? terrainHeightData = null;
    TerrainPaint? terrainPaintData = null;
    var terrainRowIndex = 0;

    foreach (var row in rows)
    {
      var isHeader = row.StartsWith("#", StringComparison.Ordinal);
      if (isHeader)
      {
        terrainRowIndex = 0;
        // Metadata and comments can appear between legacy piece rows. Terrain
        // payloads, however, always end at the next header.
        if (section is PlanBuildSection.TerrainHeight or PlanBuildSection.TerrainPaint)
          section = PlanBuildSection.Skip;
      }

      if (row.StartsWith("#name:", StringComparison.OrdinalIgnoreCase))
        bp.Name = row.Split(':')[1];
      else if (row.StartsWith("#creator:", StringComparison.OrdinalIgnoreCase))
        bp.Creator = row.Split(':')[1];
      else if (row.StartsWith("#description:", StringComparison.OrdinalIgnoreCase))
        bp.Description = row.Split(':')[1];
      else if (row.StartsWith("#category:", StringComparison.OrdinalIgnoreCase))
        bp.Category = row.Split(':')[1];
      else if (row.StartsWith("#center:", StringComparison.OrdinalIgnoreCase))
        bp.CenterPiece = row.Split(':')[1];
      else if (row.StartsWith("#coordinates:", StringComparison.OrdinalIgnoreCase))
        bp.Coordinates = Parse.VectorXZY(row.Split(':')[1]);
      else if (row.StartsWith("#rotation:", StringComparison.OrdinalIgnoreCase))
        bp.Rotation = Parse.VectorXZY(row.Split(':')[1]);
      else if (row.StartsWith("#height:", StringComparison.OrdinalIgnoreCase) || row.StartsWith("#paint:", StringComparison.OrdinalIgnoreCase))
      {
        throw new InvalidOperationException("Legacy #Height/#Paint terrain format is no longer supported. Re-save the blueprint with a newer Infinity Hammer version.");
      }
      else if (row.StartsWith("#terrainheight:", StringComparison.OrdinalIgnoreCase))
      {
        // A malformed channel header must not inherit the preceding section.
        section = PlanBuildSection.Skip;
        var parts = row.Split(':')[1].Split(';');
        if (parts.Length >= 3)
        {
          var rotation = InvariantFloat(parts, 1, 0f);
          terrainHeightData = new TerrainHeight
          {
            CenterPosition = Parse.VectorXZY(parts[0]),
            CenterRotation = Quaternion.Euler(0f, rotation, 0f),
            DistanceBetweenNodes = InvariantFloat(parts, 2, 1.0f)
          };
          section = PlanBuildSection.TerrainHeight;
        }
      }
      else if (row.StartsWith("#terrainpaint:", StringComparison.OrdinalIgnoreCase))
      {
        // A malformed channel header must not inherit the preceding section.
        section = PlanBuildSection.Skip;
        var parts = row.Split(':')[1].Split(';');
        if (parts.Length >= 3)
        {
          var rotation = InvariantFloat(parts, 1, 0f);
          terrainPaintData = new TerrainPaint
          {
            CenterPosition = Parse.VectorXZY(parts[0]),
            CenterRotation = Quaternion.Euler(0f, rotation, 0f),
            DistanceBetweenNodes = InvariantFloat(parts, 2, 1.0f)
          };
          section = PlanBuildSection.TerrainPaint;
        }
      }
      else if (row.StartsWith("#snappoints", StringComparison.OrdinalIgnoreCase))
        section = PlanBuildSection.SnapPoints;
      else if (row.StartsWith("#pieces", StringComparison.OrdinalIgnoreCase))
        section = PlanBuildSection.Pieces;
      else if (isHeader)
      {
        // Keep ordinary comments compatible with headerless piece lists, but
        // skip payload following unknown section headers.
        var isComment = row.Length == 1 || char.IsWhiteSpace(row[1]);
        if (!isComment)
          section = PlanBuildSection.Skip;
        continue;
      }
      else
      {
        switch (section)
        {
          case PlanBuildSection.Pieces:
            bp.Objects.Add(GetPlanBuildObject(row, loadData));
            break;
          case PlanBuildSection.SnapPoints:
            bp.SnapPoints.Add(GetPlanBuildSnapPoint(row));
            break;
          case PlanBuildSection.TerrainHeight when terrainHeightData != null:
            ParseTerrainHeightRow(terrainHeightData, row, terrainRowIndex);
            terrainRowIndex++;
            break;
          case PlanBuildSection.TerrainPaint when terrainPaintData != null:
            ParseTerrainPaintRow(terrainPaintData, row, terrainRowIndex);
            terrainRowIndex++;
            break;
        }
      }
    }

    bp.TerrainHeight = terrainHeightData;
    bp.TerrainPaint = terrainPaintData;

    return bp;
  }

  private static BlueprintObject GetPlanBuildObject(string row, bool loadData)
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
    var data = loadData && split.Length > 13 ? split[13] : "";
    var chance = InvariantFloat(split, 14, 1f);
    return new BlueprintObject(name, new(posX, posY, posZ), new(rotX, rotY, rotZ, rotW), new(scaleX, scaleY, scaleZ), info, data, chance);
  }
  private static Vector3 GetPlanBuildSnapPoint(string row)
  {
    if (row.IndexOf(',') > -1) row = row.Replace(',', '.');
    var split = row.Split(';');
    var x = InvariantFloat(split, 0);
    var y = InvariantFloat(split, 1);
    var z = InvariantFloat(split, 2);
    return new Vector3(x, y, z);
  }
  private static Blueprint GetBuildShare(Blueprint bp, string[] rows, bool loadData)
  {
    bp.Objects = rows.Select(r => GetBuildShareObject(r, loadData)).ToList();
    return bp;
  }
  private static BlueprintObject GetBuildShareObject(string row, bool loadData)
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
    var data = loadData && split.Length > 8 ? split[8] : "";
    var chance = InvariantFloat(split, 9, 1f);
    return new BlueprintObject(name, new(posX, posY, posZ), new(rotX, rotY, rotZ, rotW), Vector3.one, "", data, chance);
  }

  private static void ParseTerrainHeightRow(TerrainHeight terrainData, string row, int rowIndex)
  {
    if (row.IndexOf(',') > -1) row = row.Replace(',', '.');
    var values = row.Split(';');

    if (rowIndex == 0)
    {
      terrainData.InitializeGrid(values.Length, 0, terrainData.CenterPosition);
    }

    if (rowIndex >= terrainData.Height)
    {
      var oldHeights = terrainData.Heights;
      terrainData.Heights = new float?[terrainData.Width, rowIndex + 1];

      for (int x = 0; x < terrainData.Width; x++)
      {
        for (int z = 0; z < terrainData.Height; z++)
        {
          terrainData.Heights[x, z] = oldHeights[x, z];
        }
      }
      terrainData.Height = rowIndex + 1;
    }

    for (int x = 0; x < values.Length && x < terrainData.Width; x++)
    {
      if (!string.IsNullOrEmpty(values[x]))
      {
        if (float.TryParse(values[x], NumberStyles.Any, NumberFormatInfo.InvariantInfo, out float height))
        {
          terrainData.Set(x, rowIndex, height);
        }
      }
    }
  }

  private static void ParseTerrainPaintRow(TerrainPaint terrainData, string row, int rowIndex)
  {
    if (row.IndexOf(',') > -1) row = row.Replace(',', '.');
    var values = row.Split(';');

    if (rowIndex == 0)
    {
      terrainData.InitializeGrid(values.Length, 0, terrainData.CenterPosition);
    }

    if (rowIndex >= terrainData.Height)
    {
      var oldPaints = terrainData.Paints;
      terrainData.Paints = new Color?[terrainData.Width, rowIndex + 1];
      for (int x = 0; x < terrainData.Width; x++)
      {
        for (int z = 0; z < terrainData.Height; z++)
        {
          terrainData.Paints[x, z] = oldPaints[x, z];
        }
      }
      terrainData.Height = rowIndex + 1;
    }

    for (int x = 0; x < values.Length && x < terrainData.Width; x++)
    {
      if (!string.IsNullOrEmpty(values[x]))
      {
        var colorParts = values[x].Split(':');
        if (colorParts.Length >= 3)
        {
          if (float.TryParse(colorParts[0], NumberStyles.Any, NumberFormatInfo.InvariantInfo, out float r) &&
              float.TryParse(colorParts[1], NumberStyles.Any, NumberFormatInfo.InvariantInfo, out float g) &&
              float.TryParse(colorParts[2], NumberStyles.Any, NumberFormatInfo.InvariantInfo, out float b))
          {
            float a = colorParts.Length > 3 && float.TryParse(colorParts[3], NumberStyles.Any, NumberFormatInfo.InvariantInfo, out float alpha) ? alpha : 1f;
            terrainData.Set(x, rowIndex, new Color(r, g, b, a));
          }
        }
      }
    }
  }

  private static float InvariantFloat(string[] row, int index, float defaultValue = 0f)
  {
    if (index >= row.Length) return defaultValue;
    var s = row[index];
    if (string.IsNullOrEmpty(s)) return defaultValue;
    return float.Parse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
  }

  private void SelectBlueprint(Terminal.ConsoleEventArgs args, HammerBlueprintPars pars)
  {
    if (pars.Name == "") return;
    Hammer.Equip();
    var bp = GetBluePrint(pars.Name, pars.LoadData);
    bp.Center(pars.CenterPiece);
    if (pars.SnapPiece != "")
    {
      foreach (var snap in bp.SnapPoints)
        bp.Objects.Add(new BlueprintObject(pars.SnapPiece, snap, Quaternion.identity, Vector3.one, "", "", 1f));
    }
    if (bp.TerrainHeight != null)
      bp.TerrainHeight.Smooth = pars.HeightSmooth ?? Configuration.BlueprintTerrainHeightSmooth;
    if (bp.TerrainPaint != null)
      bp.TerrainPaint.Smooth = pars.PaintSmooth ?? Configuration.BlueprintTerrainPaintSmooth;

    Selection.CreateGhost(new ObjectSelection(args.Context, bp, pars.Scale));
    PrintSelected(args.Context, bp.Name);
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
      var centerOffset = bp.Center(pars.CenterPiece);
      if (pars.SnapPiece != "")
      {
        foreach (var snap in bp.SnapPoints)
          bp.Objects.Add(new BlueprintObject(pars.SnapPiece, snap, Quaternion.identity, Vector3.one, "", "", 1f));
      }
      if (bp.TerrainHeight != null)
        bp.TerrainHeight.Smooth = pars.HeightSmooth ?? Configuration.BlueprintTerrainHeightSmooth;
      if (bp.TerrainPaint != null)
        bp.TerrainPaint.Smooth = pars.PaintSmooth ?? Configuration.BlueprintTerrainPaintSmooth;
      var obj = Selection.CreateGhost(new ObjectSelection(args.Context, bp, pars.Scale));
      Position.Override = bp.Coordinates + Quaternion.Euler(bp.Rotation) * centerOffset;
      PlaceRotation.Set(Quaternion.Euler(bp.Rotation));
      PrintSelected(args.Context, bp.Name);
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

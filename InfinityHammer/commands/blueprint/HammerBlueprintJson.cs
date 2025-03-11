using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ServerDevcommands;
using UnityEngine;

namespace InfinityHammer;
#pragma warning disable IDE0046
public class HammerBlueprintCommandJson
{
    private static void PrintSelected(Terminal terminal, string name)
    {
        if (Configuration.DisableSelectMessages) return;
        HammerHelper.Message(terminal, $"Selected {name}.");
    }

    private static IEnumerable<string> LoadFiles(string folder,
        IEnumerable<string> bps)
    {
        if (Directory.Exists(folder))
        {
            var blueprints = Directory.EnumerateFiles(folder, "*.blueprint",
                SearchOption.AllDirectories);
            var vbuilds = Directory.EnumerateFiles(folder, "*.vbuild",
                SearchOption.AllDirectories);
            return bps.Concat(blueprints).Concat(vbuilds);
        }

        return bps;
    }

    private static IEnumerable<string> Files()
    {
        IEnumerable<string> bps = new List<string>();
        bps = LoadFiles(Configuration.BlueprintGlobalFolder, bps);
        if (Path.GetFullPath(Configuration.BlueprintLocalFolder) !=
            Path.GetFullPath(Configuration.BlueprintGlobalFolder))
            bps = LoadFiles(Configuration.BlueprintLocalFolder, bps);
        return bps.Distinct().OrderBy(s => s);
    }

    private static List<string> GetBlueprints() => Files()
        .Select(
            path => Path.GetFileNameWithoutExtension(path).Replace(" ", "_"))
        .ToList();

    private static BlueprintJson GetBluePrint(string name, bool loadData)
    {
        var path = Files().FirstOrDefault(path =>
                       Path.GetFileNameWithoutExtension(path)
                           .Replace(" ", "_") == name)
                   ?? throw new InvalidOperationException(
                       "Blueprint not found.");
        var rows = File.ReadAllLines(path);
        var extension = Path.GetExtension(path);
        BlueprintJson bp = new() { Name = name };
        if (extension == ".vbuild") return GetBuildShare(bp, rows, loadData);
        if (extension == ".blueprint") return GetPlanBuild(bp, rows, loadData);
        throw new InvalidOperationException("Unknown file format.");
    }

    private static BlueprintJson GetPlanBuild(BlueprintJson bp, string[] rows,
        bool loadData)
    {
        var piece = true;
        foreach (var row in rows)
        {
            if (row.StartsWith("#name:", StringComparison.OrdinalIgnoreCase))
                bp.Name = row.Split(':')[1];
            else if (row.StartsWith("#description:",
                         StringComparison.OrdinalIgnoreCase))
                bp.Description = row.Split(':')[1];
            else if (row.StartsWith("#center:",
                         StringComparison.OrdinalIgnoreCase))
                bp.CenterPiece = row.Split(':')[1];
            else if (row.StartsWith("#coordinates:",
                         StringComparison.OrdinalIgnoreCase))
                bp.Coordinates = Parse.VectorXZY(row.Split(':')[1]);
            else if (row.StartsWith("#rotation:",
                         StringComparison.OrdinalIgnoreCase))
                bp.Rotation = Parse.VectorXZY(row.Split(':')[1]);
            else if (row.StartsWith("#snappoints",
                         StringComparison.OrdinalIgnoreCase))
                piece = false;
            else if (row.StartsWith("#pieces",
                         StringComparison.OrdinalIgnoreCase))
                piece = true;
            else if (row.StartsWith("#", StringComparison.Ordinal))
                continue;
            else if (piece)
                bp.Objects.Add(GetPlanBuildObject(row, loadData));
            else
                bp.SnapPoints.Add(GetPlanBuildSnapPoint(row));
        }

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
        return new BlueprintObject(name, new(posX, posY, posZ),
            new(rotX, rotY, rotZ, rotW), new(scaleX, scaleY, scaleZ), info,
            data, chance);
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

    private static BlueprintJson GetBuildShare(BlueprintJson bp, string[] rows,
        bool loadData)
    {
        bp.Objects = rows.Select(r => GetBuildShareObject(r, loadData))
            .ToList();
        return bp;
    }

    private static BlueprintObject GetBuildShareObject(string row,
        bool loadData)
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
        return new BlueprintObject(name, new(posX, posY, posZ),
            new(rotX, rotY, rotZ, rotW), Vector3.one, "", data, chance);
    }

    private static float InvariantFloat(string[] row, int index,
        float defaultValue = 0f)
    {
        if (index >= row.Length) return defaultValue;
        var s = row[index];
        if (string.IsNullOrEmpty(s)) return defaultValue;
        return float.Parse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
    }

    public HammerBlueprintCommandJson()
    {
        AutoComplete.Register("hammer_blueprint_json", (int index, int subIndex) =>
            {
                if (index == 0) return GetBlueprints();
                return ["c", "center", "d", "data", "sc", "scale", "s", "snap"];
            },
            new()
            {
                {
                    "scale",
                    (int index) => ParameterInfo.Scale("scale",
                        "Size of the object (if the object can be scaled).",
                        index)
                },
                {
                    "sc",
                    (int index) => ParameterInfo.Scale("scale",
                        "Size of the object (if the object can be scaled).",
                        index)
                },
                { "center", (int index) => ParameterInfo.ObjectIds },
                { "c", (int index) => ParameterInfo.ObjectIds },
                { "snap", (int index) => ParameterInfo.ObjectIds },
                { "s", (int index) => ParameterInfo.ObjectIds },
                { "data", (int index) => ["true", "false"] },
                { "d", (int index) => ["true", "false"] },
            });
        Helper.Command("hammer_blueprint_json",
            "[blueprint file] [center=piece] [snap=piece] [scale=x,z,y] [data=true/false] - Selects the json blueprint to be placed.",
            (args) =>
            {
                HammerHelper.CheatCheck();
                Helper.ArgsCheck(args, 2, "Blueprint name is missing.");
                Hammer.Equip();
                var name = args[1];
                HammerBlueprintPars pars = new(args);
                var bp = GetBluePrint(name, pars.LoadData);
                bp.Center(pars.CenterPiece);
                if (pars.SnapPiece != "")
                {
                    foreach (var snap in bp.SnapPoints)
                        bp.Objects.Add(new BlueprintObject(pars.SnapPiece, snap,
                            Quaternion.identity, Vector3.one, "", "", 1f));
                }

                var obj =
                    Selection.CreateGhost(
                        new ObjectSelection(args.Context, bp, pars.Scale));
                PrintSelected(args.Context, bp.Name);
            });

        AutoComplete.Register("hammer_restore", (int index, int subIndex) =>
        {
            if (index == 0) return GetBlueprints();
            if (index == 1)
                return ParameterInfo.Scale("scale",
                    "Size of the object (if the object can be scaled).",
                    subIndex);
            return ParameterInfo.None;
        });
        Helper.Command("hammer_restore",
            "[blueprint file] [scale] - Restores the blueprint at its saved position.",
            (args) =>
            {
                HammerHelper.CheatCheck();
                Helper.ArgsCheck(args, 2, "Blueprint name is missing.");
                Hammer.Equip();
                var name = args[1];
                var scale = args.Length > 2
                    ? Parse.Scale(Parse.Split(args[2]))
                    : Vector3.one;
                var bp = GetBluePrint(name, true);
                bp.Center("");
                var obj =
                    Selection.CreateGhost(
                        new ObjectSelection(args.Context, bp, scale));
                Position.Override = bp.Coordinates;
                PlaceRotation.Set(Quaternion.Euler(bp.Rotation));
                PrintSelected(args.Context, bp.Name);
            });
    }
}

public class HammerBlueprintParsJson
{
    public string CenterPiece = "";
    public string SnapPiece = "";
    public bool LoadData = true;
    public Vector3 Scale = Vector3.one;

    public HammerBlueprintParsJson(Terminal.ConsoleEventArgs args)
    {
        var pars = args.Args.Skip(2).ToArray();
        int index = 0;
        foreach (var par in pars)
        {
            var split = par.Split('=');
            if (split.Length < 2)
            {
                // Legacy support.
                if (index == 0) CenterPiece = par;
                if (index == 1) Scale = Parse.Scale(Parse.Split(par));
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
        }
    }
}
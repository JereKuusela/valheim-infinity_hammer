﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
        var piece = false;

        for (int i = 0; i < rows.Length;i++)
        {
            if (rows[i].StartsWith("]",
                    StringComparison.OrdinalIgnoreCase) && piece)
            {
                piece = false;
            }
            else if (piece)
            {
                var str = rows[i].TrimEnd(','); // remove comma at end if any
                try
                {
                    bp.Objects.Add(
                        JsonUtility.FromJson<BlueprintObjectJson>(str));
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e);
                    System.Console.WriteLine("error while parsing: piece: " +
                                             str + "");
                }
            }
            else if (rows[i].StartsWith("{\"Header\":",
                         StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    i++;
                    bp.header = JsonUtility.FromJson<BlueprintHeader>(rows[i].TrimEnd(','));
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e);
                    System.Console.WriteLine("error while parsing: header: " +
                                             rows[i] + "");
                }
            }
            else if (rows[i].StartsWith("\"Objects\":",
                         StringComparison.OrdinalIgnoreCase))
            {
                piece = true;
            }
        }

        return bp;
    }

    

    private static BlueprintJson GetBuildShare(BlueprintJson bp, string[] rows,
        bool loadData)
    {
        bp.Objects = rows.Select(r => GetBuildShareObject(r, loadData) as IBlueprintObject)
            .ToList();
        return bp;
    }

    private static BlueprintObjectJson GetBuildShareObject(string row,
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
        return new BlueprintObjectJson(name, new(posX, posY, posZ),
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
        AutoComplete.Register("hammer_blueprint_json",
            (int index, int subIndex) =>
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
                        bp.Objects.Add(new BlueprintObjectJson(pars.SnapPiece,
                            snap,
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

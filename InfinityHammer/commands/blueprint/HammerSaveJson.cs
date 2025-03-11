using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Data;
using ServerDevcommands;
using Service;
using UnityEngine;
using UnityEngine.Rendering;

namespace InfinityHammer;

public class HammerSaveCommandJson
{
  

    private static BlueprintJson BuildBluePrint(Player player, GameObject obj,
        string centerPiece, string snapPiece, bool saveData)
    {
        BlueprintJson bp = new()
        {
            Name = Utils.GetPrefabName(obj),
            Creator = player.GetPlayerName(),
            Rotation = HammerHelper.GetPlacementGhost().transform.rotation
                .eulerAngles,
        };
        var piece = obj.GetComponent<Piece>();
        if (piece)
        {
            bp.Name = Localization.instance.Localize(piece.m_name);
        }

        if (Selection.Get() is not ObjectSelection selection) return bp;
        var objects = Snapping.GetChildren(obj);
        Dictionary<string, string> pars = [];
        if (selection.Objects.Count() == 1)
        {
            bp.AddSingleObject(pars, obj, saveData);
            // Snap points are sort of useful for single objects.
            // Since single objects should just have custom data but otherwise the original behavior.
            var snaps = Snapping.GetSnapPoints(obj);
            foreach (var snap in snaps)
                bp.SnapPoints.Add(snap.transform.localPosition);
        }
        else
        {
            var i = 0;
            foreach (var tr in objects)
            {
                if (snapPiece != "" && Utils.GetPrefabName(tr) == snapPiece)
                    bp.SnapPoints.Add(tr.transform.localPosition);
                else
                    bp.AddObject(pars, tr, saveData, i);
                i += 1;
            }

            if (snapPiece == "")
            {
                var snaps = Snapping.GetSnapPoints(obj);
                foreach (var snap in snaps)
                    bp.SnapPoints.Add(snap.transform.localPosition);
            }
        }

        var offset = bp.Center(centerPiece);
        bp.Coordinates = player.m_placementGhost.transform.position - offset;
        return bp;
    }

   





    public HammerSaveCommandJson()
    {
        AutoComplete.Register("hammer_save_json", (int index) =>
        {
            if (index == 0) return ParameterInfo.Create("File name.");
            return ["c", "center", "d", "data", "p", "profile", "s", "snap"];
        }, new()
        {
            { "c", (int index) => ParameterInfo.ObjectIds },
            { "center", (int index) => ParameterInfo.ObjectIds },
            { "d", (int index) => ["true", "false"] },
            { "data", (int index) => ["true", "false"] },
            { "p", (int index) => ["true", "false"] },
            { "profile", (int index) => ["true", "false"] },
            { "s", (int index) => ParameterInfo.ObjectIds },
            { "snap", (int index) => ParameterInfo.ObjectIds },
        });
        Helper.Command("hammer_save_json",
            "[file name] [center=piece] [snap=piece] [data=true/false] [profile=true/false] - Saves the selection to a  json blueprint.",
            (args) =>
            {
                HammerHelper.CheatCheck();
                Helper.ArgsCheck(args, 2, "Blueprint name is missing.");
                var player = Helper.GetPlayer();
                var ghost = HammerHelper.GetPlacementGhost();
                HammerSaveParsJson pars = new(args);
                var bp = BuildBluePrint(player, ghost, pars.CenterPiece,
                    pars.SnapPiece, pars.SaveData);
                var lines = bp.GetPlanBuildFile();
                var name = Path.GetFileNameWithoutExtension(args[1]) +
                           ".blueprint";
                var path =
                    Path.Combine(
                        pars.Profile
                            ? Configuration.BlueprintLocalFolder
                            : Configuration.BlueprintGlobalFolder, name);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllLines(path, lines);
                args.Context.AddString(
                    $"Json Blueprint saved to {path.Replace("\\", "\\\\")} (pos: {HammerHelper.PrintXZY(bp.Coordinates)} rot: {HammerHelper.PrintYXZ(bp.Rotation)}).");
                Selection.CreateGhost(new ObjectSelection(args.Context, bp,
                    Vector3.one));
            });
    }
}

public class HammerSaveParsJson
{
    public string CenterPiece = Configuration.BlueprintCenterPiece;
    public string SnapPiece = Configuration.BlueprintSnapPiece;
    public bool SaveData = Configuration.SaveBlueprintData;
    public bool Profile = Configuration.SaveBlueprintsToProfile;

    public HammerSaveParsJson(Terminal.ConsoleEventArgs args)
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
                if (index == 1) SnapPiece = par;
                continue;
            }

            if (split[0] == "center" || split[0] == "c")
                CenterPiece = split[1];
            if (split[0] == "snap" || split[0] == "s")
                SnapPiece = split[1];
            if (split[0] == "data" || split[0] == "d")
                SaveData = Parse.BoolNull(split[1]) ??
                           Configuration.SaveBlueprintData;
            if (split[0] == "profile" || split[0] == "p")
                Profile = Parse.BoolNull(split[1]) ??
                          Configuration.SaveBlueprintsToProfile;
        }
    }
}
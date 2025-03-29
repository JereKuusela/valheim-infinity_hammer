using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Argo.Blueprint;
using Data;
using Microsoft.Win32.SafeHandles;
using ServerDevcommands;
using Service;
using UnityEngine;
using UnityEngine.Rendering;

namespace InfinityHammer;

public class HammerSaveCommandJson
{
    public HammerSaveCommandJson() {
        AutoComplete.Register( "hammer_save_json", (int index) => {
            if (index == 0) return ParameterInfo.Create( "File name." );
            return ["c", "center", "d", "data", "p", "profile", "s", "snap"];
        }, new() {
            { "c", (int       index) => ParameterInfo.ObjectIds },
            { "center", (int  index) => ParameterInfo.ObjectIds },
            { "d", (int       index) => ["true", "false"] },
            { "data", (int    index) => ["true", "false"] },
            { "p", (int       index) => ["true", "false"] },
            { "profile", (int index) => ["true", "false"] },
            { "s", (int       index) => ParameterInfo.ObjectIds },
            { "snap", (int    index) => ParameterInfo.ObjectIds },
        } );
        Helper.Command( "hammer_save_json",
            "[file name] [center=piece] [snap=piece] [data=true/false] [profile=true/false] - Saves the selection to a  json blueprint.",
            (args) => {
                HammerHelper.CheatCheck();
                Helper.ArgsCheck( args, 2, "Blueprint name is missing." );
                var                player         = Helper.GetPlayer();
                var                placementGhost = HammerHelper.GetPlacementGhost();
                HammerSaveParsJson pars           = new(args);

                var name = Path.GetFileNameWithoutExtension( args[1] ) +
                    ".blueprint.json";
                var path =
                    Path.Combine(
                        pars.Profile
                            ? Configuration.BlueprintLocalFolder
                            : Configuration.BlueprintGlobalFolder, name );
                Directory.CreateDirectory( Path.GetDirectoryName( path ) );
                //    File.Create(path);

                if (Selection.Get() is not ObjectSelection selection) {
                    throw new ArgumentException( "Selection error" );
                }
                Vector3 placement_pos = player.m_placementGhost.transform.position;
                try {
                    SelectedObjects selectedObjects
                        = new SelectedObjects( placementGhost, selection );
                    var bp = new BlueprintJson( player.GetPlayerName(), selectedObjects,
                        placement_pos,
                        pars.SnapPiece, pars.CenterPiece, pars.SaveData );
                    if (selection.Objects.Count() == 0) {
                        bp.BuildFromSelectionSingle();
                    }
                    // todo remove?
                    else {
                        bp.BuildFromSelection();
                    }
                    // todo write write funtion
               
                bp.AddExportListener( (() => {
                    bp.WriteToFile( path );
                    args.Context.AddString(
                        $"Json Blueprint saved to {path.Replace( "\\", "\\\\" )}" +
                        $" (pos: {HammerHelper.PrintXZY( bp.Coordinates )} "      +
                        $"rot: {HammerHelper.PrintYXZ( bp.Rotation )})." );
                    Selection.CreateGhost( new ObjectSelection( args.Context,
                        bp,
                        Vector3.one ) );
                }) );
                } catch (Exception e) {
                    System.Console.WriteLine( "Error inhammer_save_json " + e );
                }
            } );
    }
}

public class HammerSaveParsJson
{
    public string CenterPiece = Configuration.BlueprintCenterPiece;
    public string SnapPiece   = Configuration.BlueprintSnapPiece;
    public bool   SaveData    = Configuration.SaveBlueprintData;
    public bool   Profile     = Configuration.SaveBlueprintsToProfile;

    public HammerSaveParsJson(Terminal.ConsoleEventArgs args) {
        var pars  = args.Args.Skip( 2 ).ToArray();
        int index = 0;
        foreach (var par in pars) {
            var split = par.Split( '=' );
            if (split.Length < 2) {
                // Legacy support.
                if (index == 0) CenterPiece = par;
                if (index == 1) SnapPiece   = par;
                continue;
            }

            if (split[0] == "center" || split[0] == "c")
                CenterPiece = split[1];
            if (split[0] == "snap" || split[0] == "s")
                SnapPiece = split[1];
            if (split[0] == "data" || split[0] == "d")
                SaveData = Parse.BoolNull( split[1] ) ??
                    Configuration.SaveBlueprintData;
            if (split[0] == "profile" || split[0] == "p")
                Profile = Parse.BoolNull( split[1] ) ??
                    Configuration.SaveBlueprintsToProfile;
        }
    }
}
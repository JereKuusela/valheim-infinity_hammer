using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Argo.Blueprint;
using Argo.Blueprint.Jobs;
using Argo.Blueprint.Util;
using Argo.DataAnalysis;
using Argo.Zdo;
using Data;
using Microsoft.Win32.SafeHandles;
using ServerDevcommands;
using Service;
using UnityEngine;
using UnityEngine.Rendering;
using ArgoRegister = Argo.Registers.SettingsRegister;
using SaveExtraData  = Argo.Registers.SaveExtraData;
namespace InfinityHammer;

public class HammerSaveCommandJ2
{
    public HammerSaveCommandJ2() {
        AutoComplete.Register("hammer_save_j2", (int index) => {
            if (index == 0) return ParameterInfo.Create("File name.");
            return ["c", "center", "d", "data", "p", "profile", "s", "snap"];
        }, new() {
            { "c", (int index) => ParameterInfo.ObjectIds },
            { "center", (int index) => ParameterInfo.ObjectIds },
            { "d", (int index) => ["true", "false"] },
            { "data", (int index) => ["true", "false"] },
            { "p", (int index) => ["true", "false"] },
            { "profile", (int index) => ["true", "false"] },
            { "s", (int index) => ParameterInfo.ObjectIds },
            { "snap", (int index) => ParameterInfo.ObjectIds },
        });
        Helper.Command("hammer_save_j2",
            "[file name] [center=piece] [snap=piece] [data=true/false] [profile=true/false] - Saves the selection to a  json blueprint.",
            (args) => {
                HammerHelper.CheatCheck();
                Helper.ArgsCheck(args, 2, "Blueprint name is missing.");
                var              player         = Helper.GetPlayer();
                var              placementGhost = HammerHelper.GetPlacementGhost();
                ArgoRegister config         = GetConfig(args, "InfintyHammer");

                var name = Path.GetFileNameWithoutExtension(args[1]) +
                    ".blueprint.json";
                var path =
                    Path.Combine(
                        config.SaveToProfile
                            ? Configuration.BlueprintLocalFolder
                            : Configuration.BlueprintGlobalFolder, name);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                //    File.Create(path);

               
                Vector3 placement_pos = player.m_placementGhost.transform.position;

                try {
                    EntityStore store = new();
                    JobManager  jobs  = new();
                
                    ImportSelectionJob importJob;
                    JsonExportJob      exportJob;
                    string             bp_name;
                    {
                        if (Selection.Get() is not ObjectSelection selection) {
                            throw new ArgumentException("Selection error");
                        }
                        MultiSelection selectedObjects
                            = ArgoExtensions.CreateMultiSelection(placementGhost, selection, player, config);
                        bp_name   = selectedObjects.Name;
                        importJob = new ImportSelectionJob(store, selectedObjects, config);
                        exportJob = new JsonExportJob(store,
                            config, // todo since we construct a new entitystore here we can get all bp names from it
                            new LazyGetter<MultiSelection, List<string>>(selectedObjects, (x) => x.Blueprints),
                            importJob);

                        exportJob.AddListener((() => {
                                    // Entity Getblueprint() => store.GetBlueprint(selectedObjects.Name);
                                    //  string name   = selectedObjects.Name;
                                    var getter = new LazyGetter<MultiSelection, MultiSelection>(selectedObjects,
                                        (selection_) => {
                                            return selection_;
                                        });
                                    var selection = getter.Get();
                                    selection.Clear(); // test if creating new selection actually works
                                }
                            ));
                    }
                    var writeJob = new IoWriteJob(path, exportJob);
                    var selectionJob
                        = new ExportToSelectionJob(store, config, new List<string> { bp_name });

                    jobs.Add(importJob, exportJob, writeJob, selectionJob);
                    jobs.AddListener((() => {
                        // Entity Getblueprint() => store.GetBlueprint(selectedObjects.Name);
                        //  string name   = selectedObjects.Name;
                        var getter = new LazyGetter<EntityStore, bool>(store, (store_) => {
                                Entity Getblueprint() => store_.GetBlueprint(bp_name);
                                args.Context.AddString(
                                    $"Json Blueprint saved to {path.Replace("\\", "\\\\")}"                   +
                                    $" (pos: {HammerHelper.PrintXZY(store_.GetBlueprint(bp_name).Position)} " +
                                    $"rot: {HammerHelper.PrintYXZ(store_.GetBlueprint(bp_name).Rotation.eulerAngles)}).");
                                return true;
                            }
                        );
                        getter.Get();
                    }));

                    //  Entity blueprint    = store.GetBlueprint(selectedObjects.Name);

                    jobs.UnityRunSequential();
                } catch (Exception e) {
                    System.Console.WriteLine("Error inhammer_save_j2 " + e);
                }
            });
    }

    public ArgoRegister GetConfig(Terminal.ConsoleEventArgs args, string name = "",
        ArgoRegister? cfg_ = null) {
        ArgoRegister cfg;
        if (cfg_ == null) {
            if (name != "") {
                if (!ArgoRegister.TryGetConfig(name, out cfg)) {
                    cfg = ArgoRegister.GetDefaultInstance().Clone(name);
                }
            } else {
                cfg = ArgoRegister.GetDefaultInstance();
            }
        } else {
            cfg = cfg_;
        }
        cfg.CenterPiece   = Configuration.BlueprintCenterPiece;
        cfg.SnapPiece     = Configuration.BlueprintSnapPiece;
        cfg.SaveMode      = Configuration.SaveBlueprintData ? SaveExtraData.All : SaveExtraData.None;
        cfg.SaveToProfile = Configuration.SaveBlueprintsToProfile;

        var pars  = args.Args.Skip(2).ToArray();
        int index = 0;
        foreach (var par in pars) {
            var split = par.Split('=');
            if (split.Length < 2) {
                // Legacy support.
                if (index == 0) cfg.CenterPiece = par;
                if (index == 1) cfg.SnapPiece   = par;
                continue;
            }

            if (split[0] == "center" || split[0] == "c")
                cfg.CenterPiece = split[1];
            if (split[0] == "snap" || split[0] == "s")
                cfg.SnapPiece = split[1];
            if (split[0] == "data" || split[0] == "d") {
                bool? extradata = Parse.BoolNull(split[1]);
                if (extradata.HasValue) {
                    cfg.SaveMode = extradata.Value ? SaveExtraData.All : SaveExtraData.None;
                }
            }
            if (split[0] == "profile" || split[0] == "p")
                cfg.SaveToProfile = Parse.BoolNull(split[1]) ??
                    Configuration.SaveBlueprintsToProfile;
        }
        return cfg;
    }
}
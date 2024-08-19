using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ServerDevcommands;
using Service;
using UnityEngine;
using UnityEngine.Rendering;
namespace InfinityHammer;


public class HammerSaveCommand
{

  private static string GetExtraInfo(GameObject obj, ZDOData data)
  {
    var info = "";
    if (obj.GetComponent<Sign>())
      info = data.GetString(ZDOVars.s_text, "");
    if (obj.GetComponent<TeleportWorld>())
      info = data.GetString(ZDOVars.s_tag, "");
    if (obj.GetComponent<Tameable>())
      info = data.GetString(ZDOVars.s_tamedName, "");

    if (obj.GetComponent<ItemStand>() && data.GetString(ZDOVars.s_item) != "")
    {
      var variant = data.GetInt(ZDOVars.s_variant);
      var item = data.GetString(ZDOVars.s_item);
      if (variant != 0)
        info = $"{item}:{variant}";
      else
        info = $"{item}";
    }
    if (obj.TryGetComponent<ArmorStand>(out var armorStand))
    {
      info = $"{armorStand.m_pose}:";
      info += $"{armorStand.m_slots.Count}:";
      var slots = armorStand.m_slots.Select(slot => $"{slot.m_visualName}:{slot.m_visualVariant}");
      info += string.Join(":", slots);
    }
    return info;
  }
  private static Blueprint BuildBluePrint(Player player, GameObject obj, string centerPiece, string snapPiece, bool saveData)
  {
    Blueprint bp = new()
    {
      Name = Utils.GetPrefabName(obj),
      Creator = player.GetPlayerName(),
      Rotation = HammerHelper.GetPlacementGhost().transform.rotation.eulerAngles,
    };
    var piece = obj.GetComponent<Piece>();
    if (piece)
    {
      bp.Name = Localization.instance.Localize(piece.m_name);
    }
    if (Selection.Get() is not ObjectSelection selection) return bp;
    var objects = Snapping.GetChildren(obj);
    if (selection.Objects.Count() == 1)
    {
      AddSingleObject(bp, obj, saveData);
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
          AddObject(bp, tr, saveData, i);
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

  private static void AddSingleObject(Blueprint bp, GameObject obj, bool saveData)
  {
    var name = Utils.GetPrefabName(obj);
    var save = saveData || Configuration.SavedObjectData.Contains(name.ToLowerInvariant());
    var data = save ? Selection.Get().GetData() : null;
    var info = data == null ? "" : GetExtraInfo(obj, data);
    bp.Objects.Add(new BlueprintObject(name, Vector3.zero, Quaternion.identity, obj.transform.localScale, info, data?.Save(), 1f));
  }
  private static void AddObject(Blueprint bp, GameObject obj, bool saveData, int index = 0)
  {
    var name = Utils.GetPrefabName(obj);
    var save = saveData || Configuration.SavedObjectData.Contains(name.ToLowerInvariant());
    var data = save ? Selection.Get().GetData(index) : null;
    var info = data == null ? "" : GetExtraInfo(obj, data);
    bp.Objects.Add(new BlueprintObject(name, obj.transform.localPosition, obj.transform.localRotation, obj.transform.localScale, info, data?.Save(), 1f));
  }
  private static string[] GetPlanBuildFile(Blueprint bp)
  {
    if (Configuration.SimplerBlueprints)
      return [
      $"#Center:{bp.CenterPiece}",
      $"#Pieces",
      .. bp.Objects.Select(GetPlanBuildObject),
    ];
    return [
      $"#Name:{bp.Name}",
      $"#Creator:{bp.Creator}",
      $"#Description:{bp.Description}",
      $"#Category:InfinityHammer",
      $"#Center:{bp.CenterPiece}",
      $"#Coordinates:{HammerHelper.PrintXZY(bp.Coordinates)}",
      $"#Rotation:{HammerHelper.PrintYXZ(bp.Rotation)}",
      $"#SnapPoints",
      .. bp.SnapPoints.Select(GetPlanBuildSnapPoint),
      $"#Pieces",
      .. bp.Objects.Select(GetPlanBuildObject),
    ];
  }
  private static string GetPlanBuildSnapPoint(Vector3 pos)
  {
    var x = HammerHelper.Format(pos.x);
    var y = HammerHelper.Format(pos.y);
    var z = HammerHelper.Format(pos.z);
    return $"{x};{y};{z}";
  }
  private static string GetPlanBuildObject(BlueprintObject obj)
  {
    var name = obj.Prefab;
    var posX = HammerHelper.Format(obj.Pos.x);
    var posY = HammerHelper.Format(obj.Pos.y);
    var posZ = HammerHelper.Format(obj.Pos.z);
    var rotX = HammerHelper.Format(obj.Rot.x);
    var rotY = HammerHelper.Format(obj.Rot.y);
    var rotZ = HammerHelper.Format(obj.Rot.z);
    var rotW = HammerHelper.Format(obj.Rot.w);
    var scaleX = HammerHelper.Format(obj.Scale.x);
    var scaleY = HammerHelper.Format(obj.Scale.y);
    var scaleZ = HammerHelper.Format(obj.Scale.z);
    var info = obj.ExtraInfo;
    var data = "";
    if (obj.Data != null)
    {
      data = obj.Data.GetBase64();
      if (data == "AAAAAA==") data = "";
    }
    return $"{name};;{posX};{posY};{posZ};{rotX};{rotY};{rotZ};{rotW};{info};{scaleX};{scaleY};{scaleZ};{data}";
  }

  public HammerSaveCommand()
  {
    AutoComplete.Register("hammer_save", (int index) =>
    {
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
    Helper.Command("hammer_save", "[file name] [center=piece] [snap=piece] [data=true/false] [profile=true/false] - Saves the selection to a blueprint.", (args) =>
    {
      HammerHelper.CheatCheck();
      Helper.ArgsCheck(args, 2, "Blueprint name is missing.");
      var player = Helper.GetPlayer();
      var ghost = HammerHelper.GetPlacementGhost();
      HammerSavePars pars = new(args);
      var bp = BuildBluePrint(player, ghost, pars.CenterPiece, pars.SnapPiece, pars.SaveData);
      var lines = GetPlanBuildFile(bp);
      var name = Path.GetFileNameWithoutExtension(args[1]) + ".blueprint";
      var path = Path.Combine(pars.Profile ? Configuration.BlueprintLocalFolder : Configuration.BlueprintGlobalFolder, name);
      Directory.CreateDirectory(Path.GetDirectoryName(path));
      File.WriteAllLines(path, lines);
      args.Context.AddString($"Blueprint saved to {path.Replace("\\", "\\\\")} (pos: {HammerHelper.PrintXZY(bp.Coordinates)} rot: {HammerHelper.PrintYXZ(bp.Rotation)}).");
      Selection.CreateGhost(new ObjectSelection(args.Context, bp, Vector3.one));
    });
  }
}

public class HammerSavePars
{
  public string CenterPiece = Configuration.BlueprintCenterPiece;
  public string SnapPiece = Configuration.BlueprintSnapPiece;
  public bool SaveData = Configuration.SaveBlueprintData;
  public bool Profile = Configuration.SaveBlueprintsToProfile;

  public HammerSavePars(Terminal.ConsoleEventArgs args)
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
        SaveData = split[1] == "true";
      if (split[0] == "profile" || split[0] == "p")
        Profile = split[1] == "true";
    }
  }
}

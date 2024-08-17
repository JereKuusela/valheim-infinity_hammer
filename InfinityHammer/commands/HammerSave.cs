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
    var objects = GetObjects(obj);
    if (selection.Objects.Count() == 1)
    {
      AddSingleObject(bp, obj, saveData);
      // Snap points are sort of useful for single objects.
      // Since single objects should just have custom data but otherwise the original behavior.
      var snaps = GetSnapPoints(obj);
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
      if (snapPiece == "all" || snapPiece == "auto")
      {
        foreach (var tr in objects)
        {
          for (int c = 0; c < tr.transform.childCount; c++)
          {
            Transform child = tr.transform.GetChild(c);
            if (child.CompareTag("snappoint"))
              bp.SnapPoints.Add(tr.transform.localPosition + child.transform.position - tr.transform.position);
          }
        }
      }
    }
    var offset = bp.Center(centerPiece);
    if (snapPiece == "auto" && bp.SnapPoints.Count > 0)
      AutoSnap(bp);
    bp.Coordinates = player.m_placementGhost.transform.position - offset;
    return bp;
  }

  private static void AutoSnap(Blueprint bp)
  {
    float minLeft = float.MaxValue;
    float minRight = float.MinValue;
    float minFront = float.MaxValue;
    float minBack = float.MinValue;
    float minTop = float.MaxValue;
    float minBottom = float.MinValue;
    float left = 0;
    float right = 0;
    float front = 0;
    float back = 0;
    float top = 0;
    float bottom = 0;
    List<Vector3> lefts = [];
    List<Vector3> rights = [];
    List<Vector3> fronts = [];
    List<Vector3> backs = [];
    List<Vector3> tops = [];
    List<Vector3> bottoms = [];
    foreach (var pos in bp.SnapPoints)
    {
      if (Helper.Approx(pos.x, left))
      {
        if (Helper.Approx(pos.sqrMagnitude, minLeft))
        {
          lefts.Add(pos);
        }
        else if (pos.sqrMagnitude < minLeft)
        {
          minLeft = pos.sqrMagnitude;
          lefts = [pos];
        }
      }
      else if (pos.x < left)
      {
        left = pos.x;
        minLeft = pos.sqrMagnitude;
        lefts = [pos];
      }

      if (Helper.Approx(pos.x, right))
      {
        if (Helper.Approx(pos.sqrMagnitude, minRight))
        {
          rights.Add(pos);
        }
        else if (pos.sqrMagnitude < minRight)
        {
          minRight = pos.sqrMagnitude;
          rights = [pos];
        }
      }
      else if (pos.x > right)
      {
        right = pos.x;
        minRight = pos.sqrMagnitude;
        rights = [pos];
      }

      if (Helper.Approx(pos.z, front))
      {
        if (Helper.Approx(pos.sqrMagnitude, minFront))
        {
          fronts.Add(pos);
        }
        else if (pos.sqrMagnitude < minFront)
        {
          minFront = pos.sqrMagnitude;
          fronts = [pos];
        }
      }
      else if (pos.z < front)
      {
        front = pos.z;
        minFront = pos.sqrMagnitude;
        fronts = [pos];
      }

      if (Helper.Approx(pos.z, back))
      {
        if (Helper.Approx(pos.sqrMagnitude, minBack))
        {
          backs.Add(pos);
        }
        else if (pos.sqrMagnitude < minBack)
        {
          minBack = pos.sqrMagnitude;
          backs = [pos];
        }
      }
      else if (pos.z > back)
      {
        back = pos.z;
        minBack = pos.sqrMagnitude;
        backs = [pos];
      }

      if (Helper.Approx(pos.y, top))
      {
        if (Helper.Approx(pos.sqrMagnitude, minTop))
        {
          tops.Add(pos);
        }
        else if (pos.sqrMagnitude < minTop)
        {
          minTop = pos.sqrMagnitude;
          tops = [pos];
        }
      }
      else if (pos.y > top)
      {
        top = pos.y;
        minTop = pos.sqrMagnitude;
        tops = [pos];
      }

      if (Helper.Approx(pos.y, bottom))
      {
        if (Helper.Approx(pos.sqrMagnitude, minBottom))
        {
          bottoms.Add(pos);
        }
        else if (pos.sqrMagnitude < minBottom)
        {
          minBottom = pos.sqrMagnitude;
          bottoms = [pos];
        }
      }
      else if (pos.y < bottom)
      {
        bottom = pos.y;
        minBottom = pos.sqrMagnitude;
        bottoms = [pos];
      }


    }
    List<Vector3> all = [.. lefts, .. rights, .. fronts, .. backs, .. tops, .. bottoms];
    List<Vector3> unique = [];
    // Very inefficient but not many snap points.
    foreach (var pos in all)
    {
      if (unique.Any(p => Helper.Approx(p.x, pos.x) && Helper.Approx(p.y, pos.y) && Helper.Approx(p.z, pos.z))) continue;
      unique.Add(pos);
    }
    bp.SnapPoints = unique;
  }
  private static List<GameObject> GetObjects(GameObject obj)
  {
    List<GameObject> objects = [];
    foreach (Transform tr in obj.transform)
    {
      if (HammerHelper.IsSnapPoint(tr.gameObject)) continue;
      objects.Add(tr.gameObject);
    }
    return objects;
  }
  private static List<GameObject> GetSnapPoints(GameObject obj)
  {
    List<GameObject> objects = [];
    foreach (Transform tr in obj.transform)
    {
      if (!HammerHelper.IsSnapPoint(tr.gameObject)) continue;
      objects.Add(tr.gameObject);
    }
    return objects;
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
      { "s", (int index) => ["auto", "all", ..ParameterInfo.ObjectIds] },
      { "snap", (int index) => ["auto", "all", ..ParameterInfo.ObjectIds] },
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

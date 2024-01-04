using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace InfinityHammer;


public class HammerSaveCommand
{

  private static string GetExtraInfo(GameObject obj, ZDOData data)
  {
    var info = "";
    if (obj.GetComponent<Sign>())
      info = data.GetString(Hash.Text, "");
    if (obj.GetComponent<TeleportWorld>())
      info = data.GetString(Hash.Tag, "");
    if (obj.GetComponent<Tameable>())
      info = data.GetString(Hash.TamedName, "");

    if (obj.GetComponent<ItemStand>() && data.GetString(Hash.Item) != "")
    {
      var item = data.GetString(Hash.Item);
      var variant = data.GetInt(Hash.Variant);
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
  private static Blueprint BuildBluePrint(Player player, GameObject obj, string centerPiece, bool saveData)
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
      bp.Description = piece.m_description;
    }
    var objects = GetObjects(obj);
    if (objects.Count() == 1)
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
        AddObject(bp, tr, saveData, i);
        i += 1;
      }
    }
    var offset = bp.Center(centerPiece);
    bp.Coordinates = player.m_placementGhost.transform.position - offset;
    return bp;
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
    var data = saveData ? Selection.Get().GetData() : null;
    var info = data == null ? "" : GetExtraInfo(obj, data);
    bp.Objects.Add(new BlueprintObject(Utils.GetPrefabName(obj), Vector3.zero, Quaternion.identity, obj.transform.localScale, info, data?.Save()));
  }
  private static void AddObject(Blueprint bp, GameObject obj, bool saveData, int index = 0)
  {
    var data = saveData ? Selection.Get().GetData(index) : null;
    var info = data == null ? "" : GetExtraInfo(obj, data);
    bp.Objects.Add(new BlueprintObject(Utils.GetPrefabName(obj), obj.transform.localPosition, obj.transform.localRotation, obj.transform.localScale, info, data?.Save()));
  }
  private static string[] GetPlanBuildFile(Blueprint bp)
  {
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
      if (index == 1) return ParameterInfo.ObjectIds;
      return null;
    });
    Helper.Command("hammer_save", "[file name] [center piece] - Saves the selection to a blueprint.", (args) =>
    {
      HammerHelper.CheatCheck();
      Helper.ArgsCheck(args, 2, "Blueprint name is missing.");
      var player = Helper.GetPlayer();
      var ghost = HammerHelper.GetPlacementGhost();
      var center = args.Length > 2 ? args[2] : "";
      var bp = BuildBluePrint(player, ghost, center, Configuration.SaveBlueprintData);
      var lines = GetPlanBuildFile(bp);
      var name = Path.GetFileNameWithoutExtension(args[1]) + ".blueprint";
      var path = Path.Combine(Configuration.SaveBlueprintsToProfile ? Configuration.BlueprintLocalFolder : Configuration.BlueprintGlobalFolder, name);
      Directory.CreateDirectory(Path.GetDirectoryName(path));
      File.WriteAllLines(path, lines);
      args.Context.AddString($"Blueprint saved to {path.Replace("\\", "\\\\")} (pos: {HammerHelper.PrintXZY(bp.Coordinates)} rot: {HammerHelper.PrintYXZ(bp.Rotation)}).");
      Selection.CreateGhost(new ObjectSelection(args.Context, bp, Vector3.one));
    });
  }
}

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Service;
using UnityEngine;
namespace InfinityHammer;


public class HammerSaveCommand {
  private static void Serialize(ZDO zdo, ZPackage pkg) {
    var num = 0;
    if (zdo.m_floats != null && zdo.m_floats.Count > 0)
      num |= 1;
    if (zdo.m_vec3 != null && zdo.m_vec3.Count > 0)
      num |= 2;
    if (zdo.m_quats != null && zdo.m_quats.Count > 0)
      num |= 4;
    if (zdo.m_ints != null && zdo.m_ints.Count > 0)
      num |= 8;
    if (zdo.m_strings != null && zdo.m_strings.Count > 0)
      num |= 16;
    if (zdo.m_longs != null && zdo.m_longs.Count > 0)
      num |= 64;
    if (zdo.m_byteArrays != null && zdo.m_byteArrays.Count > 0)
      num |= 128;

    pkg.Write(num);
    if (zdo.m_floats != null && zdo.m_floats.Count > 0) {
      pkg.Write((byte)zdo.m_floats.Count);
      foreach (KeyValuePair<int, float> keyValuePair in zdo.m_floats) {
        pkg.Write(keyValuePair.Key);
        pkg.Write(keyValuePair.Value);
      }
    }
    if (zdo.m_vec3 != null && zdo.m_vec3.Count > 0) {
      pkg.Write((byte)zdo.m_vec3.Count);
      foreach (KeyValuePair<int, Vector3> keyValuePair2 in zdo.m_vec3) {
        pkg.Write(keyValuePair2.Key);
        pkg.Write(keyValuePair2.Value);
      }
    }
    if (zdo.m_quats != null && zdo.m_quats.Count > 0) {
      pkg.Write((byte)zdo.m_quats.Count);
      foreach (KeyValuePair<int, Quaternion> keyValuePair3 in zdo.m_quats) {
        pkg.Write(keyValuePair3.Key);
        pkg.Write(keyValuePair3.Value);
      }
    }
    if (zdo.m_ints != null && zdo.m_ints.Count > 0) {
      pkg.Write((byte)zdo.m_ints.Count);
      foreach (KeyValuePair<int, int> keyValuePair4 in zdo.m_ints) {
        pkg.Write(keyValuePair4.Key);
        pkg.Write(keyValuePair4.Value);
      }
    }
    if (zdo.m_longs != null && zdo.m_longs.Count > 0) {
      pkg.Write((byte)zdo.m_longs.Count);
      foreach (KeyValuePair<int, long> keyValuePair5 in zdo.m_longs) {
        pkg.Write(keyValuePair5.Key);
        pkg.Write(keyValuePair5.Value);
      }
    }
    if (zdo.m_strings != null && zdo.m_strings.Count > 0) {
      pkg.Write((byte)zdo.m_strings.Count);
      foreach (KeyValuePair<int, string> keyValuePair6 in zdo.m_strings) {
        pkg.Write(keyValuePair6.Key);
        pkg.Write(keyValuePair6.Value);
      }
    }
    if (zdo.m_byteArrays != null && zdo.m_byteArrays.Count > 0) {
      pkg.Write((byte)zdo.m_byteArrays.Count);
      foreach (KeyValuePair<int, byte[]> keyValuePair7 in zdo.m_byteArrays) {
        pkg.Write(keyValuePair7.Key);
        pkg.Write(keyValuePair7.Value);
      }
    }
  }
  private static string GetExtraInfo(GameObject obj, ZDO zdo) {
    var info = "";
    if (obj.GetComponent<Sign>())
      info = zdo.GetString(Hash.Text, "");
    if (obj.GetComponent<TeleportWorld>())
      info = zdo.GetString(Hash.Tag, "");
    if (obj.GetComponent<Tameable>())
      info = zdo.GetString(Hash.TamedName, "");

    if (obj.GetComponent<ItemStand>() is { } itemStand && zdo?.GetString(Hash.Item) != "") {
      var item = zdo?.GetString(Hash.Item) ?? "";
      var variant = zdo?.GetInt(Hash.Variant) ?? 0;
      if (variant != 0)
        info = $"{item}:{variant}";
      else
        info = $"{item}";
    }
    if (obj.GetComponent<ArmorStand>() is { } armorStand) {
      info = $"{armorStand.m_pose}:";
      info += $"{armorStand.m_slots.Count}:";
      var slots = armorStand.m_slots.Select(slot => $"{slot.m_visualName}:{slot.m_visualVariant}");
      info += string.Join(":", slots);
    }
    return info;
  }
  private static void AddSingleObject(Blueprint bp, GameObject obj) {
    var zdo = Selection.GetData() ?? new();
    var info = GetExtraInfo(obj, zdo);
    bp.Objects.Add(new BlueprintObject(Utils.GetPrefabName(obj), Vector3.zero, Quaternion.identity, obj.transform.localScale, info, zdo));
  }
  private static void AddObject(Blueprint bp, GameObject obj, int index = 0) {
    var zdo = Selection.GetData(index) ?? new();
    var info = GetExtraInfo(obj, zdo);
    bp.Objects.Add(new BlueprintObject(Utils.GetPrefabName(obj), obj.transform.localPosition, obj.transform.localRotation, obj.transform.localScale, info, zdo));
  }
  private static Blueprint BuildBluePrint(Player player, GameObject obj) {
    Blueprint bp = new();
    bp.Name = Utils.GetPrefabName(obj);
    bp.Creator = player.GetPlayerName();
    var piece = obj.GetComponent<Piece>();
    if (piece) {
      bp.Name = Localization.instance.Localize(piece.m_name);
      bp.Description = piece.m_description;
    }
    if (Selection.Type == SelectedType.Object) {
      AddSingleObject(bp, obj);
      foreach (Transform child in obj.transform) {
        if (Helper.IsSnapPoint(child.gameObject))
          bp.SnapPoints.Add(child.localPosition);
      }
    }
    if (Selection.Type == SelectedType.Multiple || Selection.Type == SelectedType.Location) {
      var i = 0;
      foreach (Transform tr in obj.transform) {
        if (Helper.IsSnapPoint(tr.gameObject))
          bp.SnapPoints.Add(tr.localPosition);
        else {
          AddObject(bp, tr.gameObject, i);
          i += 1;
        }
      }
    }
    return bp;
  }

  private static string[] GetPlanBuildFile(Blueprint bp) {
    List<string> lines = new();
    lines.Add($"#Name:{bp.Name}");
    lines.Add($"#Creator:{bp.Creator}");
    lines.Add($"#Description:{bp.Description}");
    lines.Add($"#Category: InfinityHammer");
    lines.Add($"#SnapPoints");
    lines.AddRange(bp.SnapPoints.Select(GetPlanBuildSnapPoint));
    lines.Add($"#Pieces");
    lines.AddRange(bp.Objects.Select(GetPlanBuildObject));
    return lines.ToArray();
  }
  private static string InvariantString(float f) {
    return f.ToString(NumberFormatInfo.InvariantInfo);
  }
  private static string GetPlanBuildSnapPoint(Vector3 pos) {
    var x = InvariantString(pos.x);
    var y = InvariantString(pos.y);
    var z = InvariantString(pos.z);
    return $"{x};{y};{z}";
  }
  private static string GetPlanBuildObject(BlueprintObject obj) {
    var name = obj.Prefab;
    var posX = InvariantString(obj.Pos.x);
    var posY = InvariantString(obj.Pos.y);
    var posZ = InvariantString(obj.Pos.z);
    var rotX = InvariantString(obj.Rot.x);
    var rotY = InvariantString(obj.Rot.y);
    var rotZ = InvariantString(obj.Rot.z);
    var rotW = InvariantString(obj.Rot.w);
    var scaleX = InvariantString(obj.Scale.x);
    var scaleY = InvariantString(obj.Scale.y);
    var scaleZ = InvariantString(obj.Scale.z);
    var info = obj.ExtraInfo;
    var data = "";
    if (obj.Data != null) {
      ZPackage pkg = new();
      Serialize(obj.Data, pkg);
      data = pkg.GetBase64();
      if (data == "AAAAAA==") data = "";
    }
    return $"{name};;{posX};{posY};{posZ};{rotX};{rotY};{rotZ};{rotW};{info};{scaleX};{scaleY};{scaleZ};{data}";
  }

  public HammerSaveCommand() {
    CommandWrapper.Register("hammer_save", (int index) => {
      if (index == 0) return CommandWrapper.Info("File name.");
      return null;
    });
    Helper.Command("hammer_save", "[file name] - Saves the selection to a blueprint.", (args) => {
      Helper.CheatCheck();
      Helper.ArgsCheck(args, 2, "Blueprint name is missing.");
      var player = Helper.GetPlayer();
      var ghost = Helper.GetPlacementGhost();
      var bp = BuildBluePrint(player, ghost);
      var lines = GetPlanBuildFile(bp);
      var name = Path.GetFileNameWithoutExtension(args[1]) + ".blueprint";
      var path = Path.Combine(Configuration.PlanBuildFolder, name);
      File.WriteAllLines(path, lines);
      args.Context.AddString($"Blueprint saved to {path}");
    });
  }
}

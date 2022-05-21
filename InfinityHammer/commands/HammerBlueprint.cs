using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
namespace InfinityHammer;
public class BlueprintObject {
  public string Prefab = "";
  public Vector3 Pos;
  public Quaternion Rot;
  public string ExtraInfo;
  public ZDO Data;
  public BlueprintObject(string name, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW, string info, ZDO data) {
    Prefab = name;
    Pos = new Vector3(posX, posY, posZ);
    Rot = new Quaternion(rotX, rotY, rotZ, rotW).normalized;
    ExtraInfo = info;
    Data = data;
  }
  public BlueprintObject(string name, Vector3 pos, Quaternion rot, string info, ZDO data) {
    Prefab = name;
    Pos = pos;
    Rot = rot.normalized;
    ExtraInfo = info;
    Data = data;
  }
}
public class Blueprint {
  public string Name = "";
  public string Description = "";
  public string Creator = "";
  public List<BlueprintObject> Objects = new();
  public List<Vector3> SnapPoints = new();
}
public class HammerBlueprintCommand {
  private static void PrintSelected(Terminal terminal, Blueprint blueprint) {
    if (Settings.DisableSelectMessages) return;
    Helper.AddMessage(terminal, $"Selected {blueprint.Name}.");
  }

  private static GameObject BuildObject(Terminal terminal, Blueprint blueprint) {
    var container = new GameObject();
    // Prevents children from disappearing.
    container.SetActive(false);
    container.name = blueprint.Name;
    var piece = container.AddComponent<Piece>();
    piece.m_name = blueprint.Name;
    piece.m_description = blueprint.Description;
    ZNetView.m_forceDisableInit = true;
    foreach (var item in blueprint.Objects) {
      try {
        var obj = Helper.SafeInstantiate(item.Prefab, container);
        obj.SetActive(true);
        obj.transform.localPosition = item.Pos;
        obj.transform.localRotation = item.Rot;

      } catch (InvalidOperationException e) {
        Helper.AddMessage(terminal, $"Warning: {e.Message}");
      }
    }
    foreach (var position in blueprint.SnapPoints) {
      GameObject obj = new() {
        name = "_snappoint",
        layer = LayerMask.NameToLayer("piece"),
        tag = "snappoint"
      };
      obj.SetActive(false);
      obj.transform.SetParent(obj.transform);
      obj.transform.localPosition = position;
    }
    ZNetView.m_forceDisableInit = false;
    return container;
  }
  private static ZDO[] BuildData(Blueprint blueprint) => blueprint.Objects.Select(obj => obj.Data).ToArray();
  private static IEnumerable<string> Files() {
    if (!Directory.Exists(Settings.PlanBuildFolder)) Directory.CreateDirectory(Settings.PlanBuildFolder);
    if (!Directory.Exists(Settings.BuildShareFolder)) Directory.CreateDirectory(Settings.BuildShareFolder);
    var planBuild = Directory.EnumerateFiles(Settings.PlanBuildFolder, "*.blueprint", SearchOption.AllDirectories);
    var buildShare = Directory.EnumerateFiles(Settings.BuildShareFolder, "*.vbuild", SearchOption.AllDirectories);
    return planBuild.Concat(buildShare).OrderBy(s => s);
  }
  private static List<string> GetBlueprints() => Files().Select(path => Path.GetFileNameWithoutExtension(path).Replace(" ", "_")).ToList();
  private static Blueprint GetBluePrint(string name) {
    var path = Files().FirstOrDefault(path => Path.GetFileNameWithoutExtension(path).Replace(" ", "_") == name);
    if (path == null) throw new InvalidOperationException("Blueprint not found.");
    var rows = File.ReadAllLines(path);
    var extension = Path.GetExtension(path);
    Blueprint bp = new() { Name = name };
    if (extension == ".vbuild") return GetBuildShare(bp, rows);
    if (extension == ".blueprint") return GetPlanBuild(bp, rows);
    throw new InvalidOperationException("Unknown file format.");
  }
  private static Blueprint GetPlanBuild(Blueprint bp, string[] rows) {
    var piece = true;
    foreach (var row in rows) {
      if (row.StartsWith("#name:", StringComparison.OrdinalIgnoreCase))
        bp.Name = row.Split(':')[1];
      else if (row.StartsWith("#description:", StringComparison.OrdinalIgnoreCase))
        bp.Description = row.Split(':')[1];
      else if (row.StartsWith("#snappoints", StringComparison.OrdinalIgnoreCase))
        piece = false;
      else if (row.StartsWith("#pieces", StringComparison.OrdinalIgnoreCase))
        piece = true;
      else if (row.StartsWith("#", StringComparison.Ordinal))
        continue;
      else if (piece)
        bp.Objects.Add(GetPlanBuildObject(row));
      else
        bp.SnapPoints.Add(GetPlanBuildSnapPoint(row));
    }
    return bp;
  }
  private static void Deserialize(ZDO zdo, ZPackage pkg) {
    int num = pkg.ReadInt();
    if ((num & 1) != 0) {
      zdo.InitFloats();
      int num2 = (int)pkg.ReadByte();
      for (int i = 0; i < num2; i++) {
        int key = pkg.ReadInt();
        zdo.m_floats[key] = pkg.ReadSingle();
      }
    } else {
      zdo.ReleaseFloats();
    }
    if ((num & 2) != 0) {
      zdo.InitVec3();
      int num3 = (int)pkg.ReadByte();
      for (int j = 0; j < num3; j++) {
        int key2 = pkg.ReadInt();
        zdo.m_vec3[key2] = pkg.ReadVector3();
      }
    } else {
      zdo.ReleaseVec3();
    }
    if ((num & 4) != 0) {
      zdo.InitQuats();
      int num4 = (int)pkg.ReadByte();
      for (int k = 0; k < num4; k++) {
        int key3 = pkg.ReadInt();
        zdo.m_quats[key3] = pkg.ReadQuaternion();
      }
    } else {
      zdo.ReleaseQuats();
    }
    if ((num & 8) != 0) {
      zdo.InitInts();
      int num5 = (int)pkg.ReadByte();
      for (int l = 0; l < num5; l++) {
        int key4 = pkg.ReadInt();
        zdo.m_ints[key4] = pkg.ReadInt();
      }
    } else {
      zdo.ReleaseInts();
    }
    if ((num & 64) != 0) {
      zdo.InitLongs();
      int num6 = (int)pkg.ReadByte();
      for (int m = 0; m < num6; m++) {
        int key5 = pkg.ReadInt();
        zdo.m_longs[key5] = pkg.ReadLong();
      }
    } else {
      zdo.ReleaseLongs();
    }
    if ((num & 16) != 0) {
      zdo.InitStrings();
      int num7 = (int)pkg.ReadByte();
      for (int n = 0; n < num7; n++) {
        int key6 = pkg.ReadInt();
        zdo.m_strings[key6] = pkg.ReadString();
      }
    } else {
      zdo.ReleaseStrings();
    }
    if ((num & 128) != 0) {
      zdo.InitByteArrays();
      int num8 = (int)pkg.ReadByte();
      for (int num9 = 0; num9 < num8; num9++) {
        int key7 = pkg.ReadInt();
        zdo.m_byteArrays[key7] = pkg.ReadByteArray();
      }
      return;
    }
    zdo.ReleaseByteArrays();
  }
  private static BlueprintObject GetPlanBuildObject(string row) {
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
    var data = split.Length > 10 ? split[10] : "";
    ZDO zdo = new();
    if (data != "") {
      ZPackage pkg = new(data);
      Deserialize(zdo, pkg);
    }
    return new BlueprintObject(name, posX, posY, posZ, rotX, rotY, rotZ, rotW, info, zdo);
  }
  private static Vector3 GetPlanBuildSnapPoint(string row) {
    if (row.IndexOf(',') > -1) row = row.Replace(',', '.');
    var split = row.Split(';');
    var x = InvariantFloat(split, 0);
    var y = InvariantFloat(split, 1);
    var z = InvariantFloat(split, 2);
    return new Vector3(x, y, z);
  }
  private static Blueprint GetBuildShare(Blueprint bp, string[] rows) {
    bp.Objects = rows.Select(GetBuildShareObject).ToList();
    return bp;
  }
  private static BlueprintObject GetBuildShareObject(string row) {
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
    return new BlueprintObject(name, posX, posY, posZ, rotX, rotY, rotZ, rotW, "", new());
  }
  private static float InvariantFloat(string[] row, int index) {
    if (index >= row.Length) return 0f;
    var s = row[index];
    if (string.IsNullOrEmpty(s)) return 0f;
    return float.Parse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
  }

  public HammerBlueprintCommand() {
    CommandWrapper.Register("hammer_blueprint", (int index, int subIndex) => GetBlueprints());
    Helper.Command("hammer_blueprint", "[blueprint file] - Selects the blueprint to be placed.", (args) => {
      Helper.CheatCheck();
      Helper.ArgsCheck(args, 2, "Blueprint name is missing.");
      Hammer.Equip();
      var blueprint = GetBluePrint(string.Join("_", args.Args.Skip(1)));
      var obj = BuildObject(args.Context, blueprint);
      var data = BuildData(blueprint);
      if (Hammer.SetBlueprint(Player.m_localPlayer, obj, data))
        PrintSelected(args.Context, blueprint);

    }, GetBlueprints);
  }
}

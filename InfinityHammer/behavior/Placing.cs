using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
// Code related to adding objects.
namespace InfinityHammer;
///<summary>Overrides the piece selection.</summary>
[HarmonyPatch(typeof(PieceTable), nameof(PieceTable.GetSelectedPiece))]
public class GetSelectedPiece {
  public static bool Prefix(ref Piece __result) {
    if (Selection.Ghost)
      __result = Selection.Ghost.GetComponent<Piece>();
    if (__result) return false;
    return true;
  }
}

///<summary>Selecting a piece normally removes the override.</summary>
[HarmonyPatch(typeof(Player), nameof(Player.SetSelectedPiece))]
public class SetSelectedPiece {
  public static void Prefix(Player __instance) {
    Hammer.RemoveSelection();
    __instance.SetupPlacementGhost();
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
public class PlacePiece {
  static GameObject GetPrefab(Piece obj) {
    var type = Selection.Type;
    if (type == SelectedType.Default) return obj.gameObject;
    var ghost = Helper.GetPlayer().m_placementGhost;
    if (!ghost) return obj.gameObject;
    var name = Utils.GetPrefabName(ghost);

    if (type == SelectedType.Object)
      return ZNetScene.instance.GetPrefab(name);
    if (type == SelectedType.Location)
      return ZoneSystem.instance.m_locationProxyPrefab;
    if (type == SelectedType.Multiple) {
      var dummy = new GameObject();
      dummy.name = "Blueprint";
      return dummy;
    }
    return obj.gameObject;
  }
  static void Postprocess(GameObject obj) {
    var player = Helper.GetPlayer();
    Helper.EnsurePiece(obj);
    var ghost = Helper.GetPlayer().m_placementGhost;
    if (!ghost) return;
    var piece = obj.GetComponent<Piece>();
    if (Selection.Type == SelectedType.Command) {
      var scale = Scaling.Command;
      var shape = Ruler.GetShape();
      var x = ghost.transform.position.x.ToString(CultureInfo.InvariantCulture);
      var y = ghost.transform.position.y.ToString(CultureInfo.InvariantCulture);
      var z = ghost.transform.position.z.ToString(CultureInfo.InvariantCulture);
      var radius = scale.X.ToString(CultureInfo.InvariantCulture);
      var width = scale.X.ToString(CultureInfo.InvariantCulture);
      var depth = scale.Z.ToString(CultureInfo.InvariantCulture);
      if (shape != RulerShape.Rectangle)
        depth = width;
      var height = scale.Y.ToString(CultureInfo.InvariantCulture);
      var angle = ghost.transform.rotation.eulerAngles.y.ToString(CultureInfo.InvariantCulture);

      var command = Selection.Command;
      var multiShape = command.Contains("#r") && (command.Contains("#w") || command.Contains("#d"));
      if (multiShape) {
        var circle = shape == RulerShape.Circle;
        var args = command.Split(' ').ToList();
        for (var i = args.Count - 1; i > -1; i--) {
          if (circle && (args[i].Contains("#w") || args[i].Contains("#d")))
            args.RemoveAt(i);
          if (!circle && args[i].Contains("#r"))
            args.RemoveAt(i);
        }
        command = string.Join(" ", args);
      }
      command = command.Replace("#r", radius);
      command = command.Replace("#d", depth);
      command = command.Replace("#w", width);
      command = command.Replace("#a", angle);
      command = command.Replace("#x", x);
      command = command.Replace("#y", y);
      command = command.Replace("#z", z);
      command = command.Replace("#h", height);
      if (!Configuration.DisableMessages)
        Console.instance.AddString($"Hammering command: {command}");
      Console.instance.TryRunCommand(command);
      UnityEngine.Object.Destroy(obj);
      return;
    }
    if (Selection.Type == SelectedType.Multiple) {
      UndoHelper.StartTracking();
      for (var i = 0; i < ghost.transform.childCount; i++) {
        var ghostObj = ghost.transform.GetChild(i).gameObject;
        var name = Utils.GetPrefabName(ghostObj);
        var prefab = ZNetScene.instance.GetPrefab(name);
        if (prefab) {
          var childObj = UnityEngine.Object.Instantiate(prefab, ghostObj.transform.position, ghostObj.transform.rotation);
          var childView = childObj.GetComponent<ZNetView>();
          Hammer.CopyState(childView, i);
          Hammer.PostProcessPlaced(childObj);
          Scaling.SetPieceScale(childView, ghost);
        }
      }
      UndoHelper.StopTracking();
      UnityEngine.Object.Destroy(obj);
      return;
    }
    var view = obj.GetComponent<ZNetView>();
    // Hoe adds pieces too.
    if (!view) return;
    if (Selection.Type == SelectedType.Location && obj.GetComponent<LocationProxy>()) {
      UndoHelper.StartTracking();
      Hammer.SpawnLocation(view);
      UndoHelper.StopTracking();
    } else {
      Hammer.CopyState(view);
      Hammer.PostProcessPlaced(piece.gameObject);
      Scaling.SetPieceScale(view, ghost);
      UndoHelper.CreateObject(piece.gameObject);
    }
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(
                  OpCodes.Callvirt,
                  AccessTools.PropertyGetter(typeof(Component), nameof(Component.gameObject))))
          .SetAndAdvance(OpCodes.Call, Transpilers.EmitDelegate<Func<Piece, GameObject>>(GetPrefab).operand)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4_1),
              new CodeMatch(OpCodes.Ret))
          .Advance(-1)
          .Insert(new CodeInstruction(OpCodes.Ldloc_3),
            new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate<Action<GameObject>>(Postprocess).operand))
          .InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
public class PostProcessToolOnPlace {
  public static void Postfix(Player __instance, ref bool __result) {
    if (__result) Hammer.PostProcessTool(__instance);
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
public class UnlockBuildDistance {
  public static void Prefix(Player __instance, ref float __state) {
    __state = __instance.m_maxPlaceDistance;
    if (Configuration.BuildRange > 0f)
      __instance.m_maxPlaceDistance = Configuration.BuildRange;
    if (Selection.Type == SelectedType.Command)
      __instance.m_maxPlaceDistance = 1000f;
  }
  public static void Postfix(Player __instance, float __state) {
    __instance.m_maxPlaceDistance = __state;
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.SetupPlacementGhost))]
public class SetupPlacementGhost {
  public static void Postfix(Player __instance) {
    if (!__instance.m_placementGhost) return;
    // Ensures that the scale is reseted when selecting objects from the build menu.
    Scaling.Build.SetScale(__instance.m_placementGhost.transform.localScale);
    // When copying an existing object, the copy is inactive.
    // So the ghost must be manually activated while disabling ZNet stuff.
    if (__instance.m_placementGhost && !__instance.m_placementGhost.activeSelf) {
      ZNetView.m_forceDisableInit = true;
      __instance.m_placementGhost.SetActive(true);
      ZNetView.m_forceDisableInit = false;
    }
  }
}
[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
public class UpdatePlacementGhost {
  static void Postfix(Player __instance) {
    Scaling.Set(__instance.m_placementGhost);
    var marker = __instance.m_placementMarkerInstance;
    if (marker) {
      // Max 2 to only affect default game markers.
      for (var i = 0; i < marker.transform.childCount && i < 2; i++)
        marker.transform.GetChild(i).gameObject.SetActive(!Configuration.HidePlacementMarker);
    }
  }
}
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnLocation))]
public class CustomizeSpawnLocation {
  public static bool? RandomDamage = null;
  public static bool AllViews = false;
  static void Customize() {
    if (RandomDamage.HasValue) {
      WearNTear.m_randomInitialDamage = RandomDamage.Value;
    }
    if (AllViews) {
      var data = Selection.GetData();
      if (data != null) {
        var location = ZoneSystem.instance.GetLocation(data.GetInt(Hash.Location, 0));
        if (location != null) {
          foreach (var view in location.m_netViews)
            view.gameObject.SetActive(true);
        }

      }
    }
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(
                  OpCodes.Stsfld,
                  AccessTools.Field(typeof(WearNTear), nameof(WearNTear.m_randomInitialDamage))))
          .Advance(1)
          .Insert(new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate<Action>(Customize).operand))
          .InstructionEnumeration();
  }
}

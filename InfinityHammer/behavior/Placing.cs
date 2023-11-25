using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Service;
using UnityEngine;
// Code related to adding objects.
namespace InfinityHammer;
///<summary>Overrides the piece selection.</summary>
[HarmonyPatch(typeof(PieceTable), nameof(PieceTable.GetSelectedPiece))]
public class GetSelectedPiece
{
  public static bool Prefix(ref Piece __result)
  {
    if (Selection.Ghost)
      __result = Selection.Ghost.GetComponent<Piece>();
    return !__result;
  }
}

///<summary>Selecting a piece normally removes the override.</summary>
[HarmonyPatch(typeof(Player), nameof(Player.SetSelectedPiece), typeof(Vector2Int))]
public class SetSelectedPiece
{
  public static void Prefix(Player __instance)
  {
    Hammer.RemoveSelection();
    __instance.SetupPlacementGhost();
  }
}
[HarmonyPatch(typeof(Player), nameof(Player.SetSelectedPiece), typeof(Piece))]
public class SetSelectedPiece2
{
  public static void Prefix(Player __instance)
  {
    Hammer.RemoveSelection();
    __instance.SetupPlacementGhost();
  }
}


[HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
public class PlacePiece
{
  private static bool Clear = false;
  static void Prefix()
  {
    HideEffects.Active = true;
    Clear = Selection.IsSingleUse();
  }
  static void Finalizer(bool __result)
  {
    HideEffects.Active = false;
    DataHelper.Clear();
    if (__result && Clear)
    {
      Selection.Clear();
      Hammer.Clear();
    }
  }
  static GameObject GetPrefab(GameObject obj)
  {
    if (!Configuration.Enabled) return obj;
    var type = Selection.Type;
    var ghost = Helper.GetPlayer().m_placementGhost;
    if (!ghost) return obj;
    var name = Utils.GetPrefabName(ghost);
    if (type == SelectedType.Default)
    {
      DataHelper.Init(name, ghost.transform);
      return obj;
    }
    if (type == SelectedType.Object)
    {
      DataHelper.Init(name, ghost.transform, Selection.GetData(0));
      return ZNetScene.instance.GetPrefab(name);
    }
    if (type == SelectedType.Location)
      return ZoneSystem.instance.m_locationProxyPrefab;
    if (type == SelectedType.Multiple)
    {
      var dummy = new GameObject
      {
        name = "Blueprint"
      };
      return dummy;
    }
    return obj;
  }

  private static void HandleCommand(GameObject ghost)
  {
    var scale = Scaling.Command;
    var x = ghost.transform.position.x.ToString(CultureInfo.InvariantCulture);
    var y = ghost.transform.position.y.ToString(CultureInfo.InvariantCulture);
    var z = ghost.transform.position.z.ToString(CultureInfo.InvariantCulture);
    var radius = scale.X.ToString(CultureInfo.InvariantCulture);
    var innerSize = Mathf.Min(scale.X, scale.Z).ToString(CultureInfo.InvariantCulture);
    var outerSize = Mathf.Max(scale.X, scale.Z).ToString(CultureInfo.InvariantCulture);
    var depth = scale.Z.ToString(CultureInfo.InvariantCulture);
    var width = scale.X.ToString(CultureInfo.InvariantCulture);
    if (Ruler.Shape == RulerShape.Circle)
    {
      innerSize = radius;
      outerSize = radius;
    }
    if (Ruler.Shape != RulerShape.Rectangle)
      depth = width;
    if (Ruler.Shape == RulerShape.Square)
    {
      innerSize = radius;
      outerSize = radius;
    }
    if (Ruler.Shape == RulerShape.Rectangle)
    {
      innerSize = width;
      outerSize = width;
    }
    var height = scale.Y.ToString(CultureInfo.InvariantCulture);
    var angle = ghost.transform.rotation.eulerAngles.y.ToString(CultureInfo.InvariantCulture);

    var command = Selection.Command;
    var multiShape = command.Contains("#r") && (command.Contains("#w") || command.Contains("#d"));
    if (multiShape)
    {
      var circle = Ruler.Shape == RulerShape.Circle || Ruler.Shape == RulerShape.Ring;
      var args = command.Split(' ').ToList();
      for (var i = args.Count - 1; i > -1; i--)
      {
        if (circle && (args[i].Contains("#w") || args[i].Contains("#d")))
          args.RemoveAt(i);
        if (!circle && args[i].Contains("#r"))
          args.RemoveAt(i);
      }
      command = string.Join(" ", args);
    }
    if (command.Contains("#id"))
    {
      var hovered = Selector.GetHovered(Configuration.Range, Configuration.IgnoredIds);
      if (hovered == null)
      {
        Helper.AddError(Console.instance, "Nothing is being hovered.");
        return;
      }
      command = command.Replace("#id", Utils.GetPrefabName(hovered.gameObject));
    }
    command = command.Replace("#r1-r2", $"{innerSize}-{outerSize}");
    command = command.Replace("#w1-w2", $"{innerSize}-{outerSize}");

    if (Ruler.Shape == RulerShape.Grid)
      command = command.Replace("#d", $"{innerSize}-{outerSize}");
    else
      command = command.Replace("#d", depth);
    command = command.Replace("#r", radius);
    command = command.Replace("#w", width);
    command = command.Replace("#a", angle);
    command = command.Replace("#x", x);
    command = command.Replace("#y", y);
    command = command.Replace("#z", z);
    command = command.Replace("#tx", x);
    command = command.Replace("#ty", y);
    command = command.Replace("#tz", z);
    command = command.Replace("#h", height);
    command = command.Replace("#ignore", Configuration.configIgnoredIds.Value);
    if (!Configuration.DisableMessages)
      Console.instance.AddString($"Hammering command: {command}");
    var prev = HideEffects.Active;
    HideEffects.Active = false;
    // Hide effects prevents some visuals from being shown (like status effects).
    Console.instance.TryRunCommand(command);
    HideEffects.Active = prev;
  }

  private static void HandleMultiple(GameObject ghost)
  {
    Undo.StartTracking();
    var children = Helper.GetChildren(ghost);
    ValheimRAFT.Handle(children);
    for (var i = 0; i < children.Count; i++)
    {
      var ghostChild = children[i];
      var name = Utils.GetPrefabName(ghostChild);
      if (ValheimRAFT.IsRaft(name)) continue;
      var prefab = ZNetScene.instance.GetPrefab(name);
      if (prefab)
      {
        DataHelper.Init(name, ghostChild.transform, Selection.GetData(i));
        var childObj = UnityEngine.Object.Instantiate(prefab, ghostChild.transform.position, ghostChild.transform.rotation);
        Hammer.PostProcessPlaced(childObj);
      }
    }
    Undo.StopTracking();
  }
  static void Postprocess(GameObject obj)
  {
    if (!Configuration.Enabled) return;
    Helper.EnsurePiece(obj);
    var ghost = Helper.GetPlayer().m_placementGhost;
    if (!ghost) return;
    var piece = obj.GetComponent<Piece>();
    if (Selection.Type == SelectedType.Command)
    {
      HandleCommand(ghost);
      UnityEngine.Object.Destroy(obj);
      return;
    }
    if (Selection.Type == SelectedType.Multiple)
    {
      HandleMultiple(ghost);
      UnityEngine.Object.Destroy(obj);
      return;
    }
    var view = obj.GetComponent<ZNetView>();
    // Hoe adds pieces too.
    if (!view) return;
    view.m_body?.WakeUp();
    if (Selection.Type == SelectedType.Location && obj.GetComponent<LocationProxy>())
    {
      Undo.StartTracking();
      Hammer.SpawnLocation(view);
      Undo.StopTracking();
    }
    else
    {
      Hammer.PostProcessPlaced(piece.gameObject);
      Undo.CreateObject(piece.gameObject);
    }
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldloc_2))
          .Advance(1)
          .Insert(new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate(GetPrefab).operand))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4_1),
              new CodeMatch(OpCodes.Ret))
          .Advance(-1)
          .Insert(new CodeInstruction(OpCodes.Ldloc_3),
            new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate(Postprocess).operand))
          .InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacement))]
public class HoldUse
{
  // Needed to prevent instant usage when selecting continuous command.
  public static bool Disabled = true;
  static void CheckHold()
  {
    if (!ZInput.GetButton("Attack") && !ZInput.GetButton("JoyPlace"))
      Disabled = false;
    if (Disabled || !Selection.IsContinuous()) return;
    var player = Player.m_localPlayer;
    if (ZInput.GetButton("Attack") || ZInput.GetButton("JoyPlace"))
      player.m_placePressedTime = Time.time;
    if (Time.time - player.m_lastToolUseTime > player.m_placeDelay / 4f)
      player.m_lastToolUseTime = 0f;
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(
                  OpCodes.Ldstr,
                  "Attack"))
          .Insert(new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate(CheckHold).operand))
          .InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
public class PostProcessToolOnPlace
{
  static void Postfix(Player __instance, ref bool __result)
  {
    if (__result) Hammer.PostProcessTool(__instance);
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
public class UnlockBuildDistance
{
  public static void Prefix(Player __instance, ref float __state)
  {
    __state = __instance.m_maxPlaceDistance;
    if (Configuration.Range > 0f)
      __instance.m_maxPlaceDistance = Configuration.Range;
    if (Selection.Type == SelectedType.Command)
      __instance.m_maxPlaceDistance = 1000f;
  }
  public static void Postfix(Player __instance, float __state)
  {
    __instance.m_maxPlaceDistance = __state;
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.SetupPlacementGhost))]
public class SetupPlacementGhost
{
  public static void Postfix(Player __instance)
  {
    if (!__instance.m_placementGhost) return;
    // Ensures that the scale is reseted when selecting objects from the build menu.
    Scaling.Build.SetScale(__instance.m_placementGhost.transform.localScale);
    // When copying an existing object, the copy is inactive.
    // So the ghost must be manually activated while disabling ZNet stuff.
    if (__instance.m_placementGhost && !__instance.m_placementGhost.activeSelf)
    {
      ZNetView.m_forceDisableInit = true;
      __instance.m_placementGhost.SetActive(true);
      ZNetView.m_forceDisableInit = false;
    }
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnLocation))]
public class CustomizeSpawnLocation
{
  public static bool? RandomDamage = null;
  public static bool AllViews = false;
  static void Customize()
  {
    if (RandomDamage.HasValue)
    {
      WearNTear.m_randomInitialDamage = RandomDamage.Value;
    }
    if (AllViews)
    {
      var data = Selection.GetData();
      if (data != null)
      {
        var location = ZoneSystem.instance.GetLocation(data.GetInt(Hash.Location, 0));
        if (location != null)
        {
          foreach (var view in location.m_netViews)
            view.gameObject.SetActive(true);
        }

      }
    }
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
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

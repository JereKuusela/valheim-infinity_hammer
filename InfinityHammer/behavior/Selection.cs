using System.Collections.Generic;
using Data;
using HarmonyLib;
using ServerDevcommands;
using UnityEngine;
namespace InfinityHammer;

public class SelectedObject(int prefab, bool scalable, DataEntry? data)
{
  public int Prefab = prefab;
  public DataEntry? Data = data;
  public bool Scalable = scalable;
}

public static class Selection
{
  public static BaseSelection BaseSelection = new();
  public static Dictionary<string, BaseSelection> Selections = [];
  public static BaseSelection Get() => Selections.TryGetValue(HammerHelper.GetTool(), out var selection) ? selection : BaseSelection;
  public static void Clear()
  {
    if (Configuration.UnfreezeOnSelect) Position.Unfreeze();
    if (!Selections.TryGetValue(HammerHelper.GetTool(), out var selection))
      return;
    selection.Deactivate();
    selection.Destroy();

    Hammer.SelectRepairIfEmpty();
    Selections.Remove(HammerHelper.GetTool());
  }
  public static void Destroy()
  {
    foreach (var selection in Selections.Values)
    {
      selection.Deactivate();
      selection.Destroy();
    }
    Selections.Clear();
  }
  public static GameObject CreateGhost(BaseSelection selection)
  {
    Clear();
    selection.Activate();
    Selections[HammerHelper.GetTool()] = selection;
    var player = Helper.GetPlayer();
    player.SetupPlacementGhost();
    return player.m_placementGhost;
  }
}

///<summary>Removes resource usage.</summary>
[HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetItem))]
public class SetItemHack
{
  public static bool Hack = false;

  static void SetItem(VisEquipment obj, VisSlot slot, int hash, int variant, int quality)
  {
    switch (slot)
    {
      case VisSlot.HandLeft:
        obj.m_leftItem = hash;
        obj.m_leftItemQuality = quality;
        obj.m_leftItemVariant = variant;
        return;
      case VisSlot.HandRight:
        obj.m_rightItem = hash;
        obj.m_rightItemQuality = quality;
        return;
      case VisSlot.BackLeft:
        obj.m_leftBackItem = hash;
        obj.m_leftBackItemQuality = quality;
        obj.m_leftBackItemVariant = variant;
        return;
      case VisSlot.BackRight:
        obj.m_rightBackItem = hash;
        obj.m_rightBackItemQuality = quality;
        return;
      case VisSlot.Chest:
        obj.m_chestItem = hash;
        return;
      case VisSlot.Legs:
        obj.m_legItem = hash;
        return;
      case VisSlot.Helmet:
        obj.m_helmetItem = hash;
        return;
      case VisSlot.Shoulder:
        obj.m_shoulderItem = hash;
        obj.m_shoulderItemQuality = quality;
        obj.m_shoulderItemVariant = variant;
        return;
      case VisSlot.Utility:
        obj.m_utilityItem = hash;
        return;
      case VisSlot.Beard:
        obj.m_beardItem = hash;
        return;
      case VisSlot.Hair:
        obj.m_hairItem = hash;
        return;
    }
  }
  static bool Prefix(VisEquipment __instance, VisSlot slot, int itemHash, int variant, int quality)
  {
    if (Hack)
      SetItem(__instance, slot, itemHash, variant, quality);
    return !Hack;
  }
}

///<summary>Overrides the piece selection.</summary>
[HarmonyPatch(typeof(PieceTable), nameof(PieceTable.GetSelectedPiece))]
public class GetSelectedPiece
{
  public static Piece Postfix(Piece result, PieceTable __instance)
  {
    if (!Configuration.Enabled || !Player.m_localPlayer || Player.m_localPlayer.m_buildPieces != __instance) return result;
    var selected = Selection.Get().GetSelectedPiece();
    if (selected) return selected;
    // Navigation/action buttons are never placement prefabs after closing a menu.
    if (result && result.TryGetComponent<BuildMenuTool>(out var button) && button.tool?.Instant == true) return null!;
    return result!;
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.SetPlaceMode))]
public class SelectionActivate
{
  static void Postfix(Player __instance)
  {
    if (__instance == Player.m_localPlayer) Selection.Get().Activate();
  }
}


[HarmonyPatch(typeof(Player), nameof(Player.OnDestroy))]
public class PlayerOnDestroy
{
  static void Prefix(Player __instance)
  {
    if (__instance == Player.m_localPlayer)
    {
      Player_ManualUpdate.Projector = null;
      Selection.Destroy();
      ToolMenuPieces.Clear();
    }
  }
}

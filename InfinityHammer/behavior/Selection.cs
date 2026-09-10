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

  static void SetItem(VisEquipment obj, VisSlot slot, int itemHash, int variant)
  {
    switch (slot)
    {
      case VisSlot.HandLeft:
        obj.m_leftItem = itemHash;
        obj.m_leftItemVariant = variant;
        return;
      case VisSlot.HandRight:
        obj.m_rightItem = itemHash;
        return;
      case VisSlot.BackLeft:
        obj.m_leftBackItem = itemHash;
        obj.m_leftBackItemVariant = variant;
        return;
      case VisSlot.BackRight:
        obj.m_rightBackItem = itemHash;
        return;
      case VisSlot.Chest:
        obj.m_chestItem = itemHash;
        return;
      case VisSlot.Legs:
        obj.m_legItem = itemHash;
        return;
      case VisSlot.Helmet:
        obj.m_helmetItem = itemHash;
        return;
      case VisSlot.Shoulder:
        obj.m_shoulderItem = itemHash;
        obj.m_shoulderItemVariant = variant;
        return;
      case VisSlot.Utility:
        obj.m_utilityItem = itemHash;
        return;
      case VisSlot.Beard:
        obj.m_beardItem = itemHash;
        return;
      case VisSlot.Hair:
        obj.m_hairItem = itemHash;
        return;
    }
  }
  static bool Prefix(VisEquipment __instance, VisSlot slot, int itemHash, int variant)
  {
    if (Hack)
      SetItem(__instance, slot, itemHash, variant);
    return !Hack;
  }
}

///<summary>Overrides the piece selection.</summary>
[HarmonyPatch(typeof(PieceTable), nameof(PieceTable.GetSelectedPiece))]
public class GetSelectedPiece
{
  public static Piece Postfix(Piece result) => Configuration.Enabled ? Selection.Get().GetSelectedPiece() ?? result : result;
}

[HarmonyPatch(typeof(Player), nameof(Player.SetPlaceMode))]
public class SelectionActivate
{
  static void Postfix() => Selection.Get().Activate();
}


[HarmonyPatch(typeof(Player), nameof(Player.OnDestroy))]
public class PlayerOnDestroy
{
  static void Prefix(Player __instance)
  {
    if (__instance == Player.m_localPlayer)
      Selection.Destroy();
  }
}

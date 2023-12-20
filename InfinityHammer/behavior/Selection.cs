using System;
using System.Collections.Generic;
using HarmonyLib;
using Service;
using UnityEngine;
namespace InfinityHammer;

public class SelectedObject(string name, bool scalable, ZDOData data)
{
  public string Prefab = name;
  public ZDOData Data = data;
  public bool Scalable = scalable;
}

public static partial class Selection
{
  public static Dictionary<string, BaseSelection> Selections = [];
  public static BaseSelection Get() => Selections.TryGetValue(HammerHelper.GetTool(), out var selection) ? selection : throw new InvalidOperationException("No selection.");
  public static BaseSelection? TryGet() => Selections.TryGetValue(HammerHelper.GetTool(), out var selection) ? selection : null;
  public static void Clear()
  {
    if (Configuration.UnfreezeOnSelect) Position.Unfreeze();
    if (!Selections.TryGetValue(HammerHelper.GetTool(), out var selection))
      return;
    UnityEngine.Object.Destroy(selection.SelectedObject);
    Selections.Remove(HammerHelper.GetTool());
  }
  public static string Description() => TryGet()?.ExtraDescription ?? "";
  public static bool IsContinuous() => false;
  public static bool IsPlayerHeight() => false;
  public static GameObject Create(BaseSelection selection)
  {
    Clear();
    Selections[HammerHelper.GetTool()] = selection;
    return selection.SelectedObject;
  }
}

///<summary>Removes resource usage.</summary>
[HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetItem))]
public class SetItemHack
{
  public static bool Hack = false;

  static void SetItem(VisEquipment obj, VisSlot slot, string name, int variant)
  {
    switch (slot)
    {
      case VisSlot.HandLeft:
        obj.m_leftItem = name;
        obj.m_leftItemVariant = variant;
        return;
      case VisSlot.HandRight:
        obj.m_rightItem = name;
        return;
      case VisSlot.BackLeft:
        obj.m_leftBackItem = name;
        obj.m_leftBackItemVariant = variant;
        return;
      case VisSlot.BackRight:
        obj.m_rightBackItem = name;
        return;
      case VisSlot.Chest:
        obj.m_chestItem = name;
        return;
      case VisSlot.Legs:
        obj.m_legItem = name;
        return;
      case VisSlot.Helmet:
        obj.m_helmetItem = name;
        return;
      case VisSlot.Shoulder:
        obj.m_shoulderItem = name;
        obj.m_shoulderItemVariant = variant;
        return;
      case VisSlot.Utility:
        obj.m_utilityItem = name;
        return;
      case VisSlot.Beard:
        obj.m_beardItem = name;
        return;
      case VisSlot.Hair:
        obj.m_hairItem = name;
        return;
    }
  }
  static bool Prefix(VisEquipment __instance, VisSlot slot, string name, int variant)
  {
    if (Hack)
      SetItem(__instance, slot, name, variant);
    return !Hack;
  }
}

// This is invasive but simplifies the mod when normal selection can be handle the same way.
[HarmonyPatch(typeof(PieceTable), nameof(PieceTable.SetSelected))]
public class PieceTableSetSelected
{
  static void Postfix(PieceTable __instance)
  {
    Selection.Clear();
    var index = __instance.GetSelectedIndex();
    var piece = __instance.GetPiece((int)__instance.m_selectedCategory, index);
    if (piece)
      Selection.Create(new ObjectSelection(Utils.GetPrefabName(piece.gameObject), false));
  }
}
///<summary>Overrides the piece selection.</summary>
[HarmonyPatch(typeof(PieceTable), nameof(PieceTable.GetSelectedPiece))]
public class GetSelectedPiece
{
  public static bool Prefix(ref Piece __result)
  {
    var selection = Selection.TryGet();
    if (selection == null) return true;
    __result = selection.GetSelectedPiece();
    return false;
  }
}
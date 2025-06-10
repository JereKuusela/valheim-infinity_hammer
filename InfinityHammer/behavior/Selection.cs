using System;
using System.Collections.Generic;
using Argo.Blueprint;
using Data;
using HarmonyLib;
using ServerDevcommands;
using UnityEngine;

namespace InfinityHammer;

public class SelectedObject 
{
    public SelectedObject(int prefab, bool scalable, DataEntry? data, GameObject obj) {
        m_Prefab     = prefab;
        m_Data       = data;
        m_Scalable   = scalable;
        m_Gameobject = new WeakReference<GameObject>(obj);
    }
    protected SelectedObject(int prefab, bool scalable, DataEntry? data) {
        m_Prefab   = prefab;
        m_Data     = data;
        m_Scalable = scalable;
    }
    protected int                       m_Prefab;
    protected DataEntry?                m_Data;
    protected bool                      m_Scalable;
    protected WeakReference<GameObject> m_Gameobject;
    public int                       Prefab     { get => m_Prefab; }
    public DataEntry?                Data       { get => m_Data; }
    public bool                      Scalable   { get => m_Scalable; }
    public WeakReference<GameObject> GameObject { get => m_Gameobject; }
}

public static class Selection
{
    public static BaseSelection                     BaseSelection = new();
    public static Dictionary<string, BaseSelection> Selections    = [];
    public static BaseSelection Get() => Selections.TryGetValue(HammerHelper.GetTool(), out var selection)
        ? selection
        : BaseSelection;
    public static void Clear() {
        if (Configuration.UnfreezeOnSelect) Position.Unfreeze();
        if (!Selections.TryGetValue(HammerHelper.GetTool(), out var selection))
            return;
        selection.Deactivate();
        selection.Destroy();
        Selections.Remove(HammerHelper.GetTool());
    }
    public static void Destroy() {
        foreach (var selection in Selections.Values) {
            selection.Deactivate();
            selection.Destroy();
        }
        Selections.Clear();
    }
    public static GameObject CreateGhost(BaseSelection selection) {
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

    static void SetItem(VisEquipment obj, VisSlot slot, string name, int variant) {
        switch (slot) {
            case VisSlot.HandLeft:
                obj.m_leftItem        = name;
                obj.m_leftItemVariant = variant;
                return;
            case VisSlot.HandRight:
                obj.m_rightItem = name;
                return;
            case VisSlot.BackLeft:
                obj.m_leftBackItem        = name;
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
                obj.m_shoulderItem        = name;
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
    static bool Prefix(VisEquipment __instance, VisSlot slot, string name, int variant) {
        if (Hack)
            SetItem(__instance, slot, name, variant);
        return !Hack;
    }
}

// This is invasive but simplifies the mod when normal selection can be handled the same way.
[HarmonyPatch(typeof(PieceTable), nameof(PieceTable.SetSelected))]
public class PieceTableSetSelected
{
    static void Prefix() => Selection.Clear();
    static void Postfix(PieceTable __instance) {
        var index = __instance.GetSelectedIndex();
        var piece = __instance.GetPiece(__instance.GetSelectedCategory(), index);
        if (piece && piece.GetComponent<ZNetView>())
            Selection.CreateGhost(new ObjectSelection(piece, false));
    }
}

// This is invasive but simplifies the mod when normal selection can be handled the same way.
[HarmonyPatch(typeof(PieceTable), nameof(PieceTable.SetCategory))]
public class PieceTableSetCategory
{
    static void Prefix() => Selection.Clear();
    static void Postfix(PieceTable __instance) {
        var index = __instance.GetSelectedIndex();
        var piece = __instance.GetPiece(__instance.GetSelectedCategory(), index);
        if (piece && piece.GetComponent<ZNetView>())
            Selection.CreateGhost(new ObjectSelection(piece, false));
    }
}

[HarmonyPatch(typeof(PieceTable), nameof(PieceTable.PrevCategory))]
public class PieceTablePrevCategory
{
    static void Prefix() => Selection.Clear();
    static void Postfix(PieceTable __instance) {
        var index = __instance.GetSelectedIndex();
        var piece = __instance.GetPiece(__instance.GetSelectedCategory(), index);
        if (piece && piece.GetComponent<ZNetView>())
            Selection.CreateGhost(new ObjectSelection(piece, false));
    }
}

[HarmonyPatch(typeof(PieceTable), nameof(PieceTable.NextCategory))]
public class PieceTableNextCategory
{
    static void Prefix() => Selection.Clear();
    static void Postfix(PieceTable __instance) {
        var index = __instance.GetSelectedIndex();
        var piece = __instance.GetPiece(__instance.GetSelectedCategory(), index);
        if (piece && piece.GetComponent<ZNetView>())
            Selection.CreateGhost(new ObjectSelection(piece, false));
    }
}

///<summary>Overrides the piece selection.</summary>
[HarmonyPatch(typeof(PieceTable), nameof(PieceTable.GetSelectedPiece))]
public class GetSelectedPiece
{
    public static Piece Postfix(Piece result) =>
        Configuration.Enabled ? Selection.Get().GetSelectedPiece() ?? result : result;
}

[HarmonyPatch(typeof(Player), nameof(Player.SetPlaceMode))]
public class SelectionActivate
{
    static void Postfix() => Selection.Get().Activate();
}

[HarmonyPatch(typeof(Player), nameof(Player.OnDestroy))]
public class PlayerOnDestroy
{
    static void Prefix(Player __instance) {
        if (__instance == Player.m_localPlayer)
            Selection.Destroy();
    }
}
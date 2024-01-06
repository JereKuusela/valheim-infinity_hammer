using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace InfinityHammer;

[HarmonyPatch(typeof(ZNetView), nameof(ZNetView.Awake))]
public static class UndoTracker
{
  public static void Postfix(ZNetView __instance)
  {
    if (Undo.Track)
      Undo.CreateObject(__instance.gameObject);
  }
}
public class Undo
{

  public static void AddPlaceStep(IEnumerable<ZDO> objs)
  {
    if (objs.Count() == 0) return;
    UndoPlace action = new(objs);
    UndoManager.Add(action);
  }
  public static void AddRemoveStep(IEnumerable<ZNetView> objs)
  {
    if (objs.Count() == 0) return;
    UndoRemove action = new(objs.Select(obj => new FakeZDO(obj.GetZDO())));
    UndoManager.Add(action);
  }
  private static bool GroupCreating = false;
  public static List<ZDO> Objects = [];
  public static bool Track = false;
  public static void CreateObject(GameObject obj)
  {
    if (!obj) return;
    foreach (var view in obj.GetComponentsInChildren<ZNetView>())
    {
      if (view.GetZDO() != null)
        Objects.Add(view.GetZDO());
    }
    if (!GroupCreating && !Track) Finish();
  }
  public static void StartTracking()
  {
    Track = true;
  }
  public static void StopTracking()
  {
    Track = false;
    if (!GroupCreating && !Track) Finish();
  }
  public static void StartCreating()
  {
    GroupCreating = true;
  }
  public static void FinishCreating()
  {
    GroupCreating = false;
    if (!GroupCreating && !Track) Finish();
  }
  private static void Finish()
  {
    AddPlaceStep(Objects);
    Objects.Clear();
    GroupCreating = false;
    Track = false;
  }
}

public class UndoRemove(IEnumerable<FakeZDO> zdos) : MonoBehaviour, IUndoAction
{
  private FakeZDO[] Zdos = zdos.ToArray();

  public string Undo()
  {
    Zdos = Zdos.Select(UndoHelper.Place).ToArray();
    return $"Restored {UndoHelper.Print(Zdos)}";
  }
  public string Redo()
  {
    UndoHelper.Remove(Zdos);
    return $"Removed {UndoHelper.Print(Zdos)}"; ;
  }
}


public class UndoPlace(IEnumerable<ZDO> zdos) : MonoBehaviour, IUndoAction
{

  private FakeZDO[] Zdos = zdos.Select(zdo => new FakeZDO(zdo)).ToArray();

  public string Undo()
  {
    UndoHelper.Remove(Zdos);
    return $"Removed {UndoHelper.Print(Zdos)}";
  }

  public string Redo()
  {
    Zdos = Zdos.Select(UndoHelper.Place).ToArray();
    return $"Placed {UndoHelper.Print(Zdos)}";
  }
}

public class UndoHelper
{
  public static FakeZDO Place(FakeZDO zdo)
  {
    var prefab = ZNetScene.instance.GetPrefab(zdo.Prefab);
    if (!prefab) throw new InvalidOperationException("Error: Prefab not found.");
    ZNetView.m_initZDO = zdo.Create();
    FakeZDO newZdo = new(ZNetView.m_initZDO);
    UnityEngine.Object.Instantiate(prefab, ZNetView.m_initZDO.GetPosition(), ZNetView.m_initZDO.GetRotation());
    return newZdo;
  }
  public static void Remove(FakeZDO[] toRemove)
  {
    foreach (var zdo in toRemove)
      HammerHelper.RemoveZDO(zdo.Source);
  }

  public static string Name(int hash) => Utils.GetPrefabName(ZNetScene.instance.GetPrefab(hash));
  public static string Print(IEnumerable<FakeZDO> zdos) => Print(zdos.Select(zdo => zdo.Prefab));
  public static string Print(IEnumerable<int> data)
  {
    if (data.Count() == 1) return Name(data.First());
    var names = data.GroupBy(Name);
    if (names.Count() == 1) return $"{names.First().Key} {names.First().Count()}x";
    return $" objects {data.Count()}x";
  }
}

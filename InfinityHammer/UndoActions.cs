using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Service;
using UnityEngine;
namespace InfinityHammer;

[HarmonyPatch(typeof(ZNetView), "Awake")]
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
  private static readonly BindingFlags PrivateBinding = BindingFlags.Static | BindingFlags.NonPublic;
  private static Type? Type() => CommandWrapper.ServerDevcommands?.GetType("ServerDevcommands.UndoManager");

  public static void Place(IEnumerable<ZDO> objs)
  {
    if (objs.Count() == 0) return;
    UndoPlace action = new(objs);
    Type()?.GetMethod("Add", PrivateBinding).Invoke(null, new[] { action });
  }
  public static void Remove(IEnumerable<FakeZDO> objs)
  {
    if (objs.Count() == 0) return;
    UndoRemove action = new(objs);
    Type()?.GetMethod("Add", PrivateBinding).Invoke(null, new[] { action });
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
    Place(Objects);
    Objects.Clear();
    GroupCreating = false;
    Track = false;
  }
}

public class UndoRemove : MonoBehaviour, IUndoAction
{
  private FakeZDO[] Zdos;
  public UndoRemove(IEnumerable<FakeZDO> zdos)
  {
    Zdos = zdos.ToArray();
  }
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


public class UndoPlace : MonoBehaviour, IUndoAction
{

  private FakeZDO[] Zdos;
  public UndoPlace(IEnumerable<ZDO> zdos)
  {
    Zdos = zdos.Select(zdo => new FakeZDO(zdo)).ToArray();
  }
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
      Helper.RemoveZDO(zdo.Source);
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

public interface IUndoAction
{
  string Undo();
  string Redo();
}
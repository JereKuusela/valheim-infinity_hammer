using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace InfinityHammer;

public class UndoHelper {
  private static bool GroupCreating = false;
  private static List<ZDO> Objects = new();
  public static void CreateObject(ZNetView obj) {
    if (!obj) return;
    Objects.Add(obj.GetZDO());
    if (!GroupCreating) FinishCreating();
  }
  public static void StartCreating() {
    GroupCreating = true;
  }
  public static void FinishCreating() {
    UndoWrapper.Place(Objects);
    Objects.Clear();
    GroupCreating = false;
  }
  public static ZDO Place(ZDO zdo) {
    var prefab = ZNetScene.instance.GetPrefab(zdo.GetPrefab());
    if (prefab) {
      var obj = UnityEngine.Object.Instantiate<GameObject>(prefab, zdo.GetPosition(), zdo.GetRotation());
      var netView = obj.GetComponent<ZNetView>();
      if (netView) {
        var added = netView.GetZDO();
        netView.SetLocalScale(zdo.GetVec3("scale", obj.transform.localScale));
        Helper.CopyData(zdo.Clone(), added);
        Hammer.FixData(netView);
        return added;
      }
    }
    return null;
  }
  public static ZDO[] Place(ZDO[] data) => data.Select(Place).Where(obj => obj != null).ToArray();

  public static string Name(ZDO zdo) => Utils.GetPrefabName(ZNetScene.instance.GetPrefab(zdo.GetPrefab()));
  public static string Print(ZDO[] data) {
    if (data.Count() == 1) return Name(data.First());
    var names = data.GroupBy(Name);
    if (names.Count() == 1) return $"{names.First().Key} {names.First().Count()}x";
    return $" objects {data.Count()}x";
  }
  public static ZDO[] Remove(ZDO[] toRemove) {
    if (toRemove == null) return null;
    var data = UndoHelper.Clone(toRemove);
    foreach (var zdo in toRemove) Helper.RemoveZDO(zdo);
    return data;
  }

  public static ZDO[] Clone(IEnumerable<ZDO> data) => data.Select(zdo => zdo.Clone()).ToArray();
}
public class UndoRemove : MonoBehaviour, UndoAction {

  private ZDO[] Data = null;
  public UndoRemove(IEnumerable<ZDO> data) {
    Data = UndoHelper.Clone(data);
  }
  public void Undo() {
    Data = UndoHelper.Place(Data);
  }

  public void Redo() {
    Data = UndoHelper.Remove(Data);
  }

  public string UndoMessage() => $"Undo: Restored {UndoHelper.Print(Data)}";

  public string RedoMessage() => $"Redo: Removed {UndoHelper.Print(Data)}";
}

public class UndoPlace : MonoBehaviour, UndoAction {

  private ZDO[] Data = null;
  public UndoPlace(IEnumerable<ZDO> data) {
    Data = UndoHelper.Clone(data);
  }
  public void Undo() {
    Data = UndoHelper.Remove(Data);
  }

  public string UndoMessage() => $"Undo: Removed {UndoHelper.Print(Data)}";

  public void Redo() {
    Data = UndoHelper.Place(Data);
  }
  public string RedoMessage() => $"Redo: Restored {UndoHelper.Print(Data)}";
}

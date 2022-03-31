using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace InfinityHammer;
public class UndoData {
  public int Prefab = 0;
  public Vector3 Position;
  public Quaternion Rotation;
  public Vector3 Scale;
  public ZDO Data = null;
  public string Name => Utils.GetPrefabName(ZNetScene.instance.GetPrefab(Prefab));
}

public class UndoHelper {
  private static bool GroupCreating = false;
  private static List<ZNetView> Objects = new();
  public static void CreateObject(ZNetView obj) {
    if (!obj) return;
    Objects.Add(obj);
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
  public static ZDO Place(UndoData data) {
    var prefab = ZNetScene.instance.GetPrefab(data.Prefab);
    if (prefab) {
      var obj = UnityEngine.Object.Instantiate<GameObject>(prefab, data.Position, data.Rotation);
      var netView = obj.GetComponent<ZNetView>();
      if (netView) {
        var added = netView.GetZDO();
        netView.SetLocalScale(data.Scale);
        Helper.CopyData(data.Data.Clone(), added);
        Hammer.FixData(netView);
        return added;
      }
    }
    return null;
  }
  public static IEnumerable<ZDO> Place(IEnumerable<UndoData> data) => data.Select(Place).Where(obj => obj != null);
  public static string Print(IEnumerable<UndoData> data) {
    if (data.Count() == 1) return data.First().Name;
    var names = data.GroupBy(data => data.Name);
    if (names.Count() == 1) return $"{names.First().Key} {names.First().Count()}x";
    return $" objects {data.Count()}x";
  }
  public static void Remove(IEnumerable<ZDO> added) {
    if (added == null) return;
    foreach (var zdo in added) Helper.RemoveZDO(zdo);
  }

  public static UndoData CreateData(ZNetView obj) {
    var zdo = obj.GetZDO();
    return new UndoData {
      Prefab = zdo.GetPrefab(),
      Data = zdo.Clone(),
      Position = zdo.GetPosition(),
      Rotation = zdo.GetRotation(),
      Scale = obj.transform.localScale
    };
  }
}
public class UndoRemove : MonoBehaviour, UndoAction {

  private UndoData[] Data = null;
  private ZDO[] Added = null;
  public UndoRemove(IEnumerable<UndoData> data) {
    Data = data.ToArray();
  }
  public void Undo() {
    Added = UndoHelper.Place(Data).ToArray();
  }

  public void Redo() {
    UndoHelper.Remove(Added);
    Added = null;
  }

  public string UndoMessage() => $"Undo: Restored {UndoHelper.Print(Data)}";

  public string RedoMessage() => $"Redo: Removed {UndoHelper.Print(Data)}";
}

public class UndoPlace : MonoBehaviour, UndoAction {

  private UndoData[] Data = null;
  private ZDO[] Added = null;
  public UndoPlace(IEnumerable<ZNetView> obj) {
    Data = obj.Select(UndoHelper.CreateData).ToArray();
    Added = obj.Select(obj => obj.GetZDO()).ToArray();
  }
  public void Undo() {
    UndoHelper.Remove(Added);
    Added = null;
  }

  public string UndoMessage() => $"Undo: Removed {UndoHelper.Print(Data)}";

  public void Redo() {
    Added = UndoHelper.Place(Data).ToArray();
  }
  public string RedoMessage() => $"Redo: Restored {UndoHelper.Print(Data)}";
}

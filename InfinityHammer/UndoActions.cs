using UnityEngine;

namespace InfinityHammer {
  public class UndoData {
    public int Prefab = 0;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public ZDO Data = null;
    public string Name => Utils.GetPrefabName(ZNetScene.instance.GetPrefab(Prefab));
  }

  public class UndoHelper {
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

    public static void Remove(ZDO added) {
      if (added != null)
        Helper.RemoveZDO(added);
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

    private UndoData Data = null;
    private ZDO Added = null;
    public UndoRemove(UndoData data) {
      Data = data;
    }
    public void Undo() {
      Added = UndoHelper.Place(Data);
    }

    public void Redo() {
      UndoHelper.Remove(Added);
      Added = null;
    }

    public string UndoMessage() => $"Undo: Restored {Data.Name}";

    public string RedoMessage() => $"Redo: Removed {Data.Name}";
  }

  public class UndoPlace : MonoBehaviour, UndoAction {

    private UndoData Data = null;
    private ZDO Added = null;
    public UndoPlace(ZNetView obj) {
      Data = UndoHelper.CreateData(obj);
      Added = obj.GetZDO();
    }
    public void Undo() {
      UndoHelper.Remove(Added);
      Added = null;
    }

    public string UndoMessage() => $"Undo: Removed {Data.Name}";

    public void Redo() {
      Added = UndoHelper.Place(Data);
    }
    public string RedoMessage() => $"Redo: Restored {Data.Name}";
  }

}
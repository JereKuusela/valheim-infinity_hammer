using Service;
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
          Helper.CopyData(data.Data, added);
          return added;
        }
      }
      return null;
    }

    public static void Remove(ZDO added) {
      if (added != null)
        Helper.RemoveZDO(added);
    }

    public static UndoData CreateDate(ZNetView obj) {
      var zdo = obj.GetZDO();
      return new UndoData() {
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
      if (Data != null)
        Helper.AddMessage(Console.instance, $"Undo: Restored {Data.Name}");
    }

    public void Redo() {
      UndoHelper.Remove(Added);
      if (Data != null)
        Helper.AddMessage(Console.instance, $"Redo: Removed {Data.Name}");
      Added = null;
    }
  }

  public class UndoPlace : MonoBehaviour, UndoAction {

    private UndoData Data = null;
    private ZDO Added = null;
    public UndoPlace(ZNetView obj) {
      Data = UndoHelper.CreateDate(obj);
      Added = obj.GetZDO();
    }
    public void Undo() {
      UndoHelper.Remove(Added);
      if (Data != null)
        Helper.AddMessage(Console.instance, $"Undo: Removed {Data.Name}");
      Added = null;
    }

    public void Redo() {
      Added = UndoHelper.Place(Data);
      if (Data != null)
        Helper.AddMessage(Console.instance, $"Redo: Restored {Data.Name}");
    }
  }

}
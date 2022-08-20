using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityHammer;
public partial class Selected {
  private int ZoopsX = 0;
  private int ZoopsY = 0;
  private int ZoopsZ = 0;
  private void ToSingle() {
    var obj = Ghost.transform.GetChild(0).gameObject;
    obj.SetActive(false);
    Helper.EnsurePiece(obj);
    obj.transform.parent = null;
    UnityEngine.Object.Destroy(Ghost);
    Type = SelectedType.Object;
    Ghost = obj;
    Objects = Objects.Take(1).ToList();
    ZoopsX = 0;
    ZoopsY = 0;
    ZoopsZ = 0;
  }
  private void ToMulti() {
    if (Type == SelectedType.Default) {
      var ghost = Player.m_localPlayer.m_placementGhost;
      var name = Utils.GetPrefabName(ghost);
      Set(name);
    }
    var obj = Ghost;
    Type = SelectedType.Multiple;
    Ghost = new GameObject();
    // Prevents children from disappearing.
    Ghost.SetActive(false);
    Ghost.transform.position = obj.transform.position;
    Ghost.transform.rotation = obj.transform.rotation;
    obj.transform.parent = Ghost.transform;
    obj.SetActive(true);
    AddSnapPoints(obj);
    if (obj.GetComponent<Piece>() is { } piece) {
      var name2 = Utils.GetPrefabName(obj);
      if (ZNetScene.instance.GetPrefab(name2) is { } prefab) {
        if (!prefab.GetComponent<Piece>()) UnityEngine.Object.Destroy(piece);
      }
    }
  }
  private void RemoveLast() {
    if (Objects.Count > 0)
      Objects = Objects.Take(Objects.Count - 1).ToList();
    if (Ghost.transform.childCount > 0) {
      var child = Ghost.transform.GetChild(Ghost.transform.childCount - 1);
      child.gameObject.SetActive(false);
      child.parent = null;
      RemoveSnapPoints(child.gameObject);
      UnityEngine.Object.Destroy(child.gameObject);
    }
  }
  private void AddChild(string offset) {
    var baseObj = Ghost.transform.GetChild(0).gameObject;
    var pos = GetOffset(offset);
    ZNetView.m_forceDisableInit = true;
    var obj = Helper.SafeInstantiate(baseObj, Ghost);
    obj.SetActive(true);
    obj.transform.rotation = baseObj.transform.rotation;
    obj.transform.localPosition = pos;
    Objects.Add(new SelectedObject(Objects[0].Prefab, Objects[0].Scalable, Objects[0].Data));
    ZNetView.m_forceDisableInit = false;
    AddSnapPoints(obj);
  }
  private static GameObject SnapObj = new() {
    name = "_snappoint",
    layer = LayerMask.NameToLayer("piece"),
    tag = "snappoint",
  };
  private void RemoveSnapPoints(GameObject obj) {
    Stack<Transform> snaps = new();
    foreach (Transform child in Ghost.transform) {
      if (!child.gameObject.CompareTag("snappoint")) continue;
      snaps.Push(child);
    }
    foreach (Transform child in obj.transform) {
      if (!child.gameObject.CompareTag("snappoint")) continue;
      var snap = snaps.Pop();
      snap.parent = null;
      UnityEngine.Object.Destroy(snap.gameObject);
    }
  }
  private Vector3 GetOffset(string offset) {
    var baseObj = Ghost.transform.GetChild(0).gameObject;
    var size = Helper.ParseSize(baseObj, offset);
    size.x *= baseObj.transform.localScale.x;
    size.x *= ZoopsX;
    size.y *= baseObj.transform.localScale.y;
    size.y *= ZoopsY;
    size.z *= baseObj.transform.localScale.z;
    size.z *= ZoopsZ;
    return size;
  }
  public void ZoopRight(string offset) {
    if (Type == SelectedType.Multiple && ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 0) return;
    if (Type == SelectedType.Command) return;
    if (Type == SelectedType.Location) return;
    if (ZoopsY != 0 || ZoopsZ != 0)
      ToSingle();
    if (ZoopsX == -1) {
      ToSingle();
    } else if (ZoopsX < 0) {
      RemoveLast();
      ZoopsX += 1;
    } else {
      if (ZoopsX == 0)
        ToMulti();
      ZoopsX += 1;
      AddChild(offset);
    }
    CountObjects();
    Helper.GetPlayer().SetupPlacementGhost();
  }
  public void ZoopLeft(string offset) {
    if (Type == SelectedType.Multiple && ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 0) return;
    if (Type == SelectedType.Command) return;
    if (Type == SelectedType.Location) return;
    if (ZoopsY != 0 || ZoopsZ != 0)
      ToSingle();
    if (ZoopsX == 1) {
      ToSingle();
    } else if (ZoopsX > 0) {
      RemoveLast();
      ZoopsX -= 1;
    } else {
      if (ZoopsX == 0)
        ToMulti();
      ZoopsX -= 1;
      AddChild(offset);
    }
    CountObjects();
    Helper.GetPlayer().SetupPlacementGhost();
  }
  public void ZoopUp(string offset) {
    if (Type == SelectedType.Multiple && ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 0) return;
    if (Type == SelectedType.Command) return;
    if (Type == SelectedType.Location) return;
    if (ZoopsX != 0 || ZoopsZ != 0)
      ToSingle();
    if (ZoopsY == -1) {
      ToSingle();
    } else if (ZoopsY < 0) {
      RemoveLast();
      ZoopsY += 1;
    } else {
      if (ZoopsY == 0)
        ToMulti();
      ZoopsY += 1;
      AddChild(offset);
    }
    CountObjects();
    Helper.GetPlayer().SetupPlacementGhost();
  }
  public void ZoopDown(string offset) {
    if (Type == SelectedType.Multiple && ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 0) return;
    if (Type == SelectedType.Command) return;
    if (Type == SelectedType.Location) return;
    if (ZoopsX != 0 || ZoopsZ != 0)
      ToSingle();
    if (ZoopsY == 1) {
      ToSingle();
    } else if (ZoopsY > 0) {
      RemoveLast();
      ZoopsY -= 1;
    } else {
      if (ZoopsY == 0)
        ToMulti();
      ZoopsY -= 1;
      AddChild(offset);
    }
    CountObjects();
    Helper.GetPlayer().SetupPlacementGhost();
  }
  public void ZoopForward(string offset) {
    if (Type == SelectedType.Multiple && ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 0) return;
    if (Type == SelectedType.Command) return;
    if (Type == SelectedType.Location) return;
    if (ZoopsX != 0 || ZoopsY != 0)
      ToSingle();
    if (ZoopsZ == -1) {
      ToSingle();
    } else if (ZoopsZ < 0) {
      RemoveLast();
      ZoopsZ += 1;
    } else {
      if (ZoopsZ == 0)
        ToMulti();
      ZoopsZ += 1;
      AddChild(offset);
    }
    CountObjects();
    Helper.GetPlayer().SetupPlacementGhost();
  }
  public void ZoopBackward(string offset) {
    if (Type == SelectedType.Multiple && ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 0) return;
    if (Type == SelectedType.Command) return;
    if (Type == SelectedType.Location) return;
    if (ZoopsX != 0 || ZoopsY != 0)
      ToSingle();
    if (ZoopsZ == 1) {
      ToSingle();
    } else if (ZoopsZ > 0) {
      RemoveLast();
      ZoopsZ -= 1;
    } else {
      if (ZoopsZ == 0)
        ToMulti();
      ZoopsZ -= 1;
      AddChild(offset);
    }
    CountObjects();
    Helper.GetPlayer().SetupPlacementGhost();
  }
}
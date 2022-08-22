using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityHammer;
public partial class Selected {
  private int ZoopsX = 0;
  private int ZoopsY = 0;
  private int ZoopsZ = 0;
  private Vector3 ZoopOffset = new();
  private Dictionary<Vector3Int, GameObject> Zoops = new();
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
    Zoops.Clear();
  }
  private void ToMulti() {
    if (Type == SelectedType.Multiple) return;
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
    obj.transform.localScale = Vector3.one;
    obj.SetActive(true);
    AddSnapPoints(obj);
    if (obj.GetComponent<Piece>() is { } piece) {
      var name2 = Utils.GetPrefabName(obj);
      if (ZNetScene.instance.GetPrefab(name2) is { } prefab) {
        if (!prefab.GetComponent<Piece>()) UnityEngine.Object.Destroy(piece);
      }
    }
  }
  private int Sign(int value) => value >= 0 ? 1 : -1;
  private void RemoveChildSub(Vector3Int index) {
    if (Objects.Count > 0)
      Objects.RemoveAt(0);
    var obj = Zoops[index];
    obj.SetActive(false);
    obj.transform.parent = null;
    UnityEngine.Object.Destroy(obj);
    Zoops.Remove(index);
  }
  private void RemoveChildX() {
    for (var y = 0; Math.Abs(y) <= Math.Abs(ZoopsY); y += Sign(ZoopsY))
      for (var z = 0; Math.Abs(z) <= Math.Abs(ZoopsZ); z += Sign(ZoopsZ))
        RemoveChildSub(new(ZoopsX, y, z));
  }
  private void RemoveChildY() {
    for (var x = 0; Math.Abs(x) <= Math.Abs(ZoopsX); x += Sign(ZoopsX))
      for (var z = 0; Math.Abs(z) <= Math.Abs(ZoopsZ); z += Sign(ZoopsZ))
        RemoveChildSub(new(x, ZoopsY, z));
  }
  private void RemoveChildZ() {
    for (var x = 0; Math.Abs(x) <= Math.Abs(ZoopsX); x += Sign(ZoopsX))
      for (var y = 0; Math.Abs(y) <= Math.Abs(ZoopsY); y += Sign(ZoopsY))
        RemoveChildSub(new(x, y, ZoopsZ));
  }
  private void AddChildSub(Vector3Int index) {
    var pos = GetOffset(index);
    var baseObj = Ghost.transform.GetChild(0).gameObject;
    var obj = Helper.SafeInstantiate(baseObj, Ghost);
    obj.SetActive(true);
    obj.transform.rotation = baseObj.transform.rotation;
    obj.transform.localPosition = pos;
    Objects.Add(new SelectedObject(Objects[0].Prefab, Objects[0].Scalable, Objects[0].Data));
    Zoops[index] = obj;
  }
  private void AddChildX(string offset) {
    UpdateOffsetX(offset);
    ZNetView.m_forceDisableInit = true;
    for (var y = 0; Math.Abs(y) <= Math.Abs(ZoopsY); y += Sign(ZoopsY))
      for (var z = 0; Math.Abs(z) <= Math.Abs(ZoopsZ); z += Sign(ZoopsZ))
        AddChildSub(new(ZoopsX, y, z));
    ZNetView.m_forceDisableInit = false;
  }
  private void AddChildY(string offset) {
    UpdateOffsetY(offset);
    ZNetView.m_forceDisableInit = true;
    for (var x = 0; Math.Abs(x) <= Math.Abs(ZoopsX); x += Sign(ZoopsX))
      for (var z = 0; Math.Abs(z) <= Math.Abs(ZoopsZ); z += Sign(ZoopsZ))
        AddChildSub(new(x, ZoopsY, z));
    ZNetView.m_forceDisableInit = false;
  }

  private void AddChildZ(string offset) {
    UpdateOffsetZ(offset);
    ZNetView.m_forceDisableInit = true;
    for (var x = 0; Math.Abs(x) <= Math.Abs(ZoopsX); x += Sign(ZoopsX))
      for (var y = 0; Math.Abs(y) <= Math.Abs(ZoopsY); y += Sign(ZoopsY))
        AddChildSub(new(x, y, ZoopsZ));
    ZNetView.m_forceDisableInit = false;
  }
  private static GameObject SnapObj = new() {
    name = "_snappoint",
    layer = LayerMask.NameToLayer("piece"),
    tag = "snappoint",
  };
  private Vector3 GetOffset(Vector3Int index) {
    var offset = ZoopOffset;
    offset.x *= index.x;
    offset.y *= index.y;
    offset.z *= index.z;
    return offset;
  }
  private void UpdateOffsetX(string offset) {
    var baseObj = Ghost.transform.GetChild(0).gameObject;
    var size = Helper.ParseSize(baseObj, offset);
    ZoopOffset.x = size.x * Ghost.transform.localScale.z;
  }
  private void UpdateOffsetY(string offset) {
    var baseObj = Ghost.transform.GetChild(0).gameObject;
    var size = Helper.ParseSize(baseObj, offset);
    ZoopOffset.y = size.y * Ghost.transform.localScale.y;
  }
  private void UpdateOffsetZ(string offset) {
    var baseObj = Ghost.transform.GetChild(0).gameObject;
    var size = Helper.ParseSize(baseObj, offset);
    ZoopOffset.z = size.z * Ghost.transform.localScale.z;
  }
  private void ZoopPostprocess() {
    CountObjects();
    var scale = Scaling.Build.Value;
    Helper.GetPlayer().SetupPlacementGhost();
    Scaling.Build.SetScale(scale);
  }
  public void ZoopRight(string offset) {
    if (Type == SelectedType.Multiple && ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 0) return;
    if (Type == SelectedType.Command) return;
    if (Type == SelectedType.Location) return;
    if (ZoopsX == -1 && ZoopsY == 0 && ZoopsZ == 0) {
      ToSingle();
    } else if (ZoopsX < 0) {
      RemoveChildX();
      ZoopsX += 1;
    } else {
      ToMulti();
      ZoopsX += 1;
      AddChildX(offset);
    }
    ZoopPostprocess();
  }
  public void ZoopLeft(string offset) {
    if (Type == SelectedType.Multiple && ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 0) return;
    if (Type == SelectedType.Command) return;
    if (Type == SelectedType.Location) return;
    if (ZoopsX == 1 && ZoopsY == 0 && ZoopsZ == 0) {
      ToSingle();
    } else if (ZoopsX > 0) {
      RemoveChildX();
      ZoopsX -= 1;
    } else {
      ToMulti();
      ZoopsX -= 1;
      AddChildX(offset);
    }
    ZoopPostprocess();
  }
  public void ZoopUp(string offset) {
    if (Type == SelectedType.Multiple && ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 0) return;
    if (Type == SelectedType.Command) return;
    if (Type == SelectedType.Location) return;
    if (ZoopsX == 0 && ZoopsY == -1 && ZoopsZ == 0) {
      ToSingle();
    } else if (ZoopsY < 0) {
      RemoveChildY();
      ZoopsY += 1;
    } else {
      ToMulti();
      ZoopsY += 1;
      AddChildY(offset);
    }
    ZoopPostprocess();
  }
  public void ZoopDown(string offset) {
    if (Type == SelectedType.Multiple && ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 0) return;
    if (Type == SelectedType.Command) return;
    if (Type == SelectedType.Location) return;
    if (ZoopsX == 0 && ZoopsY == 1 && ZoopsZ == 0) {
      ToSingle();
    } else if (ZoopsY > 0) {
      RemoveChildY();
      ZoopsY -= 1;
    } else {
      ToMulti();
      ZoopsY -= 1;
      AddChildY(offset);
    }
    ZoopPostprocess();
  }
  public void ZoopForward(string offset) {
    if (Type == SelectedType.Multiple && ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 0) return;
    if (Type == SelectedType.Command) return;
    if (Type == SelectedType.Location) return;
    if (ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == -1) {
      ToSingle();
    } else if (ZoopsZ < 0) {
      RemoveChildZ();
      ZoopsZ += 1;
    } else {
      ToMulti();
      ZoopsZ += 1;
      AddChildZ(offset);
    }
    ZoopPostprocess();
  }
  public void ZoopBackward(string offset) {
    if (Type == SelectedType.Multiple && ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 0) return;
    if (Type == SelectedType.Command) return;
    if (Type == SelectedType.Location) return;
    if (ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 1) {
      ToSingle();
    } else if (ZoopsZ > 0) {
      RemoveChildZ();
      ZoopsZ -= 1;
    } else {
      ToMulti();
      ZoopsZ -= 1;
      AddChildZ(offset);
    }
    ZoopPostprocess();
  }
}
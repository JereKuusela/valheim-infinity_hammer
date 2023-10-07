using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityHammer;

public class ValheimRAFT {
  private static KeyValuePair<int, int> RaftParent = ZDO.GetHashZDOID("MBParent");

  public static bool IsValid(ZDO zdo) => zdo != null && zdo.IsValid();
  public static bool IsValid(ZNetView view) => view && IsValid(view.GetZDO());
  public static ZNetView[] GetConnectedRaft(ZNetView baseView, HashSet<int> ignoredPrefabs) {
    var id = baseView.GetZDO().GetZDOID(RaftParent);
    var instances = ZNetScene.instance.m_instances.Values;
    return instances
      .Where(IsValid)
      .Where(view => !ignoredPrefabs.Contains(view.GetZDO().m_prefab))
      .Where(view => view.GetZDO().m_uid == id || view.GetZDO().GetZDOID(RaftParent) == id)
      .ToArray();
  }
  private static ZDOID Spawn(List<GameObject> children) {
    for (var i = 0; i < children.Count; i++) {
      var child = children[i];
      var name = Utils.GetPrefabName(child);
      if (name != "MBRaft") continue;
      var prefab = ZNetScene.instance.GetPrefab(name);
      DataHelper.Init(name, child.transform, Selection.GetData(i));
      var childObj = UnityEngine.Object.Instantiate(prefab, child.transform.position, child.transform.rotation);
      Hammer.PostProcessPlaced(childObj);
      return childObj.GetComponent<ZNetView>()?.GetZDO()?.m_uid ?? ZDOID.None;
    }
    return ZDOID.None;
  }
  public static void Handle(List<GameObject> children) {
    var raft = Spawn(children);
    if (raft == ZDOID.None) return;
    for (var i = 0; i < children.Count; i++) {
      var data = Selection.Objects[i].Data;
      if (data.GetZDOID(RaftParent) != ZDOID.None)
        data.Set(RaftParent, raft);
    }
  }
  public static bool IsRaft(string name) => name == "MBRaft";
  public static bool IsInRaft(ZNetView view) => IsRaft(view.GetPrefabName()) || view.GetZDO().GetZDOID(RaftParent) != ZDOID.None;
}
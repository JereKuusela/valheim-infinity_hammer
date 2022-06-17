using System;
using UnityEngine;
namespace InfinityHammer;

public class RulerParameters {
  public float? Radius;
  public float? Width;
  public float? Depth;
}
public class Ruler {
  private static GameObject? Projector = null;
  private static CircleProjector? BaseProjector = null;

  private static CircleProjector GetBaseProjector() {
    var workbench = ZNetScene.instance.GetPrefab("piece_workbench");
    if (!workbench) throw new InvalidOperationException("Error: Unable to find the workbench object.");
    BaseProjector = workbench.GetComponentInChildren<CircleProjector>();
    return BaseProjector;
  }
  public static void Update() {
    if (Projector == null || !Player.m_localPlayer) return;
    var ghost = Player.m_localPlayer.m_placementGhost;
    Projector.SetActive(ghost);
    if (!ghost) return;
    Projector.transform.position = ghost.transform.position;
    Projector.transform.rotation = ghost.transform.rotation;
    if (Projector.GetComponent<CircleProjector>() is { } circle) {
      circle.m_radius = ghost.transform.localScale.x;
      circle.m_nrOfSegments = Math.Max(3, (int)(circle.m_radius * 4));
    }
    if (Projector.GetComponent<RectangleProjector>() is { } rect) {
      rect.m_width = ghost.transform.localScale.x;
      rect.m_depth = ghost.transform.localScale.z;
      rect.m_nrOfSegments = Math.Max(3, (int)((rect.m_depth + rect.m_width) * 2));
    }
  }

  private static GameObject InitializeGameObject(RulerParameters pars) {
    Projector = new();
    Projector.layer = LayerMask.NameToLayer("character_trigger");
    return Projector;
  }
  public static void InitializeProjector(RulerParameters pars, GameObject obj) {
    if (BaseProjector == null)
      BaseProjector = GetBaseProjector();
    if (pars.Radius.HasValue) {
      var circle = obj.AddComponent<CircleProjector>();
      circle.m_prefab = BaseProjector.m_prefab;
      circle.m_mask = BaseProjector.m_mask;
      circle.m_nrOfSegments = 3;
      Scaling.SetScale(pars.Radius.Value);
    }
    if (pars.Depth.HasValue && pars.Width.HasValue) {
      var rect = obj.AddComponent<RectangleProjector>();
      rect.m_prefab = BaseProjector.m_prefab;
      rect.m_mask = BaseProjector.m_mask;
      Scaling.SetScale(new Vector3(pars.Width.Value / 2f, 1f, pars.Depth.Value / 2f));
      rect.m_nrOfSegments = 3;
    }
  }
  public static void Create(RulerParameters pars) {
    Remove();
    if (pars.Radius == null && pars.Width == null && pars.Depth == null) return;
    var obj = InitializeGameObject(pars);
    InitializeProjector(pars, InitializeGameObject(pars));
  }

  public static void Remove() {
    if (Projector != null)
      UnityEngine.Object.Destroy(Projector);
    Projector = null;
  }
}

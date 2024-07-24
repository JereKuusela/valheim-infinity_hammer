using System;
using System.Collections.Generic;
using InfinityHammer;
using UnityEngine;
namespace InfinityTools;

public abstract class BaseRuler : MonoBehaviour
{
  private static LayerMask Mask = LayerMask.GetMask("terrain");
  // No direct way for the projector prefab, so it must be acquired from some object.
  private static GameObject? basePrefab;
  private static GameObject BasePrefab => basePrefab ??= GetBasePrefab();
  protected static Vector3 BaseScale => BasePrefab.transform.localScale;
  private static GameObject GetBasePrefab()
  {
    var workbench = ZNetScene.instance.GetPrefab("piece_workbench");
    if (!workbench) throw new InvalidOperationException("Error: Unable to find the workbench object.");
    return workbench.GetComponentInChildren<CircleProjector>().m_prefab;
  }

  public static bool SnapToGround = true;
  public bool Visible = true;

  public void OnDestroy()
  {
    foreach (GameObject obj in Segments) Destroy(obj);
    foreach (GameObject obj in OffsetSegments) Destroy(obj);
    Segments.Clear();
    OffsetSegments.Clear();
  }
  public void Update()
  {
    if (!Visible)
    {
      CreateSegments(0);
      CreateOffsetSegments(0);
      return;
    }
    CreateLines();
    if (SnapToGround)
      Snap();

    CreateOffsetSegments(Segments.Count);
    CreateOffsetLines();
  }

  protected List<GameObject> Segments = [];
  protected void CreateSegments(int count)
  {
    if (Segments.Count == count) return;
    foreach (GameObject obj in Segments) Destroy(obj);
    Segments.Clear();
    for (int i = 0; i < count; i++)
      Segments.Add(Instantiate(BasePrefab, transform));
  }
  protected abstract void CreateLines();
  private void Snap()
  {
    foreach (var segment in Segments)
      segment.transform.position = Snap(segment.transform.position);
  }
  private Vector3 Snap(Vector3 pos)
  {
    if (Physics.Raycast(pos + Vector3.up * 500f, Vector3.down, out var raycastHit, 1000f, Mask.value))
      pos.y = raycastHit.point.y;
    return pos;
  }

  // Offset is simply copy of the actual segments, so no special implementation is needed.
  public static float Offset = 0f;
  private readonly List<GameObject> OffsetSegments = [];
  private void CreateOffsetSegments(int count)
  {
    if (Mathf.Approximately(Offset, 0f)) count = 0;
    if (OffsetSegments.Count == count) return;
    foreach (GameObject obj in OffsetSegments) Destroy(obj);
    OffsetSegments.Clear();
    for (int i = 0; i < count; i++)
      OffsetSegments.Add(Instantiate(BasePrefab, Vector3.zero, Quaternion.identity, transform));
  }
  private void CreateOffsetLines()
  {
    for (int i = 0; i < OffsetSegments.Count; i++)
    {
      var segment = Segments[i];
      var offsetSegment = OffsetSegments[i];
      offsetSegment.transform.localPosition = segment.transform.localPosition + Vector3.up * Offset;
      offsetSegment.transform.localRotation = segment.transform.localRotation;
      offsetSegment.transform.localScale = segment.transform.localScale;
      offsetSegment.SetActive(segment.activeSelf);
    }
  }

  // Helper functions for the derived classes.
  protected Transform Get(int index) => Segments[index].transform;
  protected void Set(int index, Vector3 pos) => Get(index).localPosition = pos;
  protected void SetRot(int index, Vector3 rot) => Get(index).localRotation = Quaternion.LookRotation(rot, Vector3.up);
}

public class CircleRuler : BaseRuler
{
  public float Radius = 20f;
  protected override void CreateLines()
  {
    var count = Math.Max(3, (int)(Radius * 4));
    CreateSegments(count);
    UpdateLines();
  }
  private readonly float Speed = 0.1f;
  private void UpdateLines()
  {
    var count = Segments.Count;
    var angle = 6.2831855f / count;
    var time = Time.time * Speed;
    for (int i = 0; i < count; i++)
    {
      var f = i * angle + time;
      var vector = new Vector3(Mathf.Sin(f) * Radius, 0f, Mathf.Cos(f) * Radius);
      Set(i, vector);
    }
    for (int i = 0; i < count; i++)
    {
      var prev = i == 0 ? Segments[Segments.Count - 1] : Segments[i - 1];
      var next = i == Segments.Count - 1 ? Segments[0] : Segments[i + 1];
      Vector3 normalized = (prev.transform.localPosition - next.transform.localPosition).normalized;
      SetRot(i, normalized);
    }

  }
}

public class RectangleRuler : BaseRuler
{
  public float Width = 20f;
  public float Depth = 20f;
  private void EdgeFix(int index, float percent, float max, float start, float end, Vector3 direction)
  {
    var pos = percent * max;
    var tr = Get(index);
    var scale = BaseScale;
    if (pos - start < 0.5f)
    {
      scale.z *= pos - start + 0.5f;
      tr.localPosition += direction * (0.5f - scale.z / 2f);
    }
    if (end - pos < 0.5f)
    {
      scale.z *= Mathf.Max(0f, end - pos + 0.5f);
      tr.localPosition -= direction * (0.5f - scale.z / 2f);
    }
    if (scale.z == 0f) tr.gameObject.SetActive(false);
    else tr.gameObject.SetActive(true);
    tr.localScale = scale;
  }
  protected override void CreateLines()
  {
    var totalLength = 2 * Width + 2 * Depth;
    var count = Math.Max(3, (int)totalLength);
    var d = (int)Mathf.Max(2, Mathf.Ceil(count * Depth / totalLength));
    var w = (int)Mathf.Max(2, Mathf.Ceil(count * Width / totalLength));
    count = 2 * (d + w);
    CreateSegments(count);
    UpdateLines(w, d);
  }
  private void UpdateLines(int w, int d)
  {
    var count = Segments.Count;
    var index = 0;
    for (int i = 0; i < d; i++, index++)
      SetRot(index, Vector3.forward);
    for (int i = 0; i < w; i++, index++)
      SetRot(index, Vector3.right);
    for (int i = 0; i < d; i++, index++)
      SetRot(index, Vector3.back);
    for (int i = 0; i < w; i++, index++)
      SetRot(index, Vector3.left);
    index = 0;
    var baseTime = Time.time * 0.025f * (count - 4);
    var halfLine = 0.5f;
    var basePos = Width * Vector3.left - (Depth + halfLine) * Vector3.forward;
    var start = 0.5f;
    var end = start + 2f * Depth;
    var size = 2f * Depth * d / (d - 1);
    var time = baseTime / d;
    for (int i = 0; i < d; i++, index++)
    {
      var percent = ((float)i / d + time) % 1f;
      var pos = basePos + percent * size * Vector3.forward;
      Set(index, pos);
      EdgeFix(index, percent, size, start, end, Vector3.forward);
    }

    basePos = Depth * Vector3.forward - (Width + halfLine) * Vector3.right;
    end = start + 2f * Width;
    size = 2f * Width * w / (w - 1);
    time = baseTime / w;
    for (int i = 0; i < w; i++, index++)
    {
      var percent = ((float)i / w + time) % 1f;
      var pos = basePos + percent * size * Vector3.right;
      Set(index, pos);
      EdgeFix(index, percent, size, start, end, Vector3.right);
    }

    basePos = Width * Vector3.right - (Depth + halfLine) * Vector3.back;
    end = start + 2f * Depth;
    size = 2f * Depth * d / (d - 1);
    time = baseTime / d;
    for (int i = 0; i < d; i++, index++)
    {
      var percent = ((float)i / d + time) % 1f;
      var pos = basePos + percent * size * Vector3.back;
      Set(index, pos);
      EdgeFix(index, percent, size, start, end, Vector3.back);
    }

    basePos = Depth * Vector3.back - (Width + halfLine) * Vector3.left;
    end = start + 2f * Width;
    size = 2f * Width * w / (w - 1);
    time = baseTime / w;
    for (int i = 0; i < w; i++, index++)
    {
      var percent = ((float)i / w + time) % 1f;
      var pos = basePos + percent * size * Vector3.left;
      Set(index, pos);
      EdgeFix(index, percent, size, start, end, Vector3.left);
    }
  }
}
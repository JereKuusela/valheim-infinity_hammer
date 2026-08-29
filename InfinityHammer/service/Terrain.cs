using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;
using WorldEditCommands;

namespace Service;

public abstract class TerrainChannelData<TValue> where TValue : struct
{
  public Vector3 CenterPosition;
  public Quaternion CenterRotation = Quaternion.identity;
  public float DistanceBetweenNodes = 1.0f;
  public float Smooth = 0f;
  public int Width = 0;
  public int Height = 0;

  public float GetRadius() => GetRequiredRadius();

  public float GetSmoothWeight(Vector3 nodePos, Vector3 placementPos, Quaternion relativeRotation, float smooth)
  {
    smooth = Mathf.Clamp01(smooth);
    if (smooth <= 0f)
      return 1f;

    var localPos = Quaternion.Inverse(relativeRotation) * (nodePos - placementPos);
    var localX = localPos.x - CenterPosition.x;
    var localZ = localPos.z - CenterPosition.z;
    var radius = GetRequiredRadius();
    if (radius <= 0f)
      return 1f;

    var distance = Mathf.Sqrt(localX * localX + localZ * localZ);
    var innerRadius = radius * (1f - smooth);
    if (distance <= innerRadius)
      return 1f;
    if (distance >= radius)
      return 0f;

    var falloffRange = Mathf.Max(0.001f, radius - innerRadius);
    return Mathf.Clamp01(1f - ((distance - innerRadius) / falloffRange));
  }

  protected void InitializeReference(int width, int height, Vector3 centerPos)
  {
    Width = width;
    Height = height;
    CenterPosition = centerPos;
  }

  private float GetRequiredRadius()
  {
    if (Width <= 0 || Height <= 0)
      return 0f;

    var spacing = Mathf.Max(0.001f, DistanceBetweenNodes);
    var halfOffsetX = (Width - 1) * spacing * 0.5f;
    var halfOffsetZ = (Height - 1) * spacing * 0.5f;
    var firstNode = CenterPosition - new Vector3(halfOffsetX, 0f, halfOffsetZ);
    var radius = 0f;
    for (var x = 0; x < Width; x++)
    {
      for (var z = 0; z < Height; z++)
      {
        if (!Get(x, z).HasValue)
          continue;
        var node = firstNode + new Vector3(x * spacing, 0f, z * spacing);
        radius = Mathf.Max(radius, Utils.LengthXZ(node));
      }
    }
    return radius;
  }

  public void SetReference(Vector3 center, Quaternion rotation)
  {
    CenterPosition -= center;
    CenterRotation = GetYawRotation(rotation);
    OnSetReference(center);
  }

  protected static Quaternion GetYawRotation(Quaternion rotation)
  {
    var forward = rotation * Vector3.forward;
    forward.y = 0f;
    if (forward.sqrMagnitude <= 0.0001f)
      return Quaternion.identity;
    return Quaternion.LookRotation(forward.normalized, Vector3.up);
  }

  protected virtual void OnSetReference(Vector3 center)
  {
  }

  protected Vector3 GetFirstNodeAnchor()
  {
    var spacing = Mathf.Max(0.001f, DistanceBetweenNodes);
    var halfOffsetX = (Width - 1) * spacing * 0.5f;
    var halfOffsetZ = (Height - 1) * spacing * 0.5f;
    return new Vector3(CenterPosition.x - halfOffsetX, CenterPosition.y, CenterPosition.z - halfOffsetZ);
  }

  protected static bool TryGetGridCoordinates(Vector3 nodePos, Vector3 placementPos, Quaternion relativeRotation, Vector3 firstNodeAnchor, float distanceBetweenNodes, out int gridX, out int gridZ)
  {
    var spacing = Mathf.Max(0.001f, distanceBetweenNodes);

    var relativeToPlacement = nodePos - placementPos;
    // Inverse-rotate into blueprint-local terrain space.
    var localPos = Quaternion.Inverse(relativeRotation) * relativeToPlacement;
    var localX = localPos.x;
    var localZ = localPos.z;

    var fromFirstNodeX = localX - firstNodeAnchor.x;
    var fromFirstNodeZ = localZ - firstNodeAnchor.z;

    gridX = Mathf.RoundToInt(fromFirstNodeX / spacing);
    gridZ = Mathf.RoundToInt(fromFirstNodeZ / spacing);
    return true;
  }

  public abstract TValue? Get(int x, int z);
  public abstract void Set(int x, int z, TValue value);
  public abstract TValue? FindNearest(Vector3 nodePos, Vector3 placementPos, Quaternion relativeRotation);
}

public class TerrainHeight : TerrainChannelData<float>
{
  public float?[,] Heights = new float?[0, 0];

  public void InitializeGrid(int width, int height, Vector3 centerPosition)
  {
    InitializeReference(width, height, centerPosition);
    Heights = new float?[width, height];
  }

  public override float? Get(int x, int z)
  {
    if (x >= 0 && x < Width && z >= 0 && z < Height)
      return Heights[x, z];
    return null;
  }

  public override void Set(int x, int z, float value)
  {
    if (x >= 0 && x < Width && z >= 0 && z < Height)
      Heights[x, z] = value;
  }

  public override float? FindNearest(Vector3 nodePos, Vector3 placementPos, Quaternion relativeRotation)
  {
    if (!TryGetGridCoordinates(nodePos, placementPos, relativeRotation, GetFirstNodeAnchor(), DistanceBetweenNodes, out var gridX, out var gridZ))
      return null;
    if (gridX < 0 || gridX >= Width || gridZ < 0 || gridZ >= Height)
      return null;
    return Get(gridX, gridZ);
  }

  protected override void OnSetReference(Vector3 center)
  {
    for (int x = 0; x < Width; x++)
    {
      for (int z = 0; z < Height; z++)
      {
        if (Heights[x, z].HasValue)
          Heights[x, z] -= center.y;
      }
    }
  }

  public TerrainHeight Clone()
  {
    var clone = new TerrainHeight
    {
      CenterPosition = CenterPosition,
      CenterRotation = CenterRotation,
      DistanceBetweenNodes = DistanceBetweenNodes,
      Smooth = Smooth,
      Width = Width,
      Height = Height,
      Heights = new float?[Width, Height]
    };

    for (int x = 0; x < Width; x++)
    {
      for (int z = 0; z < Height; z++)
      {
        clone.Heights[x, z] = Heights[x, z];
      }
    }

    return clone;
  }
}

public class TerrainPaint : TerrainChannelData<Color>
{
  public Color?[,] Paints = new Color?[0, 0];

  public void InitializeGrid(int width, int height, Vector3 centerPosition)
  {
    InitializeReference(width, height, centerPosition);
    Paints = new Color?[width, height];
  }

  public override Color? Get(int x, int z)
  {
    if (x >= 0 && x < Width && z >= 0 && z < Height)
      return Paints[x, z];
    return null;
  }

  public override void Set(int x, int z, Color value)
  {
    if (x >= 0 && x < Width && z >= 0 && z < Height)
      Paints[x, z] = value;
  }

  public override Color? FindNearest(Vector3 nodePos, Vector3 placementPos, Quaternion relativeRotation)
  {
    if (!TryGetGridCoordinates(nodePos, placementPos, relativeRotation, GetFirstNodeAnchor(), DistanceBetweenNodes, out var gridX, out var gridZ))
      return null;
    if (gridX < 0 || gridX >= Width || gridZ < 0 || gridZ >= Height)
      return null;
    return Get(gridX, gridZ);
  }

  public TerrainPaint Clone()
  {
    var clone = new TerrainPaint
    {
      CenterPosition = CenterPosition,
      CenterRotation = CenterRotation,
      DistanceBetweenNodes = DistanceBetweenNodes,
      Smooth = Smooth,
      Width = Width,
      Height = Height,
      Paints = new Color?[Width, Height]
    };

    for (int x = 0; x < Width; x++)
    {
      for (int z = 0; z < Height; z++)
      {
        clone.Paints[x, z] = Paints[x, z];
      }
    }

    return clone;
  }
}

public class TerrainInfo
{

  public static float ResolveRadius(TerrainHeight? height, TerrainPaint? paint)
  {
    var sampleRadius = Mathf.Max(height?.GetRadius() ?? 0f, paint?.GetRadius() ?? 0f);
    // FindNearest rounds to the nearest snapshot node, so the affected area
    // extends beyond the outer node centers. Include a conservative node/grid
    // margin when finding compilers, recording undo and refreshing clutter.
    var samplingMargin = Mathf.Max(1f, height?.DistanceBetweenNodes ?? 0f, paint?.DistanceBetweenNodes ?? 0f);
    return sampleRadius + samplingMargin;
  }

  private static Vector3 VertexToWorld(Heightmap hmap, int x, int z)
  {
    var vector = hmap.transform.position;
    vector.x += (x - hmap.m_width / 2) * hmap.m_scale;
    vector.z += (z - hmap.m_width / 2) * hmap.m_scale;
    return vector;
  }

  private static float GetX(float x, float z, float angle) => Mathf.Cos(angle) * x - Mathf.Sin(angle) * z;
  private static float GetZ(float x, float z, float angle) => Mathf.Sin(angle) * x + Mathf.Cos(angle) * z;

  public static TerrainHeight CollectTerrainHeightInRadius(Vector3 centerPos, Quaternion centerRot, Vector3 searchPos, Range<float> radius, float nodeSpacingOverride = 0f)
  {
    var compilers = Terrain.GetCompilers(searchPos, new(radius.Max)).ToList();
    var data = MergeHeightmapsWithCircle(compilers, searchPos, radius, nodeSpacingOverride);
    data.SetReference(centerPos, centerRot);
    return data;
  }

  public static TerrainPaint CollectTerrainPaintInRadius(Vector3 centerPos, Quaternion centerRot, Vector3 searchPos, Range<float> radius, float nodeSpacingOverride = 0f)
  {
    var compilers = Terrain.GetCompilers(searchPos, new(radius.Max)).ToList();
    var data = MergePaintmapsWithCircle(compilers, searchPos, radius, nodeSpacingOverride);
    data.SetReference(centerPos, centerRot);
    return data;
  }

  public static TerrainHeight CollectTerrainHeightInRect(Vector3 centerPos, Quaternion centerRot, Vector3 searchPos, Range<float> width, Range<float> depth, float angle, float nodeSpacingOverride = 0f)
  {
    var compilers = Terrain.GetCompilers(searchPos, width, depth, angle).ToList();
    var data = MergeHeightmapsWithRect(compilers, searchPos, width, depth, angle, nodeSpacingOverride);
    data.SetReference(centerPos, centerRot);
    return data;
  }

  public static TerrainPaint CollectTerrainPaintInRect(Vector3 centerPos, Quaternion centerRot, Vector3 searchPos, Range<float> width, Range<float> depth, float angle, float nodeSpacingOverride = 0f)
  {
    var compilers = Terrain.GetCompilers(searchPos, width, depth, angle).ToList();
    var data = MergePaintmapsWithRect(compilers, searchPos, width, depth, angle, nodeSpacingOverride);
    data.SetReference(centerPos, centerRot);
    return data;
  }

  private static float ResolveNodeSpacing(List<TerrainComp> compilers, float nodeSpacingOverride)
  {
    if (nodeSpacingOverride > 0f)
      return Mathf.Max(0.001f, nodeSpacingOverride);
    if (compilers.Count == 0)
      return 1f;
    return Mathf.Max(0.001f, compilers[0].m_hmap.m_scale);
  }

  private static TerrainHeight MergeHeightmapsWithCircle(List<TerrainComp> compilers, Vector3 centerPos, Range<float> radius, float nodeSpacingOverride)
  {
    var mergedData = new TerrainHeight();
    if (compilers.Count == 0)
      return mergedData;

    var spacing = ResolveNodeSpacing(compilers, nodeSpacingOverride);

    float minWorldX = float.MaxValue;
    float maxWorldX = float.MinValue;
    float minWorldZ = float.MaxValue;
    float maxWorldZ = float.MinValue;
    bool hasAnyNodes = false;

    foreach (var compiler in compilers)
    {
      var max = compiler.m_width + 1;
      for (int x = 0; x < max; x++)
      {
        for (int z = 0; z < max; z++)
        {
          var nodePos = VertexToWorld(compiler.m_hmap, x, z);
          var distance = Utils.DistanceXZ(centerPos, nodePos);
          if (!Helper.Within(radius, distance))
            continue;

          hasAnyNodes = true;
          minWorldX = Math.Min(minWorldX, nodePos.x);
          maxWorldX = Math.Max(maxWorldX, nodePos.x);
          minWorldZ = Math.Min(minWorldZ, nodePos.z);
          maxWorldZ = Math.Max(maxWorldZ, nodePos.z);
        }
      }
    }

    if (!hasAnyNodes)
      return mergedData;

    int gridWidth = Mathf.RoundToInt((maxWorldX - minWorldX) / spacing) + 1;
    int gridHeight = Mathf.RoundToInt((maxWorldZ - minWorldZ) / spacing) + 1;

    mergedData.InitializeGrid(
      gridWidth,
      gridHeight,
      new Vector3(minWorldX + (gridWidth - 1) * spacing * 0.5f, 0f, minWorldZ + (gridHeight - 1) * spacing * 0.5f)
    );
    mergedData.DistanceBetweenNodes = spacing;

    foreach (var compiler in compilers)
    {
      var max = compiler.m_width + 1;
      for (int x = 0; x < max; x++)
      {
        for (int z = 0; z < max; z++)
        {
          var nodePos = VertexToWorld(compiler.m_hmap, x, z);
          var distance = Utils.DistanceXZ(centerPos, nodePos);
          if (!Helper.Within(radius, distance))
            continue;

          var index = z * max + x;
          if (index >= compiler.m_hmap.m_heights.Count)
            continue;

          int gridX = Mathf.RoundToInt((nodePos.x - minWorldX) / spacing);
          int gridZ = Mathf.RoundToInt((nodePos.z - minWorldZ) / spacing);
          if (mergedData.Get(gridX, gridZ) == null)
            mergedData.Set(gridX, gridZ, compiler.m_hmap.m_heights[index]);
        }
      }
    }

    return mergedData;
  }

  private static TerrainPaint MergePaintmapsWithCircle(List<TerrainComp> compilers, Vector3 centerPos, Range<float> radius, float nodeSpacingOverride)
  {
    var mergedData = new TerrainPaint();
    if (compilers.Count == 0)
      return mergedData;

    var spacing = ResolveNodeSpacing(compilers, nodeSpacingOverride);

    float minWorldX = float.MaxValue;
    float maxWorldX = float.MinValue;
    float minWorldZ = float.MaxValue;
    float maxWorldZ = float.MinValue;
    bool hasAnyNodes = false;

    foreach (var compiler in compilers)
    {
      var max = compiler.m_width + 1;
      for (int x = 0; x < max; x++)
      {
        for (int z = 0; z < max; z++)
        {
          var nodePos = VertexToWorld(compiler.m_hmap, x, z);
          var distance = Utils.DistanceXZ(centerPos, nodePos);
          if (!Helper.Within(radius, distance))
            continue;

          hasAnyNodes = true;
          minWorldX = Math.Min(minWorldX, nodePos.x);
          maxWorldX = Math.Max(maxWorldX, nodePos.x);
          minWorldZ = Math.Min(minWorldZ, nodePos.z);
          maxWorldZ = Math.Max(maxWorldZ, nodePos.z);
        }
      }
    }

    if (!hasAnyNodes)
      return mergedData;

    int gridWidth = Mathf.RoundToInt((maxWorldX - minWorldX) / spacing) + 1;
    int gridHeight = Mathf.RoundToInt((maxWorldZ - minWorldZ) / spacing) + 1;

    mergedData.InitializeGrid(
      gridWidth,
      gridHeight,
      new Vector3(minWorldX + (gridWidth - 1) * spacing * 0.5f, 0f, minWorldZ + (gridHeight - 1) * spacing * 0.5f)
    );
    mergedData.DistanceBetweenNodes = spacing;

    foreach (var compiler in compilers)
    {
      var max = compiler.m_width + 1;
      for (int x = 0; x < max; x++)
      {
        for (int z = 0; z < max; z++)
        {
          var nodePos = VertexToWorld(compiler.m_hmap, x, z);
          var distance = Utils.DistanceXZ(centerPos, nodePos);
          if (!Helper.Within(radius, distance))
            continue;

          int gridX = Mathf.RoundToInt((nodePos.x - minWorldX) / spacing);
          int gridZ = Mathf.RoundToInt((nodePos.z - minWorldZ) / spacing);
          if (mergedData.Get(gridX, gridZ) == null)
            // TerrainComp only stores meaningful paint for modified cells.
            // Read the composed Heightmap so the snapshot contains final values.
            mergedData.Set(gridX, gridZ, compiler.m_hmap.GetPaintMask(x, z));
        }
      }
    }

    return mergedData;
  }

  private static TerrainHeight MergeHeightmapsWithRect(List<TerrainComp> compilers, Vector3 centerPos, Range<float> width, Range<float> depth, float angle, float nodeSpacingOverride)
  {
    var mergedData = new TerrainHeight();
    if (compilers.Count == 0)
      return mergedData;

    var spacing = ResolveNodeSpacing(compilers, nodeSpacingOverride);

    float minWorldX = float.MaxValue;
    float maxWorldX = float.MinValue;
    float minWorldZ = float.MaxValue;
    float maxWorldZ = float.MinValue;
    bool hasAnyNodes = false;

    foreach (var compiler in compilers)
    {
      var max = compiler.m_width + 1;
      for (int x = 0; x < max; x++)
      {
        for (int z = 0; z < max; z++)
        {
          var nodePos = VertexToWorld(compiler.m_hmap, x, z);
          var rawDx = nodePos.x - centerPos.x;
          var rawDz = nodePos.z - centerPos.z;
          var dx = GetX(rawDx, rawDz, angle);
          var dz = GetZ(rawDx, rawDz, angle);

          if (!Helper.Within(width, depth, Mathf.Abs(dx), Mathf.Abs(dz)))
            continue;

          hasAnyNodes = true;
          minWorldX = Math.Min(minWorldX, nodePos.x);
          maxWorldX = Math.Max(maxWorldX, nodePos.x);
          minWorldZ = Math.Min(minWorldZ, nodePos.z);
          maxWorldZ = Math.Max(maxWorldZ, nodePos.z);
        }
      }
    }

    if (!hasAnyNodes)
      return mergedData;

    int gridWidth = Mathf.RoundToInt((maxWorldX - minWorldX) / spacing) + 1;
    int gridHeight = Mathf.RoundToInt((maxWorldZ - minWorldZ) / spacing) + 1;

    mergedData.InitializeGrid(
      gridWidth,
      gridHeight,
      new Vector3(minWorldX + (gridWidth - 1) * spacing * 0.5f, 0f, minWorldZ + (gridHeight - 1) * spacing * 0.5f)
    );
    mergedData.DistanceBetweenNodes = spacing;

    foreach (var compiler in compilers)
    {
      var max = compiler.m_width + 1;
      for (int x = 0; x < max; x++)
      {
        for (int z = 0; z < max; z++)
        {
          var nodePos = VertexToWorld(compiler.m_hmap, x, z);
          var rawDx = nodePos.x - centerPos.x;
          var rawDz = nodePos.z - centerPos.z;
          var dx = GetX(rawDx, rawDz, angle);
          var dz = GetZ(rawDx, rawDz, angle);

          if (!Helper.Within(width, depth, Mathf.Abs(dx), Mathf.Abs(dz)))
            continue;

          var index = z * max + x;
          if (index >= compiler.m_hmap.m_heights.Count)
            continue;

          int gridX = Mathf.RoundToInt((nodePos.x - minWorldX) / spacing);
          int gridZ = Mathf.RoundToInt((nodePos.z - minWorldZ) / spacing);
          if (mergedData.Get(gridX, gridZ) == null)
            mergedData.Set(gridX, gridZ, compiler.m_hmap.m_heights[index]);
        }
      }
    }

    return mergedData;
  }

  private static TerrainPaint MergePaintmapsWithRect(List<TerrainComp> compilers, Vector3 centerPos, Range<float> width, Range<float> depth, float angle, float nodeSpacingOverride)
  {
    var mergedData = new TerrainPaint();
    if (compilers.Count == 0)
      return mergedData;

    var spacing = ResolveNodeSpacing(compilers, nodeSpacingOverride);

    float minWorldX = float.MaxValue;
    float maxWorldX = float.MinValue;
    float minWorldZ = float.MaxValue;
    float maxWorldZ = float.MinValue;
    bool hasAnyNodes = false;

    foreach (var compiler in compilers)
    {
      var max = compiler.m_width + 1;
      for (int x = 0; x < max; x++)
      {
        for (int z = 0; z < max; z++)
        {
          var nodePos = VertexToWorld(compiler.m_hmap, x, z);
          var rawDx = nodePos.x - centerPos.x;
          var rawDz = nodePos.z - centerPos.z;
          var dx = GetX(rawDx, rawDz, angle);
          var dz = GetZ(rawDx, rawDz, angle);

          if (!Helper.Within(width, depth, Mathf.Abs(dx), Mathf.Abs(dz)))
            continue;

          hasAnyNodes = true;
          minWorldX = Math.Min(minWorldX, nodePos.x);
          maxWorldX = Math.Max(maxWorldX, nodePos.x);
          minWorldZ = Math.Min(minWorldZ, nodePos.z);
          maxWorldZ = Math.Max(maxWorldZ, nodePos.z);
        }
      }
    }

    if (!hasAnyNodes)
      return mergedData;

    int gridWidth = Mathf.RoundToInt((maxWorldX - minWorldX) / spacing) + 1;
    int gridHeight = Mathf.RoundToInt((maxWorldZ - minWorldZ) / spacing) + 1;

    mergedData.InitializeGrid(
      gridWidth,
      gridHeight,
      new Vector3(minWorldX + (gridWidth - 1) * spacing * 0.5f, 0f, minWorldZ + (gridHeight - 1) * spacing * 0.5f)
    );
    mergedData.DistanceBetweenNodes = spacing;

    foreach (var compiler in compilers)
    {
      var max = compiler.m_width + 1;
      for (int x = 0; x < max; x++)
      {
        for (int z = 0; z < max; z++)
        {
          var nodePos = VertexToWorld(compiler.m_hmap, x, z);
          var rawDx = nodePos.x - centerPos.x;
          var rawDz = nodePos.z - centerPos.z;
          var dx = GetX(rawDx, rawDz, angle);
          var dz = GetZ(rawDx, rawDz, angle);

          if (!Helper.Within(width, depth, Mathf.Abs(dx), Mathf.Abs(dz)))
            continue;

          int gridX = Mathf.RoundToInt((nodePos.x - minWorldX) / spacing);
          int gridZ = Mathf.RoundToInt((nodePos.z - minWorldZ) / spacing);
          if (mergedData.Get(gridX, gridZ) == null)
            // See the circular capture path above: snapshot composed paint.
            mergedData.Set(gridX, gridZ, compiler.m_hmap.GetPaintMask(x, z));
        }
      }
    }

    return mergedData;
  }
}

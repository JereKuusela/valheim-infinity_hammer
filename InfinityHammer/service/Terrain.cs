using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;
using WorldEditCommands;

namespace Service;

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
    data.MapValues(value => value - centerPos.y);
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
    data.MapValues(value => value - centerPos.y);
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

    mergedData.Initialize(
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

    mergedData.Initialize(
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

    mergedData.Initialize(
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

    mergedData.Initialize(
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

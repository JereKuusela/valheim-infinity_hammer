using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using InfinityHammer;
using ServerDevcommands;
using WorldEditCommands;
using UnityEngine;
using System;

namespace Service;

public class TerrainData(Vector3 heightmapPosition)
{
  public Vector3 HeightmapPosition = heightmapPosition;
  public float?[,] Heights = new float?[0, 0];
  public Color?[,] Paints = new Color?[0, 0];
  public Vector3 FirstNodePosition;
  public Quaternion FirstNodeRotation = Quaternion.identity;
  public int Width = 0;
  public int Height = 0;

  public void SetReference(Vector3 center, Quaternion rotation)
  {
    FirstNodePosition -= center;
    FirstNodeRotation = rotation;
    for (int x = 0; x < Width; x++)
    {
      for (int z = 0; z < Height; z++)
      {
        if (Heights[x, z].HasValue)
        {
          Heights[x, z] -= center.y;
        }
      }
    }
  }

  public void InitializeGrid(int width, int height, Vector3 firstNodePos)
  {
    Width = width;
    Height = height;
    FirstNodePosition = firstNodePos;
    Heights = new float?[width, height];
    Paints = new Color?[width, height];
  }

  public void SetNode(int x, int z, float height, Color paint)
  {
    if (x >= 0 && x < Width && z >= 0 && z < Height)
    {
      Heights[x, z] = height;
      Paints[x, z] = paint;
    }
  }

  public float? GetHeight(int x, int z)
  {
    if (x >= 0 && x < Width && z >= 0 && z < Height)
      return Heights[x, z];
    return null;
  }

  public Color? GetPaint(int x, int z)
  {
    if (x >= 0 && x < Width && z >= 0 && z < Height)
      return Paints[x, z];
    return null;
  }

  public float? FindNearestHeight(Vector3 nodePos, Vector3 placementPos, float rotation)
  {
    // Calculate grid coordinates based on position relative to first node
    var relativePos = nodePos - FirstNodePosition - placementPos;

    // Apply rotation if needed - rotate around the placement position
    if (rotation != 0f)
    {
      // First translate to make placementPos the origin for rotation
      var rotationCenter = -FirstNodePosition;
      var posRelativeToCenter = nodePos - placementPos;

      // Apply rotation around the center
      var rotatedX = Mathf.Cos(rotation) * posRelativeToCenter.x - Mathf.Sin(rotation) * posRelativeToCenter.z;
      var rotatedZ = Mathf.Sin(rotation) * posRelativeToCenter.x + Mathf.Cos(rotation) * posRelativeToCenter.z;

      // Translate back
      relativePos.x = rotatedX + rotationCenter.x;
      relativePos.z = rotatedZ + rotationCenter.z;
    }

    int gridX = Mathf.RoundToInt(relativePos.x);
    int gridZ = Mathf.RoundToInt(relativePos.z);
    if (gridX < 0 || gridX >= Width || gridZ < 0 || gridZ >= Height)
      return null;
    return GetHeight(gridX, gridZ);
  }

  public Color? FindNearestPaint(Vector3 nodePos, Vector3 placementPos, float rotation)
  {
    // Calculate grid coordinates based on position relative to first node
    var relativePos = nodePos - FirstNodePosition - placementPos;

    // Apply rotation if needed - rotate around the placement position
    if (rotation != 0f)
    {
      // First translate to make placementPos the origin for rotation
      var rotationCenter = -FirstNodePosition;
      var posRelativeToCenter = nodePos - placementPos;

      // Apply rotation around the center
      var rotatedX = Mathf.Cos(rotation) * posRelativeToCenter.x - Mathf.Sin(rotation) * posRelativeToCenter.z;
      var rotatedZ = Mathf.Sin(rotation) * posRelativeToCenter.x + Mathf.Cos(rotation) * posRelativeToCenter.z;

      // Translate back
      relativePos.x = rotatedX + rotationCenter.x;
      relativePos.z = rotatedZ + rotationCenter.z;
    }

    int gridX = Mathf.RoundToInt(relativePos.x);
    int gridZ = Mathf.RoundToInt(relativePos.z);
    if (gridX < 0 || gridX >= Width || gridZ < 0 || gridZ >= Height)
      return null;
    return GetPaint(gridX, gridZ);
  }
}
public class TerrainInfo
{
  private static Vector3 VertexToWorld(Heightmap hmap, int x, int z)
  {
    var vector = hmap.transform.position;
    vector.x += (x - hmap.m_width / 2) * hmap.m_scale;
    vector.z += (z - hmap.m_width / 2) * hmap.m_scale;
    return vector;
  }

  private static float GetX(float x, float z, float angle) => Mathf.Cos(angle) * x - Mathf.Sin(angle) * z;
  private static float GetZ(float x, float z, float angle) => Mathf.Sin(angle) * x + Mathf.Cos(angle) * z;

  // New methods using improved terrain data structure
  public static TerrainData GetTerrainDataWithCircle(TerrainComp compiler, Vector3 centerPos, float radius)
  {
    var heightmapData = new TerrainData(compiler.m_hmap.transform.position);

    if (radius == 0f) return heightmapData;

    var max = compiler.m_width + 1;

    // Find bounds to determine grid size
    int minX = max, maxX = -1, minZ = max, maxZ = -1;
    Vector3 firstNodePos = Vector3.zero;
    bool hasNodes = false;

    // First pass: find bounds and first node position
    for (int x = 0; x < max; x++)
    {
      for (int z = 0; z < max; z++)
      {
        var nodePos = VertexToWorld(compiler.m_hmap, x, z);
        var distance = Utils.DistanceXZ(centerPos, nodePos);
        if (distance <= radius)
        {
          if (!hasNodes)
          {
            firstNodePos = nodePos;
            hasNodes = true;
          }
          minX = Math.Min(minX, x);
          maxX = Math.Max(maxX, x);
          minZ = Math.Min(minZ, z);
          maxZ = Math.Max(maxZ, z);
        }
      }
    }

    if (!hasNodes) return heightmapData;

    // Initialize grid
    int gridWidth = maxX - minX + 1;
    int gridHeight = maxZ - minZ + 1;
    heightmapData.InitializeGrid(gridWidth, gridHeight, firstNodePos);

    // Second pass: populate the grid
    for (int x = 0; x < max; x++)
    {
      for (int z = 0; z < max; z++)
      {
        var nodePos = VertexToWorld(compiler.m_hmap, x, z);
        var distance = Utils.DistanceXZ(centerPos, nodePos);
        if (distance > radius) continue;

        var index = z * max + x;
        nodePos.y = compiler.m_hmap.m_heights[index];

        // Calculate grid position
        int gridX = x - minX;
        int gridZ = z - minZ;

        // Get paint data (no offset needed anymore)
        Color paint = index < compiler.m_paintMask.Length ? compiler.m_paintMask[index] : Color.clear;

        heightmapData.SetNode(gridX, gridZ, nodePos.y, paint);
      }
    }

    return heightmapData;
  }

  public static TerrainData GetTerrainDataWithRect(TerrainComp compiler, Vector3 centerPos, Range<float> width, Range<float> depth, float angle)
  {
    var heightmapData = new TerrainData(compiler.m_hmap.transform.position);

    if (width.Max == 0f || depth.Max == 0f) return heightmapData;

    var max = compiler.m_width + 1;

    // Find bounds to determine grid size
    int minX = max, maxX = -1, minZ = max, maxZ = -1;
    Vector3 firstNodePos = Vector3.zero;
    bool hasNodes = false;

    // First pass: find bounds and first node position
    for (int x = 0; x < max; x++)
    {
      for (int z = 0; z < max; z++)
      {
        var nodePos = VertexToWorld(compiler.m_hmap, x, z);
        var rawDx = nodePos.x - centerPos.x;
        var rawDz = nodePos.z - centerPos.z;
        var dx = GetX(rawDx, rawDz, angle);
        var dz = GetZ(rawDx, rawDz, angle);

        if (Helper.Within(width, depth, Mathf.Abs(dx), Mathf.Abs(dz)))
        {
          if (!hasNodes)
          {
            firstNodePos = nodePos;
            hasNodes = true;
          }
          minX = Math.Min(minX, x);
          maxX = Math.Max(maxX, x);
          minZ = Math.Min(minZ, z);
          maxZ = Math.Max(maxZ, z);
        }
      }
    }

    if (!hasNodes) return heightmapData;

    // Initialize grid
    int gridWidth = maxX - minX + 1;
    int gridHeight = maxZ - minZ + 1;
    heightmapData.InitializeGrid(gridWidth, gridHeight, firstNodePos);

    // Second pass: populate the grid
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
        nodePos.y = compiler.m_hmap.m_heights[index];

        // Calculate grid position
        int gridX = x - minX;
        int gridZ = z - minZ;

        // Get paint data (no offset needed anymore)
        Color paint = index < compiler.m_paintMask.Length ? compiler.m_paintMask[index] : Color.clear;

        heightmapData.SetNode(gridX, gridZ, nodePos.y, paint);
      }
    }

    return heightmapData;
  }

  public static TerrainData CollectTerrainDataInRadius(Vector3 centerPos, Quaternion centerRot, Vector3 searchPos, float radius)
  {
    var compilers = Terrain.GetCompilers(searchPos, new(radius)).ToList();
    var data = MergeHeightmapsWithCircle(compilers, searchPos, radius);
    data.SetReference(centerPos, centerRot);
    return data;
  }

  public static TerrainData CollectTerrainDataInRect(Vector3 centerPos, Quaternion centerRot, Vector3 searchPos, Range<float> width, Range<float> depth, float angle)
  {
    var radius = Math.Max(width.Max, depth.Max);
    var compilers = Terrain.GetCompilers(searchPos, new(radius)).ToList();
    var data = MergeHeightmapsWithRect(compilers, centerPos, width, depth, angle);
    data.SetReference(centerPos, centerRot);
    return data;
  }

  private static TerrainData MergeHeightmapsWithCircle(List<TerrainComp> compilers, Vector3 centerPos, float radius)
  {
    if (compilers.Count == 0)
      return new TerrainData(centerPos);

    // Find global bounds across all heightmaps
    float minWorldX = float.MaxValue, maxWorldX = float.MinValue;
    float minWorldZ = float.MaxValue, maxWorldZ = float.MinValue;
    Vector3 firstNodePos = Vector3.zero;
    bool hasAnyNodes = false;

    // First pass: determine global world bounds
    foreach (var compiler in compilers)
    {
      var max = compiler.m_width + 1;
      for (int x = 0; x < max; x++)
      {
        for (int z = 0; z < max; z++)
        {
          var nodePos = VertexToWorld(compiler.m_hmap, x, z);
          var distance = Utils.DistanceXZ(centerPos, nodePos);
          if (distance <= radius)
          {
            if (!hasAnyNodes)
            {
              firstNodePos = nodePos;
              hasAnyNodes = true;
            }
            minWorldX = Math.Min(minWorldX, nodePos.x);
            maxWorldX = Math.Max(maxWorldX, nodePos.x);
            minWorldZ = Math.Min(minWorldZ, nodePos.z);
            maxWorldZ = Math.Max(maxWorldZ, nodePos.z);
          }
        }
      }
    }

    if (!hasAnyNodes)
      return new TerrainData(centerPos);

    // Calculate grid dimensions (nodes are 1 meter apart)
    int gridWidth = Mathf.RoundToInt(maxWorldX - minWorldX) + 1;
    int gridHeight = Mathf.RoundToInt(maxWorldZ - minWorldZ) + 1;

    // Create merged terrain data
    var mergedData = new TerrainData(centerPos);
    mergedData.InitializeGrid(gridWidth, gridHeight, new Vector3(minWorldX, 0, minWorldZ));

    // Second pass: populate the merged grid
    foreach (var compiler in compilers)
    {
      var max = compiler.m_width + 1;
      for (int x = 0; x < max; x++)
      {
        for (int z = 0; z < max; z++)
        {
          var nodePos = VertexToWorld(compiler.m_hmap, x, z);
          var distance = Utils.DistanceXZ(centerPos, nodePos);
          if (distance > radius) continue;

          var index = z * max + x;
          nodePos.y = compiler.m_hmap.m_heights[index];

          // Calculate position in merged grid
          int gridX = Mathf.RoundToInt(nodePos.x - minWorldX);
          int gridZ = Mathf.RoundToInt(nodePos.z - minWorldZ);

          // Only set if not already populated (avoids edge overlap)
          if (mergedData.GetHeight(gridX, gridZ) == null)
          {
            Color paint = index < compiler.m_paintMask.Length ? compiler.m_paintMask[index] : Color.clear;
            mergedData.SetNode(gridX, gridZ, nodePos.y, paint);
          }
        }
      }
    }

    return mergedData;
  }

  private static TerrainData MergeHeightmapsWithRect(List<TerrainComp> compilers, Vector3 centerPos, Range<float> width, Range<float> depth, float angle)
  {
    if (compilers.Count == 0)
      return new TerrainData(centerPos);

    // Find global bounds across all heightmaps
    float minWorldX = float.MaxValue, maxWorldX = float.MinValue;
    float minWorldZ = float.MaxValue, maxWorldZ = float.MinValue;
    Vector3 firstNodePos = Vector3.zero;
    bool hasAnyNodes = false;

    // First pass: determine global world bounds
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

          if (Helper.Within(width, depth, Mathf.Abs(dx), Mathf.Abs(dz)))
          {
            if (!hasAnyNodes)
            {
              firstNodePos = nodePos;
              hasAnyNodes = true;
            }
            minWorldX = Math.Min(minWorldX, nodePos.x);
            maxWorldX = Math.Max(maxWorldX, nodePos.x);
            minWorldZ = Math.Min(minWorldZ, nodePos.z);
            maxWorldZ = Math.Max(maxWorldZ, nodePos.z);
          }
        }
      }
    }

    if (!hasAnyNodes)
      return new TerrainData(centerPos);

    // Calculate grid dimensions (nodes are 1 meter apart)
    int gridWidth = Mathf.RoundToInt(maxWorldX - minWorldX) + 1;
    int gridHeight = Mathf.RoundToInt(maxWorldZ - minWorldZ) + 1;

    // Create merged terrain data
    var mergedData = new TerrainData(centerPos);
    mergedData.InitializeGrid(gridWidth, gridHeight, new Vector3(minWorldX, 0, minWorldZ));

    // Second pass: populate the merged grid
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
          nodePos.y = compiler.m_hmap.m_heights[index];

          // Calculate position in merged grid
          int gridX = Mathf.RoundToInt(nodePos.x - minWorldX);
          int gridZ = Mathf.RoundToInt(nodePos.z - minWorldZ);

          // Only set if not already populated (avoids edge overlap)
          if (mergedData.GetHeight(gridX, gridZ) == null)
          {
            Color paint = index < compiler.m_paintMask.Length ? compiler.m_paintMask[index] : Color.clear;
            mergedData.SetNode(gridX, gridZ, nodePos.y, paint);
          }
        }
      }
    }

    return mergedData;
  }

  // Helper method to find closest heightmap to a world position - no longer needed with single TerrainData
  [Obsolete("Use direct TerrainData methods instead")]
  public static TerrainData? FindClosestHeightmap(List<TerrainData> heightmaps, Vector3 worldPos)
  {
    if (heightmaps.Count == 0) return null;
    if (heightmaps.Count == 1) return heightmaps[0];

    TerrainData? closest = null;
    float closestDistance = float.MaxValue;

    foreach (var heightmap in heightmaps)
    {
      var distance = Vector3.Distance(worldPos, heightmap.HeightmapPosition);
      if (distance < closestDistance)
      {
        closestDistance = distance;
        closest = heightmap;
      }
    }

    return closest;
  }
}
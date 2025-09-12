using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.Linq;
using HarmonyLib;
using InfinityHammer;
using UnityEngine;

namespace InfinityTools;

public class CategoryInfo
{
  public string Name { get; set; } = "";
  public List<Piece> Items { get; set; } = [];
}

public static class CustomBuildMenu
{
  private const int MaxItemsPerTab = 89; // 15 * 6 - 1 (for repair hammer)
  private static List<CategoryInfo> CategoryInfos { get; set; } = [];

  // Store original categories and labels for each PieceTable instance
  private static Dictionary<PieceTable, (List<Piece.PieceCategory> categories, List<string> labels)> OriginalPieceTableData { get; set; } = [];

  public static bool HandleCustomMenuMode(PieceTable __instance)
  {
    var pt = __instance;
    var tabs = pt.m_availablePieces;
    if (HammerMenuCommand.CurrentMode == MenuMode.Default)
    {
      // Restore original categories and labels if they were saved
      if (OriginalPieceTableData.TryGetValue(pt, out var originalData))
      {
        pt.m_categories = originalData.categories;
        pt.m_categoryLabels = originalData.labels;
        OriginalPieceTableData.Remove(pt);
      }

      if (CategoryInfos.Count >= 0)
      {
        CategoryInfos.Clear();
        // Clearing causes rebuild.
        tabs.Clear();
      }
      return true;
    }

    // Save original categories and labels if not already saved
    if (!OriginalPieceTableData.ContainsKey(pt))
    {
      OriginalPieceTableData[pt] = (new List<Piece.PieceCategory>(pt.m_categories), new List<string>(pt.m_categoryLabels));
    }

    // Clear category info for fresh generation
    CategoryInfos.Clear();

    // Store the repair hammer before clearing
    var repairHammer = GetRepairHammer(__instance);
    tabs.Clear();

    List<CategoryInfo> categories = [];

    switch (HammerMenuCommand.CurrentMode)
    {
      case MenuMode.Objects:
        categories = GenerateObjects();
        break;
      case MenuMode.Components:
        categories = GenerateComponents();
        break;

      case MenuMode.Locations:
        categories = GenerateLocations();
        break;

      case MenuMode.Blueprints:
        categories = GenerateBluePrints();
        break;

      case MenuMode.Sounds:
        categories = GenerateSounds();
        break;

      case MenuMode.Visuals:
        categories = GenerateVisuals();
        break;
    }

    AddCategories(tabs, categories, repairHammer);
    pt.m_categories = [.. categories.Select((_, index) => (Piece.PieceCategory)index)];
    pt.m_categoryLabels = [.. categories.Select(c => c.Name)];
    //pt.m_selectedCategory = 0;
    return false;
  }

  private static Piece BuildObject(string command, string name, string fullName)
  {
    GameObject obj = new();
    var piece = obj.AddComponent<BuildMenuTool>();
    var toolData = new ToolData()
    {
      name = name,
      description = fullName,
      icon = $"_{name}",
      command = $"{command} {fullName}",
      instant = true
    };
    piece.tool = new Tool(toolData);
    piece.m_description = fullName;
    piece.m_name = name;
    piece.m_icon = piece.tool.Icon;
    return piece;
  }

  private static Piece GetRepairHammer(PieceTable pieceTable)
  {
    // Get the original repair hammer from the first tab
    if (pieceTable.m_availablePieces.Count > 0 && pieceTable.m_availablePieces[0].Count > 0)
    {
      return pieceTable.m_availablePieces[0][0];
    }
    return null!;
  }
  private static List<CategoryInfo> GenerateObjects()
  {
    var prefabs = ZNetScene.instance.m_namedPrefabs.Values
      .Where(prefab => !prefab.name.StartsWith("sfx_") && !prefab.name.StartsWith("vfx_"))
      .Where(prefab => string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) ||
                      prefab.name.StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))
      .Select(prefab => BuildObject("hammer", prefab.name, prefab.name)).ToList();
    return GenerateCategories(prefabs);
  }

  private static List<CategoryInfo> GenerateSounds()
  {
    var prefabs = ZNetScene.instance.m_namedPrefabs.Values
      .Where(prefab => prefab.name.StartsWith("sfx_"))
      .Where(prefab => string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) ||
                      prefab.name.Substring(4).StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))
      .Select(prefab => BuildObject("hammer", prefab.name.Substring(4), prefab.name)).ToList();
    return GenerateCategories(prefabs);
  }

  private static List<CategoryInfo> GenerateVisuals()
  {
    var prefabs = ZNetScene.instance.m_namedPrefabs.Values
      .Where(prefab => prefab.name.StartsWith("vfx_"))
      .Where(prefab => string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) ||
                      prefab.name.Substring(4).StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))
      .Select(prefab => BuildObject("hammer", prefab.name.Substring(4), prefab.name)).ToList();
    return GenerateCategories(prefabs);
  }

  private static List<CategoryInfo> GenerateComponents()
  {
    Dictionary<string, List<string>> componentCategories = [];
    foreach (var prefab in ZNetScene.instance.m_namedPrefabs.Values)
    {
      if (!prefab) continue;
      prefab.GetComponentsInChildren(ZNetView.m_tempComponents);
      if (ZNetView.m_tempComponents.Count == 0) continue;
      foreach (var c in ZNetView.m_tempComponents)
      {
        var name = c.GetType().Name;
        if (name == "ZNetView") continue;

        // Apply filter if specified - for components mode, filter component names
        if (!string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) &&
            !name.StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))
          continue;

        if (!componentCategories.ContainsKey(name))
          componentCategories[name] = [];
        componentCategories[name].Add(prefab.name);
      }
    }

    // Process each category and generate category info
    List<CategoryInfo> categories = [];
    foreach (var category in componentCategories)
    {
      var componentType = category.Key;
      var objectNames = category.Value;

      // Sort objects within category
      objectNames.Sort(StringComparer.InvariantCultureIgnoreCase);

      // Create pieces for this component category
      var pieces = objectNames.Select(name => BuildObject("hammer", name, name)).ToList();

      // Split into multiple categories if too many items
      if (pieces.Count <= MaxItemsPerTab)
      {
        categories.Add(new CategoryInfo
        {
          Name = componentType,
          Items = pieces
        });
      }
      else
      {
        // Split large categories into smaller ones with letter prefixes
        var sortedPieces = pieces.OrderBy(p => p.name, StringComparer.InvariantCultureIgnoreCase).ToList();
        int count = sortedPieces.Count;
        while (count > 0)
        {
          var categoryPieces = sortedPieces.Take(MaxItemsPerTab).ToList();
          count -= categoryPieces.Count;
          sortedPieces = sortedPieces.Skip(categoryPieces.Count).ToList();

          var firstLetter = categoryPieces.First().name.Substring(0, 1).ToUpper();
          var lastLetter = categoryPieces.Last().name.Substring(0, 1).ToUpper();
          var categoryName = firstLetter == lastLetter ? $"{firstLetter}-{componentType}" : $"{firstLetter}-{lastLetter}-{componentType}";

          categories.Add(new CategoryInfo
          {
            Name = categoryName,
            Items = categoryPieces
          });
        }
      }
    }

    return categories;
  }
  private static List<CategoryInfo> GenerateLocations()
  {
    var locations = ZoneSystem.instance.m_locations
      .Where(loc => loc.m_prefab.IsValid)
      .Where(loc => string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) ||
                   loc.m_prefab.Name.StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))
      .Select(loc => BuildObject("hammer_location", loc.m_prefab.Name, loc.m_prefab.Name))
      .ToList();
    return GenerateCategories(locations);
  }
  private static List<CategoryInfo> GenerateBluePrints()
  {
    var blueprints = HammerBlueprintCommand.GetBlueprints()
      .Where(bp => string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) ||
                  bp.StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))
      .Select(bp => BuildObject("hammer_blueprint", bp, bp)).ToList();
    return GenerateCategories(blueprints);
  }
  private static List<CategoryInfo> GenerateCategories(List<Piece> items)
  {
    List<CategoryInfo> categories = [];
    if (items.Count == 0) return categories;

    var sorted = items.OrderBy(i => i.m_name, StringComparer.InvariantCultureIgnoreCase).ToList();
    int count = sorted.Count;
    // Use MaxItemsPerTab to determine category splits
    while (count > 0)
    {
      var pieces = sorted.Take(MaxItemsPerTab).ToList();
      count -= pieces.Count;
      sorted = [.. sorted.Skip(pieces.Count)];
      var firstLetter = pieces.First().m_name.Substring(0, 1).ToUpper();
      var lastLetter = pieces.Last().m_name.Substring(0, 1).ToUpper();
      var categoryName = firstLetter == lastLetter ? firstLetter : $"{firstLetter}-{lastLetter}";
      categories.Add(new CategoryInfo
      {
        Name = categoryName,
        Items = pieces,
      });
    }
    return categories;

  }
  private static void AddCategories(List<List<Piece>> tabs, List<CategoryInfo> categories, Piece repairHammer)
  {
    if (categories.Count == 0)
    {
      tabs.Add([repairHammer]);
      return;
    }
    foreach (var category in categories)
    {
      var items = category.Items;
      if (items.Count == 0) continue;
      foreach (var item in items)
        item.m_category = (Piece.PieceCategory)tabs.Count;
      tabs.Add([repairHammer, .. category.Items]);
    }
  }
}
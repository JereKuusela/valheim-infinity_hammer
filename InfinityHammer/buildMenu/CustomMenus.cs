

using System;
using System.Collections.Generic;
using System.Linq;
using InfinityTools;
using ServerDevcommands;
using UnityEngine;

namespace InfinityHammer;

public class BuildItem
{
  public string Command { get; set; } = "";
  public string ShortName { get; set; } = "";
  public string FullName { get; set; } = "";
  public bool Instant { get; set; } = true;
  public string? Icon { get; set; }
}

public static class CustomMenu
{
  public const int CATEGORY_OFFSET = 1000;
  private static int MaxTabs => Configuration.MaxTabs;
  private static int MaxItemsPerTab => Configuration.ItemsPerTab - 1; // Reserve 1 slot for back button
  private static int MaxItems => MaxTabs * MaxItemsPerTab;
  public static BuildItem BuildItem(string command, string shortName, string? fullName = null, bool instant = true, string? icon = null)
  {
    return new BuildItem()
    {
      Command = command,
      ShortName = shortName,
      FullName = fullName ?? shortName,
      Instant = instant,
      Icon = icon
    };
  }
  public static List<CategoryInfo> GenerateObjects()
  {
    var allItems = ZNetScene.instance.m_namedPrefabs.Values
      .Where(prefab => !prefab.name.StartsWith("sfx_") && !prefab.name.StartsWith("vfx_") && !prefab.name.StartsWith("fx_"))
      .Where(prefab => string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) ||
                      prefab.name.StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))
      .Select(prefab => BuildItem($"hammer {prefab.name}", prefab.name))
      .OrderBy(item => item.ShortName, StringComparer.InvariantCultureIgnoreCase)
      .ToList();

    if (allItems.Count == 0) return [];

    // Group items by starting letter
    var groupedByLetter = allItems
      .GroupBy(item => item.ShortName.Substring(0, 1).ToUpper())
      .OrderBy(g => g.Key);

    List<CategoryInfo> categories = [];

    foreach (var letterGroup in groupedByLetter)
    {
      var letter = letterGroup.Key;
      var itemsInGroup = letterGroup.OrderBy(item => item.ShortName, StringComparer.InvariantCultureIgnoreCase).ToList();

      // If the group fits in one category, create a single category for this letter
      if (itemsInGroup.Count <= MaxItemsPerTab)
      {
        categories.Add(new CategoryInfo
        {
          Name = letter,
          Items = itemsInGroup
        });
      }
      else
      {
        // If too many items for one letter, split into subcategories
        int count = itemsInGroup.Count;
        int subIndex = 1;
        while (count > 0)
        {
          var categoryItems = itemsInGroup.Take(MaxItemsPerTab).ToList();
          count -= categoryItems.Count;
          itemsInGroup = [.. itemsInGroup.Skip(categoryItems.Count)];
          var secondLetter = categoryItems.First().ShortName.Substring(1, 1).ToLower();
          categories.Add(new CategoryInfo
          {
            Name = $"{letter}{secondLetter}",
            Items = categoryItems
          });
          subIndex++;
        }
      }
    }

    return categories;
  }

  public static List<CategoryInfo> GenerateMenu()
  {
    List<BuildItem> items = [];

    items.AddRange(GetBuildItems().Select(item => BuildItem($"hammer_menu builds {item.name}", item.name, item.name, true, item.name)));
    items.AddRange(Enum.GetNames(typeof(MenuMode))
      .Where(mode => mode != MenuMode.Menu.ToString() && mode != MenuMode.Builds.ToString())
      .OrderBy(mode => mode)
      .Select(mode => BuildItem($"hammer_menu {mode.ToString().ToLower()}", mode.ToString())));
    List<CategoryInfo> categories = [];
    categories.Add(new CategoryInfo { Name = "Menu", Items = items });
    return categories;
  }

  public static List<CategoryInfo> GenerateBinds()
  {
    return [
      CreateBindCategory(BindManager.GetBinds(), "Binds"),
      CreateBindCategory(BindManager.GetWheelBinds(), "Wheel")
    ];
  }

  private static CategoryInfo CreateBindCategory(Tuple<string, string>[] binds, string categoryName)
  {
    List<BuildItem> bindItems = binds.Select(bind => BuildItem(bind.Item2, bind.Item1, bind.Item2)).ToList();
    return new CategoryInfo { Name = categoryName, Items = bindItems };
  }
  public static List<CategoryInfo> GenerateSounds()
  {
    var items = ZNetScene.instance.m_namedPrefabs.Values
      .Where(prefab => prefab.name.StartsWith("sfx_"))
      .Where(prefab => string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) ||
                      prefab.name.Substring(4).Trim().StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))
      .Select(prefab => BuildItem($"hammer {prefab.name}", prefab.name.Substring(4).Trim(), prefab.name)).ToList();
    return GenerateCategories(items, MaxItemsPerTab);
  }

  public static List<CategoryInfo> GenerateVisuals()
  {
    var items = ZNetScene.instance.m_namedPrefabs.Values
      .Where(prefab => prefab.name.StartsWith("vfx_") || prefab.name.StartsWith("fx_"))
      .Where(prefab => string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) ||
                      prefab.name.Split(['_'], 2).Last().Trim().StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))
      .Select(prefab => BuildItem($"hammer {prefab.name}", prefab.name.Split(['_'], 2).Last().Trim(), prefab.name)).ToList();
    return GenerateCategories(items, MaxItemsPerTab);
  }

  public static List<CategoryInfo> GenerateComponents()
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
    var limit = componentCategories.Count > MaxTabs ? MaxItems : MaxItemsPerTab;
    // Process each category and generate category info
    List<CategoryInfo> categories = [];
    foreach (var category in componentCategories)
    {
      var componentType = category.Key;
      var objectNames = category.Value;

      // Sort objects within category
      objectNames.Sort(StringComparer.InvariantCultureIgnoreCase);

      // Create pieces for this component category
      var items = objectNames.Select(name => BuildItem($"hammer {name}", name)).ToList();

      // Split into multiple categories if too many items
      if (items.Count <= limit)
      {
        categories.Add(new CategoryInfo
        {
          Name = componentType,
          Items = items
        });
      }
      else
      {
        // Split large categories into smaller ones with letter prefixes
        var sortedItems = items.OrderBy(p => p.ShortName, StringComparer.InvariantCultureIgnoreCase).ToList();
        int count = sortedItems.Count;
        while (count > 0)
        {
          var categoryItems = sortedItems.Take(limit).ToList();
          count -= categoryItems.Count;
          sortedItems = sortedItems.Skip(categoryItems.Count).ToList();

          categories.Add(new CategoryInfo
          {
            Name = componentType,
            Items = categoryItems
          });
        }
      }
    }

    return categories;
  }

  public static List<CategoryInfo> GenerateLocations()
  {
    var items = ZoneSystem.instance.m_locations
      .Where(loc => loc.m_prefab.IsValid)
      .Where(loc => string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) ||
                   loc.m_prefab.Name.StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))
      .Select(loc => BuildItem($"hammer_location {loc.m_prefab.Name}", loc.m_prefab.Name)).ToList();
    return GenerateCategories(items, MaxItemsPerTab);
  }
  public static List<CategoryInfo> GenerateRooms()
  {
    var items = DungeonDB.m_instance.m_rooms
      .Where(room => room.m_prefab.IsValid)
      .Where(room => string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) ||
                    room.m_prefab.Name.StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))
      .Select(room => BuildItem($"hammer_room {room.m_prefab.Name}", room.m_prefab.Name)).ToList();
    return GenerateCategories(items, MaxItemsPerTab);
  }
  public static List<CategoryInfo> GenerateBluePrints()
  {
    List<Blueprint> bps = [];
    switch (Configuration.BlueprintSorting)
    {
      case BlueprintSortingMode.Category:
        bps = HammerBlueprintCommand.GetBlueprintsByCategory();
        break;
      case BlueprintSortingMode.Folder:
        bps = HammerBlueprintCommand.GetBlueprintsByFolder();
        break;
      case BlueprintSortingMode.CategoryAndFolder:
        bps = HammerBlueprintCommand.GetBlueprintsByCategoryAndFolder();
        break;
      case BlueprintSortingMode.OnlyName:
        return GenerateBluePrintsByName();
    }
    return GenerateBluePrintCategories(bps);
  }

  private static List<CategoryInfo> GenerateBluePrintCategories(List<Blueprint> bps)
  {
    // Apply filter if specified
    if (!string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter))
    {
      bps = [.. bps.Where(bp => bp.Name.StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))];
    }

    // Group by category
    var categorizedBlueprints = bps
      .GroupBy(bp => bp.Category)
      .OrderBy(g => g.Key, StringComparer.InvariantCultureIgnoreCase);

    List<CategoryInfo> categories = [];

    foreach (var categoryGroup in categorizedBlueprints)
    {
      var categoryName = categoryGroup.Key;
      var blueprintsInCategory = categoryGroup
        .OrderBy(bp => bp.Name, StringComparer.InvariantCultureIgnoreCase)
        .Select(bp => BuildItem($"hammer_blueprint {bp.Name}", bp.Name))
        .ToList();

      // If category has too many items, split them
      if (blueprintsInCategory.Count <= MaxItemsPerTab)
      {
        categories.Add(new CategoryInfo
        {
          Name = categoryName,
          Items = blueprintsInCategory
        });
      }
      else
      {
        // Split large categories into smaller ones with letter prefixes
        var sortedItems = blueprintsInCategory.OrderBy(p => p.ShortName, StringComparer.InvariantCultureIgnoreCase).ToList();
        int count = sortedItems.Count;
        while (count > 0)
        {
          var categoryItems = sortedItems.Take(MaxItemsPerTab).ToList();
          count -= categoryItems.Count;
          sortedItems = sortedItems.Skip(categoryItems.Count).ToList();

          var firstLetter = categoryItems.First().ShortName.Substring(0, 1).ToUpper();
          var lastLetter = categoryItems.Last().ShortName.Substring(0, 1).ToUpper();
          var subCategoryName = firstLetter == lastLetter ? $"{categoryName}-{firstLetter}" : $"{categoryName}-{firstLetter}-{lastLetter}";

          categories.Add(new CategoryInfo
          {
            Name = subCategoryName,
            Items = categoryItems
          });
        }
      }
    }

    return categories;
  }

  private static List<CategoryInfo> GenerateBluePrintsByName()
  {
    var items = HammerBlueprintCommand.GetBlueprints()
      .Where(bp => string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) ||
                  bp.StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))
      .Select(bp => BuildItem($"hammer_blueprint {bp}", bp)).ToList();
    return GenerateCategories(items, MaxItemsPerTab);
  }

  public static List<CategoryInfo> GenerateTools()
  {
    List<CategoryInfo> categories = [];

    // Group tools by equipment type (category)
    foreach (var equipment in ToolManager.Tools.Keys)
    {
      var tools = ToolManager.Get(equipment);
      if (tools.Count == 0) continue;

      // Apply filter if specified
      var filteredTools = tools;
      if (!string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter))
      {
        filteredTools = [.. tools.Where(tool =>
          tool.Name.StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase) ||
          tool.Description.IndexOf(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase) >= 0
        )];
      }

      if (filteredTools.Count == 0) continue;

      // Create build items for this equipment category
      var items = filteredTools
        .OrderBy(tool => tool.Name, StringComparer.InvariantCultureIgnoreCase)
        .Select(tool => BuildItem($"tool {tool.Name}", tool.Name, tool.Description, tool.Instant))
        .ToList();

      // If we have too many tools in one category, split them
      if (items.Count <= MaxItemsPerTab)
      {
        categories.Add(new CategoryInfo
        {
          Name = equipment,
          Items = items
        });
      }
      else
      {
        // Split large categories into smaller ones with letter prefixes
        int count = items.Count;
        while (count > 0)
        {
          var categoryItems = items.Take(MaxItemsPerTab).ToList();
          count -= categoryItems.Count;
          items = items.Skip(categoryItems.Count).ToList();

          var firstLetter = categoryItems.First().ShortName.Substring(0, 1).ToUpper();
          var lastLetter = categoryItems.Last().ShortName.Substring(0, 1).ToUpper();
          var categoryName = firstLetter == lastLetter ? $"{equipment}-{firstLetter}" : $"{equipment}-{firstLetter}-{lastLetter}";

          categories.Add(new CategoryInfo
          {
            Name = categoryName,
            Items = categoryItems
          });
        }
      }
    }

    return categories;
  }

  public static List<CategoryInfo> GenerateBuilds()
  {
    var buildItems = GetBuildItems();

    CategoryInfo category = new CategoryInfo { Name = "Builds", Items = [] };

    foreach (var item in buildItems)
      category.Items.Add(BuildItem($"hammer_menu navigate {item.name}", item.name, item.name, true, item.name));

    return [category];
  }

  public static void AddTools(PieceTable pt)
  {
    var equipment = PieceTableToEquipment(pt);
    List<Tool> tools = ToolManager.Get(equipment);
    int tab = 0;
    Dictionary<int, int> indices = [];
    foreach (var tool in tools)
    {
      tab = tool.TabIndex ?? tab;
      if (pt.m_availablePieces.Count <= tab) return;
      if (!indices.ContainsKey(tab))
        indices[tab] = equipment == "hammer" ? 0 : pt.m_availablePieces[tab].Count - 1;
      var index = tool.Index ?? indices[tab] + 1;
      var pieces = pt.m_availablePieces[tab];
      index = Math.Min(index, pieces.Count);
      pieces.Insert(index, Build(tool));
      indices[tab] = index;
    }
  }
  private static string PieceTableToEquipment(PieceTable pt)
  {
    var name = pt.name;
    if (name.StartsWith("_")) name = name.Substring(1);
    if (name.EndsWith("PieceTable")) name = name.Substring(0, name.Length - "PieceTable".Length);
    return name.ToLowerInvariant();
  }
  private static Piece Build(Tool tool)
  {
    GameObject obj = new();
    var piece = obj.AddComponent<BuildMenuTool>();
    piece.tool = tool;
    piece.m_description = tool.Description;
    piece.m_name = tool.Name;
    piece.m_icon = tool.Icon;
    return piece;
  }

  public static List<ItemDrop> GetBuildItems()
  {
    var buildItems = ObjectDB.instance.m_items
     .Where(item => item.TryGetComponent<ItemDrop>(out var itemDrop) && itemDrop.m_itemData.m_shared.m_buildPieces)
     .Select(item => item.GetComponent<ItemDrop>())
     .OrderBy(item => item.name, StringComparer.InvariantCultureIgnoreCase)
     .ToList();
    return buildItems;
  }

  public static List<CategoryInfo> GenerateCategories(List<BuildItem> items, int limit)
  {
    List<CategoryInfo> categories = [];
    if (items.Count == 0) return categories;

    var sorted = items.OrderBy(i => i.ShortName, StringComparer.InvariantCultureIgnoreCase).ToList();
    int count = sorted.Count;
    // Use MaxItemsPerTab to determine category splits
    while (count > 0)
    {
      var itemsForCategory = sorted.Take(limit).ToList();
      count -= itemsForCategory.Count;
      sorted = [.. sorted.Skip(itemsForCategory.Count)];
      var firstLetter = itemsForCategory.First().ShortName.Substring(0, 1).ToUpper();
      var lastLetter = itemsForCategory.Last().ShortName.Substring(0, 1).ToUpper();
      var categoryName = firstLetter == lastLetter ? firstLetter : $"{firstLetter}-{lastLetter}";
      categories.Add(new CategoryInfo
      {
        Name = categoryName,
        Items = itemsForCategory,
      });
    }
    if (categories.Count > MaxTabs && limit < MaxItems) return GenerateCategories(items, MaxItems);
    return categories;

  }

  private static readonly Dictionary<string, Piece> pieceCache = [];
  public static Piece BuildObject(BuildItem item, Piece.PieceCategory category)
  {
    var key = $"{item.Command}|{item.ShortName}";
    if (pieceCache.TryGetValue(key, out var cachedPiece))
    {
      cachedPiece.m_category = category;
      return cachedPiece;
    }

    GameObject obj = new();
    UnityEngine.Object.DontDestroyOnLoad(obj);
    var piece = obj.AddComponent<BuildMenuTool>();
    var toolData = new ToolData()
    {
      name = item.ShortName,
      description = item.FullName,
      icon = item.Icon ?? $"_{item.ShortName}",
      command = item.Command,
      instant = item.Instant
    };
    piece.tool = new Tool(toolData);
    piece.m_description = item.FullName;
    piece.m_name = item.ShortName;
    piece.m_icon = piece.tool.Icon;
    pieceCache[key] = piece;
    piece.m_category = category;
    return piece;
  }

  public static Piece RepairButton()
  {
    if (pieceCache.TryGetValue("piece_repair", out var cachedPiece))
    {
      // Log out might invalidate the cached piece.
      if (!cachedPiece) pieceCache.Remove("piece_repair");
      else return cachedPiece;
    }
    var buildItem = GetBuildItems().FirstOrDefault(i => i.name.Equals("hammer", StringComparison.InvariantCultureIgnoreCase));
    if (!buildItem) return BackButton();
    var pt = buildItem.m_itemData.m_shared.m_buildPieces;
    if (!pt) return BackButton();
    var repair = pt.m_pieces.FirstOrDefault(p => p.name.Equals("piece_repair", StringComparison.InvariantCultureIgnoreCase));
    if (!repair) return BackButton();
    var piece = repair.GetComponent<Piece>();
    if (!piece) return BackButton();
    pieceCache["piece_repair"] = piece;
    return piece;
  }
  public static Piece BackButton() => BuildObject(BuildItem("hammer_menu back", "←", "Back"), Piece.PieceCategory.All);

}
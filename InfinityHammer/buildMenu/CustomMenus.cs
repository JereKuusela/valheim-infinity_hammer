

using System;
using System.Collections.Generic;
using System.Linq;
using Data;
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
      .Where(prefab => !prefab.name.StartsWith("sfx_") && !prefab.name.StartsWith("vfx_") && !prefab.name.StartsWith("fx_") && !prefab.name.StartsWith("vx_"))
      .Where(prefab => string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) ||
                      prefab.name.StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))
      .Select(prefab => BuildItem($"hammer {prefab.name}", prefab.name))
      .ToList();
    return GenerateCategories(allItems);
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
    return GenerateCategories(items);
  }

  public static List<CategoryInfo> GenerateVisuals()
  {
    var items = ZNetScene.instance.m_namedPrefabs.Values
      .Where(prefab => prefab.name.StartsWith("vfx_") || prefab.name.StartsWith("fx_") || prefab.name.StartsWith("vx_"))
      .Where(prefab => string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) ||
                      prefab.name.Split(['_'], 2).Last().Trim().StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))
      .Select(prefab => BuildItem($"hammer {prefab.name}", prefab.name.Split(['_'], 2).Last().Trim(), prefab.name)).ToList();
    return GenerateCategories(items);
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
    List<CategoryInfo> categories = [];
    foreach (var category in componentCategories.OrderBy(kvp => kvp.Key, StringComparer.InvariantCultureIgnoreCase))
    {
      category.Value.Sort(StringComparer.InvariantCultureIgnoreCase);
      categories.Add(new CategoryInfo
      {
        Name = category.Key,
        Items = [.. category.Value.Select(name => BuildItem($"hammer {name}", name))]
      });
    }
    return categories;
  }

  public static List<CategoryInfo> GenerateVegetations()
  {
    var items = ParameterInfo.VegetationIds
      .Where(veg => string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) ||
                   veg.StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))
      .Select(veg => BuildItem($"hammer {veg}", veg)).ToList();
    return GenerateCategories(items);
  }
  public static List<CategoryInfo> GenerateLocations()
  {
    var items = ParameterInfo.LocationIds
      .Where(loc => string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) ||
                   loc.StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))
      .Select(loc => BuildItem($"hammer_location {loc}", loc)).ToList();
    return GenerateCategories(items);
  }
  public static List<CategoryInfo> GenerateRooms()
  {
    var items = DungeonDB.m_instance.m_rooms
      .Where(room => room.m_prefab.IsValid)
      .Where(room => string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) ||
                    room.m_prefab.name.StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))
      .Select(room => BuildItem($"hammer_room {room.m_prefab.name}", room.m_prefab.name)).ToList();
    return GenerateCategories(items);
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
      categories.Add(new CategoryInfo
      {
        Name = categoryGroup.Key,
        Items = [.. categoryGroup
          .OrderBy(bp => bp.Name, StringComparer.InvariantCultureIgnoreCase)
          .Select(bp => BuildItem($"hammer_blueprint {bp.Name}", bp.Name))]
      });
    }

    return categories;
  }

  private static List<CategoryInfo> GenerateBluePrintsByName()
  {
    var items = HammerBlueprintCommand.GetBlueprints()
      .Where(bp => string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) ||
                  bp.StartsWith(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase))
      .Select(bp => BuildItem($"hammer_blueprint {bp}", bp)).ToList();
    return GenerateCategories(items);
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
      categories.Add(new CategoryInfo
      {
        Name = equipment,
        Items = [.. filteredTools
          .OrderBy(tool => tool.Name, StringComparer.InvariantCultureIgnoreCase)
          .Select(tool => BuildItem($"tool {tool.Name}", tool.Name, tool.Description, tool.Instant))]
      });
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
    // Custom pages may leave the table without any tab.
    if (pt.m_availablePiecesByCategory.Count == 0 && Hammer.IsInfinityHammer(pt)) pt.m_availablePiecesByCategory.Add([]);
    List<Tool> tools = ToolManager.Get(equipment);
    // Infinity Hammer pages have their own tabs, so tools go to the first one.
    var customPages = Hammer.IsInfinityHammer(pt);
    var defaultTab = pt.m_categories.Count > 0 ? (int)pt.m_categories[0] : 0;
    int tab = 0;
    Dictionary<int, int> indices = [];
    foreach (var tool in tools)
    {
      if (!IsToolCommandAllowed(tool)) continue;
      int target;
      if (customPages)
        target = defaultTab;
      else
      {
        tab = tool.TabIndex ?? tab;
        target = tab;
      }
      if (target < 0 || pt.m_availablePiecesByCategory.Count <= target) continue;
      var piece = Build(pt, tool);
      if (!indices.ContainsKey(target))
        indices[target] = equipment == "hammer" ? 0 : pt.m_availablePiecesByCategory[target].Count - 1;
      var pieces = pt.m_availablePiecesByCategory[target];
      var index = tool.GetSlot(pieces, indices[target]);
      // The piece can be in another legacy tab, so visibility is decided by the build menu view.
      if (index < 0) index = pieces.Count;
      piece.m_category = (Piece.PieceCategory)target;
      pieces.Insert(index, piece);
      indices[target] = index;
      pt.m_availablePieces.Add(piece);
      pt.m_enabledPieces.Add(piece);
    }
  }

  private static bool IsToolCommandAllowed(Tool tool)
  {
    var command = tool.GetCommand();
    if (string.IsNullOrWhiteSpace(command)) return true;
    var subCommands = command.Split(';');
    foreach (var subCommand in subCommands)
    {
      var trimmed = subCommand.Trim();
      if (trimmed == "") continue;
      var commandName = trimmed.Split(' ')[0];
      if (!PermissionApi.IsCommandAllowed(commandName))
        return false;
    }
    return true;
  }

  internal static string PieceTableToEquipment(PieceTable pt)
  {
    var name = pt.name;
    if (name.StartsWith("_")) name = name.Substring(1);
    if (name.EndsWith("PieceTable")) name = name.Substring(0, name.Length - "PieceTable".Length);
    return name.ToLowerInvariant();
  }
  private static Piece Build(PieceTable pt, Tool tool)
  {
    return ToolMenuPieces.Get(pt, tool);
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

  public static List<CategoryInfo> GenerateCategories(List<BuildItem> items)
  {
    return [.. items
      .GroupBy(i => i.ShortName.Length > 0 ? i.ShortName.Substring(0, 1).ToUpperInvariant() : "#")
      .OrderBy(g => g.Key, StringComparer.InvariantCultureIgnoreCase)
      .Select(g => new CategoryInfo
      {
        Name = g.Key,
        Items = [.. g.OrderBy(i => i.ShortName, StringComparer.InvariantCultureIgnoreCase)]
      })];
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

    GameObject obj = new("_IH_Menu_" + key);
    // Inactive so that Piece.Awake doesn't register it as a world piece.
    obj.SetActive(false);
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

  public static void ReleaseUnused(HashSet<Piece> used)
  {
    var unused = pieceCache.Where(kvp => kvp.Value is BuildMenuTool && !used.Contains(kvp.Value)).Select(kvp => kvp.Key).ToList();
    foreach (var key in unused)
    {
      if (pieceCache[key]) UnityEngine.Object.Destroy(pieceCache[key].gameObject);
      pieceCache.Remove(key);
    }
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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using InfinityTools;
using ServerDevcommands;

namespace InfinityHammer;

public class BuildItem
{
  public string Command { get; set; } = "";
  public string ShortName { get; set; } = "";
  public string FullName { get; set; } = "";
  public bool Instant { get; set; } = true;
}

public static class CustomMenu
{
  private static int MaxTabs => Configuration.MaxTabs;
  private static int MaxItemsPerTab => Configuration.ItemsPerTab - 1; // Reserve 1 slot for back button
  private static int MaxItems => MaxTabs * MaxItemsPerTab;
  public static BuildItem BuildItem(string command, string shortName, string? fullName = null, bool instant = true)
  {
    return new BuildItem()
    {
      Command = command,
      ShortName = shortName,
      FullName = fullName ?? shortName,
      Instant = instant
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
    List<BuildItem> items = [.. Enum.GetNames(typeof(MenuMode))
      .Where(mode => mode != MenuMode.Default.ToString() && mode != MenuMode.Menu.ToString())
      .OrderBy(mode => mode)
      .Select(mode => BuildItem($"hammer_menu {mode.ToString().ToLower()}", mode.ToString()))];
    List<CategoryInfo> categories = [];
    categories.Add(new CategoryInfo { Name = "Menu", Items = items });
    return categories;
  }

  public static List<CategoryInfo> GenerateBinds()
  {
    try
    {
      // Use reflection to access private members of BindManager
      var bindManagerType = typeof(BindManager);
      var printBindMethod = bindManagerType.GetMethod("PrintBind", BindingFlags.NonPublic | BindingFlags.Static);
      var bindsField = bindManagerType.GetField("Binds", BindingFlags.NonPublic | BindingFlags.Static);
      var wheelBindsField = bindManagerType.GetField("WheelBinds", BindingFlags.NonPublic | BindingFlags.Static);

      // Early exit if reflection failed
      if (printBindMethod == null || bindsField == null || wheelBindsField == null)
      {
        return [new CategoryInfo
        {
          Name = "Error",
          Items = [BuildItem("", "Reflection failed", "Could not access BindManager private members")]
        }];
      }

      List<CategoryInfo> categories = [];

      // Process regular binds
      var bindsCategory = ProcessBindCategory(bindsField, printBindMethod, "Binds");
      if (bindsCategory != null) categories.Add(bindsCategory);

      // Process wheel binds
      var wheelCategory = ProcessBindCategory(wheelBindsField, printBindMethod, "Wheel");
      if (wheelCategory != null) categories.Add(wheelCategory);

      return categories;
    }
    catch (Exception ex)
    {
      return [new CategoryInfo
      {
        Name = "Error",
        Items = [BuildItem("", "Exception", $"Error accessing binds: {ex.Message}")]
      }];
    }
  }

  private static CategoryInfo? ProcessBindCategory(FieldInfo bindsField, MethodInfo printBindMethod, string categoryName)
  {
    if (bindsField.GetValue(null) is not System.Collections.IList binds || binds.Count == 0)
      return null;

    List<BuildItem> bindItems = [];

    foreach (var bind in binds)
    {
      if (bind == null) continue;

      var bindItem = CreateBindItem(bind, printBindMethod);
      if (bindItem != null) bindItems.Add(bindItem);
    }

    return bindItems.Count > 0 ? new CategoryInfo { Name = categoryName, Items = bindItems } : null;
  }

  private static BuildItem? CreateBindItem(object bind, MethodInfo printBindMethod)
  {
    // Use reflection to call PrintBind method
    string bindString = printBindMethod.Invoke(null, [bind]) as string ?? "Unknown Bind";

    // Apply filter early exit
    if (!string.IsNullOrEmpty(HammerMenuCommand.CurrentFilter) &&
        bindString.IndexOf(HammerMenuCommand.CurrentFilter, StringComparison.InvariantCultureIgnoreCase) < 0)
      return null;

    var parts = bindString.Split([':'], 2);
    if (parts.Length < 2) return null;

    string keys = parts[0].Trim();
    string command = parts[1].Trim();

    // Keys as display name, command gets executed when clicked
    return BuildItem(command, keys, bindString);
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


}
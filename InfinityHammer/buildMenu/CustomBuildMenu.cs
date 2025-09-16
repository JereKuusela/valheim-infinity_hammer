using System;
using System.Collections.Generic;
using System.Linq;
using InfinityTools;
using UnityEngine;

namespace InfinityHammer;

public class CategoryInfo
{
  public string Name { get; set; } = "";
  public List<BuildItem> Items { get; set; } = [];
}

public static class CustomBuildMenu
{
  private static int MaxTabs => Configuration.MaxTabs;
  private static int MaxItemsPerTab => Configuration.ItemsPerTab - 1; // Reserve 1 slot for back button
  private static List<CategoryInfo> CategoryInfos { get; set; } = [];


  public static void HandleCustomMenuMode(PieceTable pt)
  {
    var tabs = pt.m_availablePieces;
    CategoryInfos.Clear();
    tabs.Clear();

    List<CategoryInfo> categories = [];

    switch (HammerMenuCommand.CurrentMode)
    {
      case MenuMode.Menu:
        categories = CustomMenu.GenerateMenu();
        break;
      case MenuMode.Binds:
        categories = CustomMenu.GenerateBinds();
        break;
      case MenuMode.Objects:
        categories = CustomMenu.GenerateObjects();
        break;
      case MenuMode.Types:
        categories = CustomMenu.GenerateComponents();
        break;
      case MenuMode.Locations:
        categories = CustomMenu.GenerateLocations();
        break;
      case MenuMode.Blueprints:
        categories = CustomMenu.GenerateBluePrints();
        break;
      case MenuMode.Rooms:
        categories = CustomMenu.GenerateRooms();
        break;
      case MenuMode.Sounds:
        categories = CustomMenu.GenerateSounds();
        break;
      case MenuMode.Visuals:
        categories = CustomMenu.GenerateVisuals();
        break;
      case MenuMode.Tools:
        categories = CustomMenu.GenerateTools();
        break;
      case MenuMode.Builds:
        categories = CustomMenu.GenerateBuilds();
        break;
    }

    // Process categories with pagination if needed
    categories = HandleNavigation(categories);

    AddCategories(tabs, categories);
    pt.m_categories = [.. categories.Select((_, index) => (Piece.PieceCategory)index)];
    pt.m_categoryLabels = [.. categories.Select(c => c.Name)];
    if (pt.m_selectedPiece.Length < pt.m_categories.Count)
      pt.m_selectedPiece = new Vector2Int[pt.m_categories.Count];
    if (pt.m_lastSelectedPiece.Length < pt.m_categories.Count)
      pt.m_lastSelectedPiece = new Vector2Int[pt.m_categories.Count];
    if ((int)pt.m_selectedCategory >= pt.m_categories.Count)
      pt.m_selectedCategory = 0;
  }



  private static Piece BuildObject(BuildItem item)
  {
    GameObject obj = new();
    var piece = obj.AddComponent<BuildMenuTool>();
    var toolData = new ToolData()
    {
      name = item.ShortName,
      description = item.FullName,
      icon = $"_{item.ShortName}",
      command = item.Command,
      instant = item.Instant
    };
    piece.tool = new Tool(toolData);
    piece.m_description = item.FullName;
    piece.m_name = item.ShortName;
    piece.m_icon = piece.tool.Icon;
    return piece;
  }
  private static List<CategoryInfo> HandleNavigation(List<CategoryInfo> categories)
  {
    if (HammerMenuCommand.CurrentPage >= 0 && HammerMenuCommand.CurrentPage < categories.Count)
      return CustomMenu.GenerateCategories(categories[HammerMenuCommand.CurrentPage].Items, MaxItemsPerTab);
    // If went over max tabs, bigger limit was used.
    if (categories.Count <= MaxTabs && categories.All(c => c.Items.Count <= MaxItemsPerTab))
      return categories;
    var items = categories.Select((c, i) => CustomMenu.BuildItem($"hammer_menu navigate {i}", c.Name, FormatItemsList(c.Items))).ToList();
    var result = CustomMenu.GenerateCategories(items, MaxItemsPerTab);
    if (result.Count == 1) result[0].Name = HammerMenuCommand.CurrentMode.ToString();
    return result;
  }

  private static string FormatItemsList(List<BuildItem> items)
  {
    var lines = new List<string>();
    var currentLine = "";

    foreach (var item in items)
    {
      var itemName = item.FullName;

      // If adding this item would exceed 50 characters, start a new line
      if (!string.IsNullOrEmpty(currentLine) && (currentLine + ", " + itemName).Length > 50)
      {
        lines.Add(currentLine);
        currentLine = itemName;

        // Stop if we've reached 5 lines
        if (lines.Count >= 5)
          break;
      }
      else
      {
        // Add to current line
        if (string.IsNullOrEmpty(currentLine))
          currentLine = itemName;
        else
          currentLine += ", " + itemName;
      }
    }

    if (!string.IsNullOrEmpty(currentLine) && lines.Count < 5)
      lines.Add(currentLine);

    return string.Join("\n", lines);
  }

  private static void AddCategories(List<List<Piece>> tabs, List<CategoryInfo> categories)
  {
    var backCommand = HammerMenuCommand.CurrentMode == MenuMode.Menu ? "hammer_menu default" : "hammer_menu back";
    var back = BuildObject(CustomMenu.BuildItem(backCommand, "←", "Back"));
    back.m_category = Piece.PieceCategory.All;
    if (categories.Count == 0)
    {
      tabs.Add([back]);
      return;
    }
    foreach (var category in categories)
    {
      var items = category.Items;
      if (items.Count == 0) continue;

      var pieces = items.Select(BuildObject).ToList();

      foreach (var piece in pieces)
        piece.m_category = (Piece.PieceCategory)tabs.Count;
      tabs.Add([back, .. pieces]);
    }
  }
}
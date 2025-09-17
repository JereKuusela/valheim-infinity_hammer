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
    CategoryInfos.Clear();

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
        // Handled by replacing the entire piece table.
        if (HammerMenuCommand.CurrentFilter != "")
          return;
        categories = CustomMenu.GenerateBuilds();
        break;
    }

    // Process categories with pagination if needed
    categories = HandleNavigation(categories);

    AddCategories(pt.m_availablePieces, categories);
    pt.m_categories = [.. categories.Select((_, index) => (Piece.PieceCategory)(CustomMenu.CATEGORY_OFFSET + index))];
    pt.m_categoryLabels = [.. categories.Select(c => c.Name)];
    SanityCheck(pt);
  }


  private static void SanityCheck(PieceTable pt)
  {
    int amount = 1 + (pt.m_categories.Count > 0 ? pt.m_categories.Max(c => (int)c) : 0);
    if (pt.m_selectedPiece.Length < amount)
      Array.Resize(ref pt.m_selectedPiece, amount);
    if (pt.m_lastSelectedPiece.Length < amount)
      Array.Resize(ref pt.m_lastSelectedPiece, amount);
    if (!pt.m_categories.Contains(pt.m_selectedCategory))
      pt.m_selectedCategory = pt.m_categories.Count > 0 ? pt.m_categories[0] : 0;
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
    var back = HammerMenuCommand.CurrentMode == MenuMode.Menu ? CustomMenu.RepairButton() : CustomMenu.BackButton();
    while (tabs.Count < (CustomMenu.CATEGORY_OFFSET + categories.Count))
      tabs.Add([]);
    var index = CustomMenu.CATEGORY_OFFSET;
    foreach (var category in categories)
    {
      var tab = tabs[index];
      tab.Clear();
      tab.Add(back);
      var pieces = category.Items.Select(item => CustomMenu.BuildObject(item, (Piece.PieceCategory)index)).ToList();
      tab.AddRange(pieces);
      index += 1;
    }
  }
}
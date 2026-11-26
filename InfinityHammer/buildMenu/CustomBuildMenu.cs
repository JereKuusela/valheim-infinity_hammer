using System;
using System.Collections.Generic;
using System.Linq;

namespace InfinityHammer;

public class CategoryInfo
{
  public string Name { get; set; } = "";
  public List<BuildItem> Items { get; set; } = [];
}

public static class CustomBuildMenu
{
  public static void HandleCustomMenuMode(PieceTable pt)
  {
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
      case MenuMode.Vegetation:
        categories = CustomMenu.GenerateVegetations();
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

    foreach (var tab in pt.m_availablePiecesByCategory) tab.Clear();
    pt.m_availablePieces.Clear();
    pt.m_enabledPieces.Clear();
    AddCategories(pt.m_availablePiecesByCategory, categories);
    foreach (var category in pt.m_availablePiecesByCategory)
      foreach (var piece in category)
      {
        pt.m_availablePieces.Add(piece);
        pt.m_enabledPieces.Add(piece);
      }
    pt.m_categories = [.. categories.Select((_, index) => (Piece.PieceCategory)(CustomMenu.CATEGORY_OFFSET + index))];
    pt.m_categoryLabels = [.. categories.Select(c => c.Name)];
    SanityCheck(pt);
    CustomMenu.ReleaseUnused(pt.m_availablePieces);
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
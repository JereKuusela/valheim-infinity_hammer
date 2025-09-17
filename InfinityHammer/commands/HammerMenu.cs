using System.Collections.Generic;
using HarmonyLib;
using ServerDevcommands;

namespace InfinityHammer;

public enum MenuMode
{
  Menu,
  Binds,
  Blueprints,
  Builds,
  Locations,
  Objects,
  Rooms,
  Sounds,
  Tools,
  Types,
  Visuals,
}

public class MenuNavigation(MenuMode mode, string filter, int page)
{
  public MenuMode Mode = mode;
  public string Filter = filter;
  public int Page = page;
}

public class HammerMenuCommand
{
  public static MenuMode CurrentMode => NavigationStack.Count > 0 ? NavigationStack[NavigationStack.Count - 1].Mode : MenuMode.Menu;
  public static string CurrentFilter => NavigationStack.Count > 0 ? NavigationStack[NavigationStack.Count - 1].Filter : "";
  public static int CurrentPage => NavigationStack.Count > 0 ? NavigationStack[NavigationStack.Count - 1].Page : -1;

  private static readonly List<MenuNavigation> NavigationStack = [];

  public HammerMenuCommand()
  {
    AutoComplete.Register("hammer_menu", (int index, int subIndex) =>
    {
      if (index == 0) return ["binds", "blueprints", "builds", "objects", "locations", "rooms", "sounds", "visuals", "tools", "types"];
      if (index == 1) return ParameterInfo.Create("Filter text (optional)");
      return ParameterInfo.None;
    });

    Helper.Command("hammer_menu", "[mode] [filter] - Sets the hammer menu mode with optional filter.", Execute);
  }
  private static void Execute(Terminal.ConsoleEventArgs args)
  {
    if (Hud.IsPieceSelectionVisible())
      IgnoreNextHide.IgnoreHide = true;


    var mode = args.Length > 1 ? args[1].ToLowerInvariant() : "menu";
    if (mode == "back")
    {
      if (NavigationStack.Count > 0)
        NavigationStack.RemoveAt(NavigationStack.Count - 1);
      Hammer.OpenBuildMenu();
      return;
    }
    if (mode == "navigate")
    {
      if (args.Length < 3)
      {
        HammerHelper.Message(args.Context, "Missing the navigation step.");
        return;
      }
      var page = Parse.IntNull(args[2]);
      if (page.HasValue)
        NavigationStack.Add(new MenuNavigation(CurrentMode, CurrentFilter, page.Value));
      else
        NavigationStack.Add(new MenuNavigation(CurrentMode, args[2], CurrentPage));
      Hammer.OpenBuildMenu();
      return;
    }
    var filter = args.Length > 2 ? args[2] : "";

    NavigationStack.Clear();
    switch (mode)
    {
      case "menu":
        NavigationStack.Add(new MenuNavigation(MenuMode.Menu, filter, -1));
        break;
      case "binds":
        NavigationStack.Add(new MenuNavigation(MenuMode.Binds, filter, -1));
        break;
      case "types":
        NavigationStack.Add(new MenuNavigation(MenuMode.Types, filter, -1));
        break;
      case "objects":
        NavigationStack.Add(new MenuNavigation(MenuMode.Objects, filter, -1));
        break;
      case "locations":
        NavigationStack.Add(new MenuNavigation(MenuMode.Locations, filter, -1));
        break;
      case "blueprints":
        NavigationStack.Add(new MenuNavigation(MenuMode.Blueprints, filter, -1));
        break;
      case "rooms":
        NavigationStack.Add(new MenuNavigation(MenuMode.Rooms, filter, -1));
        break;
      case "sounds":
        NavigationStack.Add(new MenuNavigation(MenuMode.Sounds, filter, -1));
        break;
      case "visuals":
        NavigationStack.Add(new MenuNavigation(MenuMode.Visuals, filter, -1));
        break;
      case "tools":
        NavigationStack.Add(new MenuNavigation(MenuMode.Tools, filter, -1));
        break;
      case "builds":
        NavigationStack.Add(new MenuNavigation(MenuMode.Builds, filter, -1));
        break;
      default:
        HammerHelper.Message(args.Context, "Invalid mode.");
        return;
    }
    Hammer.OpenBuildMenu();
  }
}

[HarmonyPatch(typeof(Hud), nameof(Hud.HidePieceSelection))]
public class IgnoreNextHide
{
  public static bool IgnoreHide = false;
  static bool Prefix()
  {
    if (IgnoreHide)
    {
      IgnoreHide = false;
      return false;
    }
    return true;
  }
}
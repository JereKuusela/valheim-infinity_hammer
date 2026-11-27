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
  Vegetation,
}

public class MenuNavigation(MenuMode mode, string filter)
{
  public MenuMode Mode = mode;
  public string Filter = filter;
}

public class HammerMenuCommand
{
  public static MenuMode CurrentMode => NavigationStack.Count > 0 ? NavigationStack[NavigationStack.Count - 1].Mode : MenuMode.Menu;
  public static string CurrentFilter => NavigationStack.Count > 0 ? NavigationStack[NavigationStack.Count - 1].Filter : "";

  private static readonly List<MenuNavigation> NavigationStack = [];

  public HammerMenuCommand()
  {
    AutoComplete.Register("hammer_menu", (int index, int subIndex) =>
    {
      if (index == 0) return ["close", "binds", "blueprints", "builds", "objects", "locations", "rooms", "sounds", "visuals", "tools", "types"];
      if (index == 1) return ParameterInfo.Create("Filter text (optional)");
      return ParameterInfo.None;
    });

    Helper.Command("hammer_menu", "[mode] [filter] - Opens the hammer menu; repeat the same menu to close, or use close.", Execute);
  }
  private static void Execute(Terminal.ConsoleEventArgs args)
  {


    var mode = args.Length > 1 ? args[1].ToLowerInvariant() : "menu";
    var requestedFilter = args.Length > 2 ? args[2] : "";
    var samePage = mode == CurrentMode.ToString().ToLowerInvariant() && requestedFilter == CurrentFilter;
    if (mode == "close" || (samePage && Hud.IsPieceSelectionVisible() && Hammer.IsInfinityHammer(Helper.GetPlayer().GetRightItem())))
    {
      Hud.CloseBuildUi();
      return;
    }
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
      NavigationStack.Add(new MenuNavigation(CurrentMode, args[2]));
      Hammer.OpenBuildMenu();
      return;
    }
    var filter = args.Length > 2 ? args[2] : "";
    if (int.TryParse(mode, out _) || !System.Enum.TryParse<MenuMode>(mode, true, out var menuMode))
    {
      HammerHelper.Message(args.Context, "Invalid mode.");
      return;
    }
    NavigationStack.Clear();
    NavigationStack.Add(new MenuNavigation(menuMode, filter));
    Hammer.OpenBuildMenu();
  }
}

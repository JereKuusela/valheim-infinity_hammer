using System;
using System.Collections.Generic;
using HarmonyLib;
using ServerDevcommands;
using Service;

namespace InfinityHammer;

public enum MenuMode
{
  Default,
  Menu,
  Types,
  Objects,
  Locations,
  Blueprints,
  Sounds,
  Visuals,
  Tools,
  Binds
}

public class HammerMenuCommand
{
  public static MenuMode CurrentMode { get; private set; } = MenuMode.Default;
  public static string CurrentFilter { get; private set; } = "";
  public static int CurrentPage { get; private set; } = -1;

  public HammerMenuCommand()
  {
    AutoComplete.Register("hammer_menu", (int index, int subIndex) =>
    {
      if (index == 0) return ["binds", "blueprints", "objects", "locations", "sounds", "visuals", "tools", "types", "default"];
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
      if (CurrentPage >= 0)
        CurrentPage = -1;
      else CurrentMode = MenuMode.Menu;
      Hammer.OpenBuildMenu();
      return;
    }
    if (mode == "navigate")
    {
      CurrentPage = args.Length > 2 ? Parse.Int(args[2], -1) : -1;
      Hammer.OpenBuildMenu();
      return;
    }
    var filter = args.Length > 2 ? args[2] : "";

    CurrentPage = -1;
    switch (mode)
    {
      case "menu":
        CurrentMode = MenuMode.Menu;
        CurrentFilter = filter;
        break;
      case "binds":
        CurrentMode = MenuMode.Binds;
        CurrentFilter = filter;
        break;
      case "types":
        CurrentMode = MenuMode.Types;
        CurrentFilter = filter;
        break;
      case "objects":
        CurrentMode = MenuMode.Objects;
        CurrentFilter = filter;
        break;
      case "locations":
        CurrentMode = MenuMode.Locations;
        CurrentFilter = filter;
        break;
      case "blueprints":
        CurrentMode = MenuMode.Blueprints;
        CurrentFilter = filter;
        break;
      case "sounds":
        CurrentMode = MenuMode.Sounds;
        CurrentFilter = filter;
        break;
      case "visuals":
        CurrentMode = MenuMode.Visuals;
        CurrentFilter = filter;
        break;
      case "tools":
        CurrentMode = MenuMode.Tools;
        CurrentFilter = filter;
        break;
      case "default":
        CurrentMode = MenuMode.Default;
        CurrentFilter = "";
        CustomBuildMenu.Clear();
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
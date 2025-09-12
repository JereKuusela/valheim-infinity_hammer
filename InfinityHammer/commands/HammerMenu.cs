using System.Collections.Generic;
using ServerDevcommands;

namespace InfinityHammer;

public enum MenuMode
{
  Default,
  Components,
  Objects,
  Locations,
  Blueprints,
  Sounds,
  Visuals
}

public class HammerMenuCommand
{
  public static MenuMode CurrentMode { get; private set; } = MenuMode.Default;
  public static string CurrentFilter { get; private set; } = "";

  public HammerMenuCommand()
  {
    AutoComplete.Register("hammer_menu", (int index, int subIndex) =>
    {
      if (index == 0) return ["components", "blueprints", "objects", "locations", "sounds", "visuals", "default"];
      if (index == 1) return ParameterInfo.Create("Filter text (optional)");
      return ParameterInfo.None;
    });

    Helper.Command("hammer_menu", "[mode] [filter] - Sets the hammer menu mode (components, blueprints, objects, locations, or default) with optional filter.", Execute);
  }

  private static void Execute(Terminal.ConsoleEventArgs args)
  {
    if (args.Length < 2)
    {
      HammerHelper.Message(args.Context, $"Current menu mode: {CurrentMode}" + (string.IsNullOrEmpty(CurrentFilter) ? "" : $" (filter: {CurrentFilter})"));
      return;
    }

    var mode = args[1].ToLowerInvariant();
    var filter = args.Length > 2 ? args[2] : "";

    switch (mode)
    {
      case "components":
        if (CurrentMode == MenuMode.Components && CurrentFilter == filter) mode = "default";
        break;
      case "objects":
        if (CurrentMode == MenuMode.Objects && CurrentFilter == filter) mode = "default";
        break;
      case "locations":
        if (CurrentMode == MenuMode.Locations && CurrentFilter == filter) mode = "default";
        break;
      case "blueprints":
        if (CurrentMode == MenuMode.Blueprints && CurrentFilter == filter) mode = "default";
        break;
      case "sounds":
        if (CurrentMode == MenuMode.Sounds && CurrentFilter == filter) mode = "default";
        break;
      case "visuals":
        if (CurrentMode == MenuMode.Visuals && CurrentFilter == filter) mode = "default";
        break;
    }
    switch (mode)
    {
      case "components":
        CurrentMode = MenuMode.Components;
        CurrentFilter = filter;
        HammerHelper.Message(args.Context, "Menu mode set to: Components" + (string.IsNullOrEmpty(filter) ? "" : $" (filter: {filter})"));
        break;
      case "objects":
        CurrentMode = MenuMode.Objects;
        CurrentFilter = filter;
        HammerHelper.Message(args.Context, "Menu mode set to: Objects" + (string.IsNullOrEmpty(filter) ? "" : $" (filter: {filter})"));
        break;
      case "locations":
        CurrentMode = MenuMode.Locations;
        CurrentFilter = filter;
        HammerHelper.Message(args.Context, "Menu mode set to: Locations" + (string.IsNullOrEmpty(filter) ? "" : $" (filter: {filter})"));
        break;
      case "blueprints":
        CurrentMode = MenuMode.Blueprints;
        CurrentFilter = filter;
        HammerHelper.Message(args.Context, "Menu mode set to: Blueprints" + (string.IsNullOrEmpty(filter) ? "" : $" (filter: {filter})"));
        break;
      case "sounds":
        CurrentMode = MenuMode.Sounds;
        CurrentFilter = filter;
        HammerHelper.Message(args.Context, "Menu mode set to: Sounds" + (string.IsNullOrEmpty(filter) ? "" : $" (filter: {filter})"));
        break;
      case "visuals":
        CurrentMode = MenuMode.Visuals;
        CurrentFilter = filter;
        HammerHelper.Message(args.Context, "Menu mode set to: Visuals" + (string.IsNullOrEmpty(filter) ? "" : $" (filter: {filter})"));
        break;
      case "default":
        CurrentMode = MenuMode.Default;
        CurrentFilter = "";
        HammerHelper.Message(args.Context, "Menu mode set to: Default");
        break;
      default:
        HammerHelper.Message(args.Context, "Invalid mode. Use: components, objects, locations, blueprints, sounds, visuals, or default");
        return;
    }
    if (Player.m_localPlayer)
      Player.m_localPlayer.UpdateAvailablePiecesList();
  }
}
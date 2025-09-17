using BepInEx.Configuration;
using Service;
using UnityEngine;

namespace InfinityHammer;

public partial class Configuration
{
#nullable disable
  public static ConfigEntry<int> configMaxTabs;
  public static int MaxTabs => configMaxTabs.Value;
  public static ConfigEntry<int> configItemsPerTab;
  public static int ItemsPerTab => configItemsPerTab.Value;
  public static ConfigEntry<string> configBlueprintSorting;
  public static string BlueprintSorting => configBlueprintSorting.Value;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuTypes;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuBlueprints;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuObjects;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuLocations;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuSounds;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuVisuals;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuTools;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuRooms;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuBinds;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenu;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuBuilds;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuBuildsHammer;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuBuildsHoe;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuBuildsCultivator;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuBuildsFeaster;


#nullable enable


  private static void InitCustomMenu(ConfigWrapper wrapper)
  {
    var section = "7. Build Menu";
    configMaxTabs = wrapper.Bind(section, "Max tabs", 5, "Maximum number of tabs in the custom build menu.");
    configItemsPerTab = wrapper.Bind(section, "Items per tab", 90, "Maximum number of items per tab in the custom build menu.");
    configBlueprintSorting = wrapper.Bind(section, "Blueprint sorting", BlueprintSortingMode.Folder, new ConfigDescription("How blueprints are sorted in the menu.", new AcceptableValueList<string>(BlueprintSortingMode.Category, BlueprintSortingMode.Folder, BlueprintSortingMode.CategoryAndFolder, BlueprintSortingMode.OnlyName)));
    configKeybindMenu = wrapper.BindCommand("hammer_menu", section, "Menu", new KeyboardShortcut(KeyCode.None), "Opens the custom menu.");
    configKeybindMenuBinds = wrapper.BindCommand("hammer_menu binds", section, "Menu: Binds", new KeyboardShortcut(KeyCode.None), "Opens the binds menu.");
    configKeybindMenuBlueprints = wrapper.BindCommand("hammer_menu blueprints", section, "Menu: Blueprints", new KeyboardShortcut(KeyCode.None), "Opens the blueprints menu.");
    configKeybindMenuBuilds = wrapper.BindCommand("hammer_menu builds", section, "Menu: Builds", new KeyboardShortcut(KeyCode.None), "Opens the builds menu.");
    configKeybindMenuLocations = wrapper.BindCommand("hammer_menu locations", section, "Menu: Locations", new KeyboardShortcut(KeyCode.None), "Opens the locations menu.");
    configKeybindMenuObjects = wrapper.BindCommand("hammer_menu objects", section, "Menu: Objects", new KeyboardShortcut(KeyCode.None), "Opens the objects menu.");
    configKeybindMenuRooms = wrapper.BindCommand("hammer_menu rooms", section, "Menu: Rooms", new KeyboardShortcut(KeyCode.None), "Opens the rooms menu.");
    configKeybindMenuSounds = wrapper.BindCommand("hammer_menu sounds", section, "Menu: Sounds", new KeyboardShortcut(KeyCode.None), "Opens the sounds menu.");
    configKeybindMenuTools = wrapper.BindCommand("hammer_menu tools", section, "Menu: Tools", new KeyboardShortcut(KeyCode.None), "Opens the tools menu.");
    configKeybindMenuTypes = wrapper.BindCommand("hammer_menu types", section, "Menu: Types", new KeyboardShortcut(KeyCode.None), "Opens the objects by type menu.");
    configKeybindMenuVisuals = wrapper.BindCommand("hammer_menu visuals", section, "Menu: Visuals", new KeyboardShortcut(KeyCode.None), "Opens the visuals menu.");
    configKeybindMenuBuildsHammer = wrapper.BindCommand("hammer_menu builds hammer", section, "Menu: Builds (Hammer)", new KeyboardShortcut(KeyCode.None), "Opens the builds menu with hammer filter.");
    configKeybindMenuBuildsHoe = wrapper.BindCommand("hammer_menu builds hoe", section, "Menu: Builds (Hoe)", new KeyboardShortcut(KeyCode.None), "Opens the builds menu with hoe filter.");
    configKeybindMenuBuildsCultivator = wrapper.BindCommand("hammer_menu builds cultivator", section, "Menu: Builds (Cultivator)", new KeyboardShortcut(KeyCode.None), "Opens the builds menu with cultivator filter.");
    configKeybindMenuBuildsFeaster = wrapper.BindCommand("hammer_menu builds feaster", section, "Menu: Builds (Feaster)", new KeyboardShortcut(KeyCode.None), "Opens the builds menu with feaster filter.");
  }

}

public static class BlueprintSortingMode
{
  public const string Folder = "Folder";
  public const string Category = "Category";
  public const string CategoryAndFolder = "Category and folder";
  public const string OnlyName = "Only name";
}

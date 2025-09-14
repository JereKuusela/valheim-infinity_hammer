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
  public static ConfigEntry<bool> configShowMenuButton;
  public static bool ShowMenuButton => configShowMenuButton.Value;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuComponents;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuBlueprints;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuObjects;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuLocations;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuSounds;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuVisuals;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuTools;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenuDefault;
  public static ConfigEntry<KeyboardShortcut> configKeybindMenu;

#nullable enable


  private static void InitCustomMenu(ConfigWrapper wrapper)
  {
    var section = "7. Build Menu";
    configMaxTabs = wrapper.Bind(section, "Max tabs", 5, "Maximum number of tabs in the custom build menu.");
    configItemsPerTab = wrapper.Bind(section, "Items per tab", 90, "Maximum number of items per tab in the custom build menu.");
    configBlueprintSorting = wrapper.Bind(section, "Blueprint sorting", BlueprintSortingMode.Folder, new ConfigDescription("How blueprints are sorted in the menu.", new AcceptableValueList<string>(BlueprintSortingMode.Category, BlueprintSortingMode.Folder, BlueprintSortingMode.CategoryAndFolder, BlueprintSortingMode.OnlyName)));
    configShowMenuButton = wrapper.Bind(section, "Show menu button", true, "Shows the menu button at the start of every build menu tab.");
    configKeybindMenu = wrapper.BindCommand("hammer_menu", section, "Menu", new KeyboardShortcut(KeyCode.None), "Opens the custom menu.");
    configKeybindMenuDefault = wrapper.BindCommand("hammer_menu default", section, "Menu: Default", new KeyboardShortcut(KeyCode.None), "Opens the default build menu.");
    configKeybindMenuBlueprints = wrapper.BindCommand("hammer_menu blueprints", section, "Menu: Blueprints", new KeyboardShortcut(KeyCode.None), "Opens the blueprints menu.");
    configKeybindMenuComponents = wrapper.BindCommand("hammer_menu components", section, "Menu: Components", new KeyboardShortcut(KeyCode.None), "Opens the objects by component menu.");
    configKeybindMenuObjects = wrapper.BindCommand("hammer_menu objects", section, "Menu: Objects", new KeyboardShortcut(KeyCode.None), "Opens the objects menu.");
    configKeybindMenuLocations = wrapper.BindCommand("hammer_menu locations", section, "Menu: Locations", new KeyboardShortcut(KeyCode.None), "Opens the locations menu.");
    configKeybindMenuSounds = wrapper.BindCommand("hammer_menu sounds", section, "Menu: Sounds", new KeyboardShortcut(KeyCode.None), "Opens the sounds menu.");
    configKeybindMenuTools = wrapper.BindCommand("hammer_menu tools", section, "Menu: Tools", new KeyboardShortcut(KeyCode.None), "Opens the tools menu.");
    configKeybindMenuVisuals = wrapper.BindCommand("hammer_menu visuals", section, "Menu: Visuals", new KeyboardShortcut(KeyCode.None), "Opens the visuals menu.");
  }

}

public static class BlueprintSortingMode
{
  public const string Folder = "Folder";
  public const string Category = "Category";
  public const string CategoryAndFolder = "Category and folder";
  public const string OnlyName = "Only name";
}

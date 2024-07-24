using BepInEx.Configuration;
using InfinityHammer;
using Service;
using UnityEngine;

namespace InfinityHammer;
public partial class Configuration
{
#nullable disable
  public static ConfigEntry<bool> configToolsEnabled;
  public static bool ToolsEnabled => configToolsEnabled.Value;
  public static ConfigEntry<bool> configShapeCircle;
  public static bool ShapeCircle => configShapeCircle.Value;
  public static ConfigEntry<bool> configShapeRing;
  public static bool ShapeRing => configShapeRing.Value;
  public static ConfigEntry<bool> configShapeSquare;
  public static bool ShapeSquare => configShapeSquare.Value;
  public static ConfigEntry<bool> configShapeRectangle;
  public static bool ShapeRectangle => configShapeRectangle.Value;
  public static ConfigEntry<bool> configShapeFrame;
  public static bool ShapeFrame => configShapeFrame.Value;
  public static ConfigEntry<bool> configShowCommandValues;
  public static bool AlwaysShowCommand => configShowCommandValues.Value;
  public static ConfigEntry<KeyboardShortcut> shapeKey;
  public static ConfigEntry<KeyboardShortcut> commandRadius;
  public static ConfigEntry<KeyboardShortcut> commandRotate;
  public static ConfigEntry<KeyboardShortcut> commandDepth;
  public static ConfigEntry<KeyboardShortcut> commandHeight;
  public static ConfigEntry<KeyboardShortcut> commandModifier1;
  public static ConfigEntry<KeyboardShortcut> commandModifier2;
  public static ConfigEntry<string> commandHeightAmount;

#nullable enable

  public static string ModifierKey1()
  {
    if (commandModifier1 == null) return "leftalt";
    return ConfigWrapper.GetKeys(commandModifier1.Value);
  }
  public static string ModifierKey2()
  {
    if (commandModifier2 == null) return "leftcontrol";
    return ConfigWrapper.GetKeys(commandModifier2.Value);
  }
  private static void InitTools(ConfigWrapper wrapper)
  {
    var section = "5. Tools";
    configToolsEnabled = wrapper.Bind(section, "Tools enabled", true, "Enables the tools.");
    configShowCommandValues = wrapper.Bind(section, "Show command values", false, "Always shows the command in the tool descriptions.");
    configShapeCircle = wrapper.Bind(section, "Shape circle", true, "Enables circle shape for commands.");
    configShapeRing = wrapper.Bind(section, "Shape ring", false, "Enables ring shape for commands.");
    configShapeRectangle = wrapper.Bind(section, "Shape rectangle", true, "Enables rectangle shape for commands.");
    configShapeSquare = wrapper.Bind(section, "Shape square", true, "Enables square shape for commands.");
    configShapeFrame = wrapper.Bind(section, "Shape frame", false, "Enables frame shape for commands.");

    commandHeightAmount = wrapper.Bind(section, "Command height amount", "0.1", "Meters to move.");

    section = "3. Binds";
    commandModifier1 = wrapper.Bind(section, "Command modifier 1", new KeyboardShortcut(KeyCode.LeftAlt), "");
    commandModifier2 = wrapper.Bind(section, "Command modifier 2", new KeyboardShortcut(KeyCode.LeftControl), "");

    shapeKey = wrapper.BindCommand("tool_shape", section, "Change shape", new KeyboardShortcut(KeyCode.Q), "Changes the selection shape.", "command");
    commandRadius = wrapper.BindWheelCommand(() => $"hammer_zoom_x {(Selection.Get().TerrainGrid ? "0.5" : "1")}", section, "Command radius (mouse wheel)", new KeyboardShortcut(KeyCode.None), "Changes the command radius.", "command");
    commandDepth = wrapper.BindWheelCommand(() => $"hammer_zoom_z {(Selection.Get().TerrainGrid ? "0.5" : "1")}", section, "Command depth (mouse wheel)", new KeyboardShortcut(KeyCode.LeftShift, KeyCode.LeftAlt), "Changes the command rectangle depth.", "command");
    commandHeight = wrapper.BindWheelCommand(() => $"hammer_zoom_y {commandHeightAmount.Value}", section, "Command height (mouse wheel)", new KeyboardShortcut(KeyCode.LeftShift), "Changes the command height.", "command");
    commandRotate = wrapper.BindWheelCommand("hammer_rotate", section, "Command rotation (mouse wheel)", new KeyboardShortcut(KeyCode.LeftShift, KeyCode.LeftControl), "Changes the command rotation.", "command");
  }
}

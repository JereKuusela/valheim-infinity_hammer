using BepInEx.Configuration;
using Service;
using UnityEngine;
namespace InfinityHammer;

public partial class Configuration
{
#nullable disable
  public static ConfigEntry<KeyboardShortcut> configShapeKey;
  public static ConfigEntry<KeyboardShortcut> configBuildScale;
  public static ConfigEntry<KeyboardShortcut> configCommandRadius;
  public static ConfigEntry<KeyboardShortcut> configCommandDepth;
  public static ConfigEntry<KeyboardShortcut> configCommandHeight;
  public static ConfigEntry<KeyboardShortcut> configZoopHorizontal;
  public static ConfigEntry<KeyboardShortcut> configZoopVertical;
  public static ConfigEntry<KeyboardShortcut> configZoopForward;
  public static ConfigEntry<KeyboardShortcut> configMoveUp;
  public static ConfigEntry<KeyboardShortcut> configMoveUpLarge;
  public static ConfigEntry<KeyboardShortcut> configMoveDown;
  public static ConfigEntry<KeyboardShortcut> configMoveDownLarge;
  public static ConfigEntry<KeyboardShortcut> configMoveLeft;
  public static ConfigEntry<KeyboardShortcut> configMoveLeftLarge;
  public static ConfigEntry<KeyboardShortcut> configMoveRight;
  public static ConfigEntry<KeyboardShortcut> configMoveRightLarge;
  public static ConfigEntry<KeyboardShortcut> configMoveForward;
  public static ConfigEntry<KeyboardShortcut> configMoveForwardLarge;
  public static ConfigEntry<KeyboardShortcut> configMoveBackward;
  public static ConfigEntry<KeyboardShortcut> configMoveBackwardLarge;
  public static ConfigEntry<KeyboardShortcut> configFreeze;
  public static ConfigEntry<KeyboardShortcut> configSelect;
  public static ConfigEntry<KeyboardShortcut> configSelectFrozen;
  public static ConfigEntry<KeyboardShortcut> configPick;
  public static ConfigEntry<KeyboardShortcut> configPickFrozen;
  public static ConfigEntry<KeyboardShortcut> configSelectAll;
  public static ConfigEntry<KeyboardShortcut> configSelectAllFrozen;
  public static ConfigEntry<KeyboardShortcut> configPickAll;
  public static ConfigEntry<KeyboardShortcut> configPickAllFrozen;
  public static ConfigEntry<KeyboardShortcut> configResetOffset;
  public static ConfigEntry<KeyboardShortcut> configUndo;
  public static ConfigEntry<KeyboardShortcut> configRedo;
  public static ConfigEntry<KeyboardShortcut> configCommandModifier1;
  public static ConfigEntry<KeyboardShortcut> configCommandModifier2;
  public static ConfigEntry<string> configMoveAmount;
  public static ConfigEntry<string> configMoveAmountLarge;
#nullable enable
  public static string ModifierKey1()
  {
    if (configCommandModifier1 == null) return "leftalt";
    return ConfigWrapper.GetKeys(configCommandModifier1.Value);
  }
  public static string ModifierKey2()
  {
    if (configCommandModifier2 == null) return "leftcontrol";
    return ConfigWrapper.GetKeys(configCommandModifier2.Value);
  }
  private static void InitBinds(ConfigWrapper wrapper)
  {
    var section = "2. Binds";
    configCommandModifier1 = wrapper.Bind(section, "Command modifier 1", new KeyboardShortcut(KeyCode.LeftAlt), "");
    configCommandModifier2 = wrapper.Bind(section, "Command modifier 2", new KeyboardShortcut(KeyCode.LeftControl), "");

    configMoveAmount = wrapper.Bind(section, "Move amount", "0.1", "Meters to move with move binds.");
    configMoveAmount.SettingChanged += (s, e) => wrapper.SetupBinds();
    configMoveAmountLarge = wrapper.Bind(section, "Move amount large", "1", "Meters to move with large move binds.");
    configMoveAmountLarge.SettingChanged += (s, e) => wrapper.SetupBinds();
    configHammerTools.SettingChanged += (s, e) => wrapper.SetupBinds();
    configHoeTools.SettingChanged += (s, e) => wrapper.SetupBinds();

    configShapeKey = wrapper.BindCommand("hammer_shape", section, "Change shape", new KeyboardShortcut(KeyCode.Q), "Changes the selection shape.", "build");
    configBuildScale = wrapper.BindWheelCommand("hammer_zoom 5%", section, "Build scaling (mouse wheel)", new KeyboardShortcut(KeyCode.LeftShift), "Changes the selection scale.", "build");
    configCommandRadius = wrapper.BindWheelCommand("hammer_zoom_x_cmd 1", section, "Command radius (mouse wheel)", new KeyboardShortcut(KeyCode.LeftShift), "Changes the command radius.", "build");
    configCommandDepth = wrapper.BindWheelCommand("hammer_zoom_z_cmd 1", section, "Command depth (mouse wheel)", new KeyboardShortcut(KeyCode.LeftShift, KeyCode.LeftAlt), "Changes the command rectangle depth.", "build");
    configCommandHeight = wrapper.BindWheelCommand("hammer_zoom_y_cmd 0.5", section, "Command height (mouse wheel)", new KeyboardShortcut(KeyCode.LeftShift, KeyCode.LeftControl), "Changes the command height.", "build");
    configZoopForward = wrapper.BindWheelCommand("hammer_zoop_forward auto", section, "Zooping forward (mouse wheel)", new KeyboardShortcut(KeyCode.None), "Zoops next to each other.", "build");
    configZoopHorizontal = wrapper.BindWheelCommand("hammer_zoop_right auto", section, "Zooping horizontal (mouse wheel)", new KeyboardShortcut(KeyCode.None), "Zoops next to each other.", "build");
    configZoopVertical = wrapper.BindWheelCommand("hammer_zoop_up auto", section, "Zooping vertical (mouse wheel)", new KeyboardShortcut(KeyCode.None), "Zoops next to each other.", "build");
    configMoveUp = wrapper.BindCommand(() => $"hammer_move_up {configMoveAmount.Value}", section, "Move up", new KeyboardShortcut(KeyCode.PageUp), "Precise placement.", "build");
    configMoveUpLarge = wrapper.BindCommand(() => $"hammer_move_up {configMoveAmountLarge.Value}", section, "Move up (large)", new KeyboardShortcut(KeyCode.PageUp, KeyCode.LeftAlt), "Precise placement.", "build");
    configMoveDown = wrapper.BindCommand(() => $"hammer_move_down {configMoveAmount.Value}", section, "Move down", new KeyboardShortcut(KeyCode.PageDown), "Precise placement.", "build");
    configMoveDownLarge = wrapper.BindCommand(() => $"hammer_move_down {configMoveAmountLarge.Value}", section, "Move down (large)", new KeyboardShortcut(KeyCode.PageDown, KeyCode.LeftAlt), "Precise placement.", "build");
    configMoveLeft = wrapper.BindCommand(() => $"hammer_move_left {configMoveAmount.Value}", section, "Move left", new KeyboardShortcut(KeyCode.LeftArrow), "Precise placement.", "build");
    configMoveLeftLarge = wrapper.BindCommand(() => $"hammer_move_left {configMoveAmountLarge.Value}", section, "Move left (large)", new KeyboardShortcut(KeyCode.LeftArrow, KeyCode.LeftAlt), "Precise placement.", "build");
    configMoveRight = wrapper.BindCommand(() => $"hammer_move_right {configMoveAmount.Value}", section, "Move right", new KeyboardShortcut(KeyCode.RightArrow), "Precise placement.", "build");
    configMoveRightLarge = wrapper.BindCommand(() => $"hammer_move_right {configMoveAmountLarge.Value}", section, "Move right (large)", new KeyboardShortcut(KeyCode.RightArrow, KeyCode.LeftAlt), "Precise placement.", "build");
    configMoveForward = wrapper.BindCommand(() => $"hammer_move_forward {configMoveAmount.Value}", section, "Move forward", new KeyboardShortcut(KeyCode.UpArrow), "Precise placement.", "build");
    configMoveForwardLarge = wrapper.BindCommand(() => $"hammer_move_forward {configMoveAmountLarge.Value}", section, "Move forward (large)", new KeyboardShortcut(KeyCode.UpArrow, KeyCode.LeftAlt), "Precise placement.", "build");
    configMoveBackward = wrapper.BindCommand(() => $"hammer_move_backward {configMoveAmount.Value}", section, "Move backward", new KeyboardShortcut(KeyCode.DownArrow), "Precise placement.", "build");
    configMoveBackwardLarge = wrapper.BindCommand(() => $"hammer_move_backward {configMoveAmountLarge.Value}", section, "Move backward (large)", new KeyboardShortcut(KeyCode.DownArrow, KeyCode.LeftAlt), "Precise placement.", "build");
    configFreeze = wrapper.BindCommand("hammer_freeze", section, "Freeze selection", new KeyboardShortcut(KeyCode.Keypad0), "Freezes placement position for precise placement.", "build");
    configResetOffset = wrapper.BindCommand("hammer_offset", section, "Reset offset", new KeyboardShortcut(KeyCode.None), "Resets the offset.", "build");
    configSelect = wrapper.BindCommand("hammer", section, "Select", new KeyboardShortcut(KeyCode.Keypad5), "Select the hovered object.");
    configSelectFrozen = wrapper.BindCommand("hammer freeze", section, "Select frozen", new KeyboardShortcut(KeyCode.Keypad5, KeyCode.LeftControl), "Select the hovered object.");
    configPick = wrapper.BindCommand("hammer pick", section, "Pick", new KeyboardShortcut(KeyCode.Keypad5, KeyCode.LeftAlt), "Pick the hovered object.");
    configPickFrozen = wrapper.BindCommand("hammer pick freeze", section, "Pick frozen", new KeyboardShortcut(KeyCode.Keypad5, KeyCode.LeftAlt, KeyCode.LeftControl), "Select the hovered object.");
    configSelectAll = wrapper.BindCommand("hammer connect", section, "Select building", new KeyboardShortcut(KeyCode.None), "Select the whole building.");
    configPickAll = wrapper.BindCommand("hammer connect pick", section, "Pick building", new KeyboardShortcut(KeyCode.None), "Pick the whole building.");
    configSelectAllFrozen = wrapper.BindCommand("hammer connect freeze", section, "Select building frozen", new KeyboardShortcut(KeyCode.None), "Select the whole building.");
    configPickAllFrozen = wrapper.BindCommand("hammer connect pick freeze", section, "Pick building frozen", new KeyboardShortcut(KeyCode.None), "Pick the whole building.");
    configUndo = wrapper.BindCommand("undo", section, "Undo", new KeyboardShortcut(KeyCode.Keypad7), "Undo actions.");
    configRedo = wrapper.BindCommand("redo", section, "Redo", new KeyboardShortcut(KeyCode.Keypad9), "Redo actions.");
  }
}

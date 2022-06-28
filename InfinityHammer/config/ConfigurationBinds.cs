using BepInEx.Configuration;
using Service;
using UnityEngine;
namespace InfinityHammer;

public partial class Configuration {
#nullable disable
  public static ConfigEntry<KeyboardShortcut> configShapeKey;
  public static ConfigEntry<KeyboardShortcut> configBuildScale;
  public static ConfigEntry<KeyboardShortcut> configCommandRadius;
  public static ConfigEntry<KeyboardShortcut> configCommandDepth;
  public static ConfigEntry<KeyboardShortcut> configCommandHeight;
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
  public static ConfigEntry<KeyboardShortcut> configSelectAll;
  public static ConfigEntry<KeyboardShortcut> configUndo;
  public static ConfigEntry<KeyboardShortcut> configRedo;
#nullable enable


  private static void InitBinds(ConfigWrapper wrapper) {
    var section = "2. Binds";
    configShapeKey = wrapper.BindCommand("hammer_shape", section, "Change shape", new KeyboardShortcut(KeyCode.Q), "Changes the selection shape.");
    configBuildScale = wrapper.BindWheelCommand("hammer_scale build 5%", section, "Build scaling (mouse wheel)", new KeyboardShortcut(KeyCode.LeftShift), "Changes the selection scale.");
    configCommandRadius = wrapper.BindWheelCommand("hammer_scale_x command 1", section, "Command radius (mouse wheel)", new KeyboardShortcut(KeyCode.LeftShift), "Changes the command radius.");
    configCommandDepth = wrapper.BindWheelCommand("hammer_scale_z command 1", section, "Command depth (mouse wheel)", new KeyboardShortcut(KeyCode.LeftShift, KeyCode.LeftAlt), "Changes the command rectangle depth.");
    configCommandHeight = wrapper.BindWheelCommand("hammer_scale_y command 0.5", section, "Command height (mouse wheel)", new KeyboardShortcut(KeyCode.LeftShift, KeyCode.LeftControl), "Changes the command height.");
    configMoveUp = wrapper.BindCommand("hammer_move_up 0.1", section, "Move up", new KeyboardShortcut(KeyCode.PageUp), "Precise placement.");
    configMoveUpLarge = wrapper.BindCommand("hammer_move_up 1", section, "Move up (large)", new KeyboardShortcut(KeyCode.PageUp, KeyCode.LeftShift), "Precise placement.");
    configMoveDown = wrapper.BindCommand("hammer_move_down 0.1", section, "Move down", new KeyboardShortcut(KeyCode.PageDown), "Precise placement.");
    configMoveDownLarge = wrapper.BindCommand("hammer_move_down 1", section, "Move down (large)", new KeyboardShortcut(KeyCode.PageDown, KeyCode.LeftShift), "Precise placement.");
    configMoveLeft = wrapper.BindCommand("hammer_move_left 0.1", section, "Move left", new KeyboardShortcut(KeyCode.LeftArrow), "Precise placement.");
    configMoveLeftLarge = wrapper.BindCommand("hammer_move_left 1", section, "Move left (large)", new KeyboardShortcut(KeyCode.LeftArrow, KeyCode.LeftShift), "Precise placement.");
    configMoveRight = wrapper.BindCommand("hammer_move_right 0.1", section, "Move right", new KeyboardShortcut(KeyCode.RightArrow), "Precise placement.");
    configMoveRightLarge = wrapper.BindCommand("hammer_move_right 1", section, "Move right (large)", new KeyboardShortcut(KeyCode.RightArrow, KeyCode.LeftShift), "Precise placement.");
    configMoveForward = wrapper.BindCommand("hammer_move_forward 0.1", section, "Move forward", new KeyboardShortcut(KeyCode.UpArrow), "Precise placement.");
    configMoveForwardLarge = wrapper.BindCommand("hammer_move_forward 1", section, "Move forward (large)", new KeyboardShortcut(KeyCode.UpArrow, KeyCode.LeftShift), "Precise placement.");
    configMoveBackward = wrapper.BindCommand("hammer_move_backward 0.1", section, "Move backward", new KeyboardShortcut(KeyCode.DownArrow), "Precise placement.");
    configMoveBackwardLarge = wrapper.BindCommand("hammer_move_backward 1", section, "Move backward (large)", new KeyboardShortcut(KeyCode.DownArrow, KeyCode.LeftShift), "Precise placement.");
    configFreeze = wrapper.BindCommand("hammer_freeze", section, "Freeze selection", new KeyboardShortcut(KeyCode.Keypad0), "Precise placement.");
    configSelect = wrapper.BindCommand("hammer", section, "Select", new KeyboardShortcut(KeyCode.Keypad5), "Select the hovered object.");
    configSelectAll = wrapper.BindCommand("hammer connect", section, "Select building", new KeyboardShortcut(KeyCode.Keypad5, KeyCode.LeftShift), "Select entire buildings.");
    configUndo = wrapper.BindCommand("hammer_undo", section, "Undo", new KeyboardShortcut(KeyCode.Keypad7), "Undo actions.");
    configRedo = wrapper.BindCommand("hammer_redo", section, "Redo", new KeyboardShortcut(KeyCode.Keypad9), "Redo actions.");
  }
}

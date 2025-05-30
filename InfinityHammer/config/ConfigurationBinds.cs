using BepInEx.Configuration;
using Service;
using UnityEngine;
namespace InfinityHammer;

public partial class Configuration
{
#nullable disable
  public static ConfigEntry<KeyboardShortcut> buildScale;
  public static ConfigEntry<KeyboardShortcut> zoopHorizontal;
  public static ConfigEntry<KeyboardShortcut> zoopVertical;
  public static ConfigEntry<KeyboardShortcut> zoopForward;
  public static ConfigEntry<KeyboardShortcut> moveUp;
  public static ConfigEntry<KeyboardShortcut> moveUpLarge;
  public static ConfigEntry<KeyboardShortcut> moveDown;
  public static ConfigEntry<KeyboardShortcut> moveDownLarge;
  public static ConfigEntry<KeyboardShortcut> moveLeft;
  public static ConfigEntry<KeyboardShortcut> moveLeftLarge;
  public static ConfigEntry<KeyboardShortcut> moveRight;
  public static ConfigEntry<KeyboardShortcut> moveRightLarge;
  public static ConfigEntry<KeyboardShortcut> moveForward;
  public static ConfigEntry<KeyboardShortcut> moveForwardLarge;
  public static ConfigEntry<KeyboardShortcut> moveBackward;
  public static ConfigEntry<KeyboardShortcut> moveBackwardLarge;
  public static ConfigEntry<KeyboardShortcut> freeze;
  public static ConfigEntry<KeyboardShortcut> select;
  public static ConfigEntry<KeyboardShortcut> selectFrozen;
  public static ConfigEntry<KeyboardShortcut> pick;
  public static ConfigEntry<KeyboardShortcut> pickFrozen;
  public static ConfigEntry<KeyboardShortcut> selectAll;
  public static ConfigEntry<KeyboardShortcut> selectAllFrozen;
  public static ConfigEntry<KeyboardShortcut> pickAll;
  public static ConfigEntry<KeyboardShortcut> pickAllFrozen;
  public static ConfigEntry<KeyboardShortcut> resetOffset;
  public static ConfigEntry<KeyboardShortcut> undo;
  public static ConfigEntry<KeyboardShortcut> redo;
  public static ConfigEntry<KeyboardShortcut> grid;
  public static ConfigEntry<KeyboardShortcut> gridHold;
  public static ConfigEntry<string> moveAmount;
  public static ConfigEntry<string> moveAmountLarge;
  public static ConfigEntry<string> gridPrecision;
#nullable enable
  private static void InitBinds(ConfigWrapper wrapper)
  {
    var section = "3. Binds";
    moveAmount = wrapper.Bind(section, "Move amount", "0.1", "Meters to move with move binds.");
    moveAmountLarge = wrapper.Bind(section, "Move amount large", "1", "Meters to move with large move binds.");
    gridPrecision = wrapper.Bind(section, "Grid precision", "1", "Grid precision in meters.");

    buildScale = wrapper.BindWheelCommand("hammer_zoom 5%", section, "Build scaling (mouse wheel)", new KeyboardShortcut(KeyCode.LeftShift), "Changes the selection scale.", "build");
    zoopForward = wrapper.BindWheelCommand("hammer_zoop_forward auto", section, "Zooping forward (mouse wheel)", new KeyboardShortcut(KeyCode.None), "Zoops next to each other.", "build");
    zoopHorizontal = wrapper.BindWheelCommand("hammer_zoop_right auto", section, "Zooping horizontal (mouse wheel)", new KeyboardShortcut(KeyCode.None), "Zoops next to each other.", "build");
    zoopVertical = wrapper.BindWheelCommand("hammer_zoop_up auto", section, "Zooping vertical (mouse wheel)", new KeyboardShortcut(KeyCode.None), "Zoops next to each other.", "build");
    moveUp = wrapper.BindCommand(() => $"hammer_move_up {moveAmount.Value}", section, "Move up", new KeyboardShortcut(KeyCode.PageUp), "Precise placement.", "build, command");
    moveUpLarge = wrapper.BindCommand(() => $"hammer_move_up {moveAmountLarge.Value}", section, "Move up (large)", new KeyboardShortcut(KeyCode.PageUp, KeyCode.LeftAlt), "Precise placement.", "build, command");
    moveDown = wrapper.BindCommand(() => $"hammer_move_down {moveAmount.Value}", section, "Move down", new KeyboardShortcut(KeyCode.PageDown), "Precise placement.", "build, command");
    moveDownLarge = wrapper.BindCommand(() => $"hammer_move_down {moveAmountLarge.Value}", section, "Move down (large)", new KeyboardShortcut(KeyCode.PageDown, KeyCode.LeftAlt), "Precise placement.", "build, command");
    moveLeft = wrapper.BindCommand(() => $"hammer_move_left {moveAmount.Value}", section, "Move left", new KeyboardShortcut(KeyCode.LeftArrow), "Precise placement.", "build, command");
    moveLeftLarge = wrapper.BindCommand(() => $"hammer_move_left {moveAmountLarge.Value}", section, "Move left (large)", new KeyboardShortcut(KeyCode.LeftArrow, KeyCode.LeftAlt), "Precise placement.", "build, command");
    moveRight = wrapper.BindCommand(() => $"hammer_move_right {moveAmount.Value}", section, "Move right", new KeyboardShortcut(KeyCode.RightArrow), "Precise placement.", "build, command");
    moveRightLarge = wrapper.BindCommand(() => $"hammer_move_right {moveAmountLarge.Value}", section, "Move right (large)", new KeyboardShortcut(KeyCode.RightArrow, KeyCode.LeftAlt), "Precise placement.", "build, command");
    moveForward = wrapper.BindCommand(() => $"hammer_move_forward {moveAmount.Value}", section, "Move forward", new KeyboardShortcut(KeyCode.UpArrow), "Precise placement.", "build, command");
    moveForwardLarge = wrapper.BindCommand(() => $"hammer_move_forward {moveAmountLarge.Value}", section, "Move forward (large)", new KeyboardShortcut(KeyCode.UpArrow, KeyCode.LeftAlt), "Precise placement.", "build, command");
    moveBackward = wrapper.BindCommand(() => $"hammer_move_backward {moveAmount.Value}", section, "Move backward", new KeyboardShortcut(KeyCode.DownArrow), "Precise placement.", "build, command");
    moveBackwardLarge = wrapper.BindCommand(() => $"hammer_move_backward {moveAmountLarge.Value}", section, "Move backward (large)", new KeyboardShortcut(KeyCode.DownArrow, KeyCode.LeftAlt), "Precise placement.", "build, command");
    freeze = wrapper.BindCommand("hammer_freeze", section, "Freeze selection", new KeyboardShortcut(KeyCode.Keypad0), "Freezes placement position for precise placement.", "build, command");
    resetOffset = wrapper.BindCommand("hammer_offset", section, "Reset offset", new KeyboardShortcut(KeyCode.None), "Resets the offset.", "build, command");
    select = wrapper.BindCommand("hammer", section, "Select", new KeyboardShortcut(KeyCode.Keypad5), "Select the hovered object.");
    selectFrozen = wrapper.BindCommand("hammer freeze", section, "Select frozen", new KeyboardShortcut(KeyCode.Keypad5, KeyCode.LeftControl), "Select the hovered object.");
    pick = wrapper.BindCommand("hammer pick", section, "Pick", new KeyboardShortcut(KeyCode.Keypad5, KeyCode.LeftAlt), "Pick the hovered object.");
    pickFrozen = wrapper.BindCommand("hammer pick freeze", section, "Pick frozen", new KeyboardShortcut(KeyCode.Keypad5, KeyCode.LeftAlt, KeyCode.LeftControl), "Select the hovered object.");
    selectAll = wrapper.BindCommand("hammer connect", section, "Select building", new KeyboardShortcut(KeyCode.None), "Select the whole building.");
    pickAll = wrapper.BindCommand("hammer connect pick", section, "Pick building", new KeyboardShortcut(KeyCode.None), "Pick the whole building.");
    selectAllFrozen = wrapper.BindCommand("hammer connect freeze", section, "Select building frozen", new KeyboardShortcut(KeyCode.None), "Select the whole building.");
    pickAllFrozen = wrapper.BindCommand("hammer connect pick freeze", section, "Pick building frozen", new KeyboardShortcut(KeyCode.None), "Pick the whole building.");
    undo = wrapper.BindCommand("undo", section, "Undo", new KeyboardShortcut(KeyCode.Keypad7), "Undo actions.");
    redo = wrapper.BindCommand("redo", section, "Redo", new KeyboardShortcut(KeyCode.Keypad9), "Redo actions.");
    grid = wrapper.BindCommand(() => $"hammer_grid {gridPrecision.Value} 0,0,0", section, "Grid", new KeyboardShortcut(KeyCode.None), "Toggles grid.", "build, command");
    gridHold = wrapper.BindCommand(() => $"hammer_grid {gridPrecision.Value} 0,0,0", section, "Grid hold", new KeyboardShortcut(KeyCode.None), "Enables grid when keys are down.", "hammer_grid", "build, command");
  }
}

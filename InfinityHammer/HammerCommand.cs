using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityHammer {

  public class HammerCommand {
    ///<summary>Returns the hovered object within 50 meters.</summary>
    public static ZNetView GetHovered(Terminal context) {
      if (Player.m_localPlayer == null) return null;
      var range = Settings.SelectRange == 0 ? Player.m_localPlayer.m_maxInteractDistance : Settings.SelectRange;
      var hovered = Helper.GetHovered(Player.m_localPlayer, range, Settings.SelectBlacklist);
      if (hovered == null) {
        Helper.AddMessage(context, "Nothing is being hovered.");
        return null;
      }
      return hovered.Obj;
    }
    private static void PrintSelected(Terminal terminal, GameObject obj, Vector3 scale) {
      if (Settings.DisableSelectMessages) return;
      var view = obj?.GetComponent<ZNetView>();
      var name = obj ? Utils.GetPrefabName(obj) : "";
      if (view == null)
        Helper.AddMessage(terminal, "Invalid object.");
      else if (view && view.m_syncInitialScale) {
        if (scale.x == scale.y && scale.y == scale.z)
          Helper.AddMessage(terminal, $"Selected {name} (size {scale.y.ToString("P0")}).");
        else
          Helper.AddMessage(terminal, $"Selected {name} (scale {scale.ToString("F2")}).");
      } else
        Helper.AddMessage(terminal, $"Selected {name}.");
    }
    private static void PrintSelected(Terminal terminal, GameObject obj) => PrintSelected(terminal, obj, obj?.transform.localScale ?? Vector3.one);
    private static GameObject SetItem(Terminal terminal, string name) {
      var prefab = ZNetScene.instance.GetPrefab(name);
      if (!prefab) return null;
      if (!Hammer.Set(Player.m_localPlayer, prefab, null)) return null;
      return prefab;
    }
    private static GameObject SetHoveredItem(Terminal terminal) {
      var view = GetHovered(terminal);
      if (!view) return null;
      var name = Utils.GetPrefabName(view.gameObject);
      if (!Hammer.Set(Player.m_localPlayer, view.gameObject, view.GetZDO())) return null;
      Rotating.UpdatePlacementRotation(view.gameObject);
      return view.gameObject;
    }
    private static void BindGeneral(Terminal terminal) {
      terminal.TryRunCommand("unbind keypad0");
      terminal.TryRunCommand("bind keypad0 hammer");
      terminal.TryRunCommand("unbind keypad1");
      terminal.TryRunCommand("bind keypad1 hammer_scale_down");
      terminal.TryRunCommand("unbind keypad2");
      terminal.TryRunCommand("bind keypad2 hammer_scale");
      terminal.TryRunCommand("unbind keypad3");
      terminal.TryRunCommand("bind keypad3 hammer_scale_up");
      terminal.TryRunCommand("unbind keypad7");
      terminal.TryRunCommand("bind keypad7 hammer_undo");
      terminal.TryRunCommand("unbind keypad8");
      terminal.TryRunCommand("bind keypad8 hammer_config enabled");
      terminal.TryRunCommand("unbind keypad9");
      terminal.TryRunCommand("bind keypad9 hammer_redo");
    }
    public HammerCommand() {
      CommandWrapper.Register("hammer", (int index, int subIndex) => {
        if (index == 0) return CommandWrapper.ObjectIds();
        if (index == 1) return CommandWrapper.Scale("Size of the object (if the object supports it).", subIndex);
        return null;
      }, new Dictionary<string, Func<int, List<string>>>{
        { "scale", (int index) => CommandWrapper.Scale("scale", "Size of the object (if the object supports it).", index)}
      });
      new Terminal.ConsoleCommand("hammer", "[item id] [scale=1] - Adds an object to the hammer placement (hovered object by default).", delegate (Terminal.ConsoleEventArgs args) {
        if (!Player.m_localPlayer) return;
        if (!Settings.Enabled) return;
        GameObject selected = null;
        Hammer.Equip();
        if (args.Length > 1) {
          selected = SetItem(args.Context, args[1]);

          if (args.Length < 3)
            Scaling.SetScale(1f);
          else if (args[2].Contains(",")) {
            var scale = Vector3.one;
            var split = args[2].Replace("scale=", "").Split(',');
            if (split.Length > 0) scale.x = Helper.ParseFloat(split[0], 1f);
            if (split.Length > 1) scale.y = Helper.ParseFloat(split[1], 1f);
            if (split.Length > 2) scale.z = Helper.ParseFloat(split[2], 1f);
            if (scale.x == 0f) scale.x = 1f;
            if (scale.y == 0f) scale.y = 1f;
            if (scale.z == 0f) scale.z = 1f;
            Scaling.SetScale(scale);
          } else
            Scaling.SetScale(Helper.ParseFloat(args[2].Replace("scale=", ""), 1f));
          PrintSelected(args.Context, selected, Scaling.Scale);
        } else {
          selected = SetHoveredItem(args.Context);
          if (selected != null)
            PrintSelected(args.Context, selected);
        }
      }, optionsFetcher: CommandWrapper.ObjectIds);
      CommandWrapper.RegisterEmpty("hammer_repair");
      new Terminal.ConsoleCommand("hammer_repair", "Selects the repair tool.", delegate (Terminal.ConsoleEventArgs args) {
        if (!Player.m_localPlayer) return;
        if (!Settings.Enabled) return;
        Hammer.Clear();
      });
      CommandWrapper.RegisterEmpty("hammer_undo");
      new Terminal.ConsoleCommand("hammer_undo", "Reverts object placing or removing.", delegate (Terminal.ConsoleEventArgs args) {
        if (!Player.m_localPlayer) return;
        if (!Settings.EnableUndo) return;
        UndoWrapper.Undo(args.Context);
      });
      CommandWrapper.RegisterEmpty("hammer_redo");
      new Terminal.ConsoleCommand("hammer_redo", "Restores reverted object placing or removing.", delegate (Terminal.ConsoleEventArgs args) {
        if (!Player.m_localPlayer) return;
        if (!Settings.EnableUndo) return;
        UndoWrapper.Redo(args.Context);
      });
      CommandWrapper.RegisterEmpty("hammer_scale_up");
      new Terminal.ConsoleCommand("hammer_scale_up", "Scales up the selection (if the object supports it).", delegate (Terminal.ConsoleEventArgs args) {
        if (Settings.ScaleStep <= 0f) return;
        Scaling.ScaleUp();
        Scaling.PrintScale(args.Context);
      });
      CommandWrapper.RegisterEmpty("hammer_scale_down");
      new Terminal.ConsoleCommand("hammer_scale_down", "Scales down the selection (if the object supports it).", delegate (Terminal.ConsoleEventArgs args) {
        if (Settings.ScaleStep <= 0f) return;
        Scaling.ScaleDown();
        Scaling.PrintScale(args.Context);
      });
      CommandWrapper.Register("hammer_rotate_x", (int index) => {
        if (index == 0) return CommandWrapper.Info("Rotates around the X axis.");
        if (index == 1) return CommandWrapper.Info("Direction (1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_rotate_x", "[value] [direction=1] - Rotates around the X axis.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        Rotating.RotateX(Helper.ParseDirection(args.Args, 2) * Helper.ParseFloat(args[1], 0f));
      });
      CommandWrapper.Register("hammer_rotate_y", (int index) => {
        if (index == 0) return CommandWrapper.Info("Rotates around the Y axis.");
        if (index == 1) return CommandWrapper.Info("Direction (1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_rotate_y", "[value] [direction=1] - Rotates around the Y axis.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        Rotating.RotateY(Helper.ParseDirection(args.Args, 2) * Helper.ParseFloat(args[1], 0f));
      });
      CommandWrapper.Register("hammer_rotate_z", (int index) => {
        if (index == 0) return CommandWrapper.Info("Rotates around the Z axis.");
        if (index == 1) return CommandWrapper.Info("Direction (1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_rotate_z", "[value] [direction=1] - Rotates around the Z axis.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        Rotating.RotateZ(Helper.ParseDirection(args.Args, 2) * Helper.ParseFloat(args[1], 0f));
      });
      CommandWrapper.Register("hammer_move_x", (int index) => {
        if (index == 0) return CommandWrapper.Info("Moves the X offset (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
        if (index == 1) return CommandWrapper.Info("Direction (default 1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_move_x", "[value=auto] [direction=1] - Moves the X offset.", delegate (Terminal.ConsoleEventArgs args) {
        var amount = 0f;
        if (args.Length < 2 || args[1].Contains("auto")) {
          amount = Bounds.Get[Utils.GetPrefabName(Player.m_localPlayer.m_placementGhost)].x;
          amount *= Helper.ParseMultiplier(args.Args, 1);
        } else
          amount = Helper.ParseFloat(args[1], 0f);
        Offset.MoveX(Helper.ParseDirection(args.Args, 2) * amount);
        Offset.Print(args.Context);
      });
      CommandWrapper.Register("hammer_move_y", (int index) => {
        if (index == 0) return CommandWrapper.Info("Moves the Y offset (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
        if (index == 1) return CommandWrapper.Info("Direction (default 1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_move_y", "[value=auto] [direction=1] - Moves the Y offset.", delegate (Terminal.ConsoleEventArgs args) {
        var amount = 0f;
        if (args.Length < 2 || args[1].Contains("auto")) {
          amount = Bounds.Get[Utils.GetPrefabName(Player.m_localPlayer.m_placementGhost)].y;
          amount *= Helper.ParseMultiplier(args.Args, 1);
        } else
          amount = Helper.ParseFloat(args[1], 0f);
        Offset.MoveY(Helper.ParseDirection(args.Args, 2) * amount);
        Offset.Print(args.Context);
      });
      CommandWrapper.Register("hammer_move_z", (int index) => {
        if (index == 0) return CommandWrapper.Info("Moves the Z offset (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
        if (index == 1) return CommandWrapper.Info("Direction (default 1 or -1).");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_move_z", "[value=auto] [direction=1] - Moves the Z offset.", delegate (Terminal.ConsoleEventArgs args) {
        var amount = 0f;
        if (args.Length < 2 || args[1].Contains("auto")) {
          amount = Bounds.Get[Utils.GetPrefabName(Player.m_localPlayer.m_placementGhost)].z;
          amount *= Helper.ParseMultiplier(args.Args, 1);
        } else
          amount = Helper.ParseFloat(args[1], 0f);
        Offset.MoveZ(Helper.ParseDirection(args.Args, 2) * amount);
        Offset.Print(args.Context);
      });
      CommandWrapper.Register("hammer_move", (int index) => {
        if (index < 3) return CommandWrapper.XYZ("Moves the offset.", index);
        return null;
      });
      new Terminal.ConsoleCommand("hammer_move", "[x,y,z] - Moves the offset.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        var value = Vector3.zero;
        var split = args[1].Split(',');
        if (split.Length > 0) value.x = Helper.ParseFloat(split[0], 1f);
        if (split.Length > 1) value.y = Helper.ParseFloat(split[1], 1f);
        if (split.Length > 2) value.z = Helper.ParseFloat(split[2], 1f);
        Offset.Move(value);
        Offset.Print(args.Context);
      });
      CommandWrapper.Register("hammer_offset_x", (int index) => {
        if (index == 0) return CommandWrapper.Info("Sets the X offset.");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_offset_x", "[value] - Sets the X offset.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        Offset.SetX(Helper.ParseFloat(args[1], 0f));
        Offset.Print(args.Context);
      });
      CommandWrapper.Register("hammer_offset_y", (int index) => {
        if (index == 0) return CommandWrapper.Info("Sets the Y offset.");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_offset_y", "[value] - Sets the Y offset.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        Offset.SetY(Helper.ParseFloat(args[1], 0f));
        Offset.Print(args.Context);
      });
      CommandWrapper.Register("hammer_offset_z", (int index) => {
        if (index == 0) return CommandWrapper.Info("Sets the Z offset.");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_offset_z", "[value] - Sets the Z offset.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        Offset.SetZ(Helper.ParseFloat(args[1], 0f));
        Offset.Print(args.Context);
      });
      CommandWrapper.Register("hammer_offset", (int index, int subIndex) => {
        if (index == 0) return CommandWrapper.XYZ("Sets the offset.", subIndex);
        return null;
      });
      new Terminal.ConsoleCommand("hammer_offset", "[x,y,z] - Sets the offset.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        var value = Vector3.zero;
        var split = args[1].Split(',');
        if (split.Length > 0) value.x = Helper.ParseFloat(split[0], 1f);
        if (split.Length > 1) value.y = Helper.ParseFloat(split[1], 1f);
        if (split.Length > 2) value.z = Helper.ParseFloat(split[2], 1f);
        Offset.Set(value);
        Offset.Print(args.Context);
      });
      CommandWrapper.Register("hammer_scale", (int index, int subIndex) => {
        if (index == 0) return CommandWrapper.Scale("Sets the size (if the object supports it).", subIndex);
        return null;
      });
      new Terminal.ConsoleCommand("hammer_scale", "[scale=1] - Sets the size (if the object supports it).", delegate (Terminal.ConsoleEventArgs args) {
        if (Settings.ScaleStep <= 0f) return;
        if (args.Length < 2)
          Scaling.SetScale(1f);
        else if (args[1].Contains(",")) {
          var scale = Vector3.one;
          var split = args[1].Replace("scale=", "").Split(',');
          if (split.Length > 0) scale.x = Helper.ParseFloat(split[0], 1f);
          if (split.Length > 1) scale.y = Helper.ParseFloat(split[1], 1f);
          if (split.Length > 2) scale.z = Helper.ParseFloat(split[2], 1f);
          if (scale.x == 0f) scale.x = 1f;
          if (scale.y == 0f) scale.y = 1f;
          if (scale.z == 0f) scale.z = 1f;
          Scaling.SetScale(scale);
        } else
          Scaling.SetScale(Helper.ParseFloat(args[1].Replace("scale=", ""), 1f));
        Scaling.PrintScale(args.Context);
      });
      CommandWrapper.Register("hammer_config", (int index) => {
        if (index == 0) return Settings.Options;
        if (index == 1) return CommandWrapper.Info("Value.");
        return null;
      });
      new Terminal.ConsoleCommand("hammer_config", "[key] [value] - Toggles or sets config value.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        if (args.Length == 2)
          Settings.UpdateValue(args.Context, args[1], "");
        else
          Settings.UpdateValue(args.Context, args[1], string.Join(" ", args.Args.Skip(2)));
      }, optionsFetcher: () => Settings.Options);
      CommandWrapper.RegisterEmpty("hammer_place");
      new Terminal.ConsoleCommand("hammer_place", "Places the current object with a command.", delegate (Terminal.ConsoleEventArgs args) {
        if (!Player.m_localPlayer) return;
        Player.m_localPlayer.m_placePressedTime = Time.time;
        Player.m_localPlayer.m_lastToolUseTime = 0f;
        Player.m_localPlayer.UpdatePlacement(true, 0f);
      });
      CommandWrapper.RegisterEmpty("hammer_setup_binds");
      new Terminal.ConsoleCommand("hammer_setup_binds", "Sets recommended key bindings.", delegate (Terminal.ConsoleEventArgs args) {
        BindGeneral(args.Context);
        var isDev = InfinityHammer.IsServerDevcommands;
        var modifier = (isDev ? " keys=-leftalt" : "");
        args.Context.TryRunCommand("unbind rightcontrol");
        args.Context.TryRunCommand("bind rightcontrol hammer_offset 0,0,0");
        args.Context.TryRunCommand("unbind rightarrow");
        args.Context.TryRunCommand("bind rightarrow hammer_move_z -0.1" + modifier);
        if (isDev) args.Context.TryRunCommand("bind rightarrow hammer_move_z -1 keys=leftalt");
        args.Context.TryRunCommand("unbind leftarrow");
        args.Context.TryRunCommand("bind leftarrow hammer_move_z 0.1" + modifier);
        if (isDev) args.Context.TryRunCommand("bind leftarrow hammer_move_z 1 keys=leftalt");
        args.Context.TryRunCommand("unbind downarrow");
        args.Context.TryRunCommand("bind downarrow hammer_move_y -0.1" + modifier);
        if (isDev) args.Context.TryRunCommand("bind downarrow hammer_move_y -1 keys=leftalt");
        args.Context.TryRunCommand("unbind uparrow");
        args.Context.TryRunCommand("bind uparrow hammer_move_y 0.1" + modifier);
        if (isDev) args.Context.TryRunCommand("bind uparrow hammer_move_y 1 keys=leftalt");
        Helper.AddMessage(args.Context, "Keybindings set for Infinity Hammer (with Dedicated Server Devcommands mod).");
      });
      CommandWrapper.RegisterEmpty("hammer_add_piece_components");
      new Terminal.ConsoleCommand("hammer_add_piece_components", "Adds the Piece component to every prefab to allow copying them with PlanBuild.", delegate (Terminal.ConsoleEventArgs args) {
        if (!ZNetScene.instance) return;
        if (!Settings.IsCheats) {
          Helper.AddMessage(args.Context, "Error: Cheats are not allowed.");
          return;
        }
        foreach (var prefab in ZNetScene.instance.m_prefabs) {
          if (prefab.GetComponent<Piece>()) continue;
          if (prefab.name == "Player") continue;
          if (prefab.name.StartsWith("_", StringComparison.Ordinal)) continue;
          if (prefab.name.StartsWith("fx_", StringComparison.Ordinal)) continue;
          if (prefab.name.StartsWith("sfx_", StringComparison.Ordinal)) continue;
          var piece = prefab.AddComponent<Piece>();
          piece.m_name = Utils.GetPrefabName(piece.gameObject);
          piece.m_clipEverything = true;
        }
        foreach (var instance in ZNetScene.instance.m_instances.Values) {
          if (instance.gameObject.GetComponent<Piece>()) continue;
          if (instance.gameObject.name == "Player(Clone)") continue;
          if (instance.gameObject.name.StartsWith("_", StringComparison.Ordinal)) continue;
          if (instance.gameObject.name.StartsWith("fx_", StringComparison.Ordinal)) continue;
          if (instance.gameObject.name.StartsWith("sfx_", StringComparison.Ordinal)) continue;
          var piece = instance.gameObject.AddComponent<Piece>();
          piece.m_name = Utils.GetPrefabName(piece.gameObject);
          piece.m_clipEverything = true;
        }
        Helper.AddMessage(args.Context, "Piece component added to every prefab.");
      });
    }
  }
}
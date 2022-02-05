using UnityEngine;

namespace InfinityHammer {

  public class HammerCommand {
    ///<summary>Returns the hovered object within 50 meters.</summary>
    public static ZNetView GetHovered(Terminal context) {
      if (Player.m_localPlayer == null) return null;
      var range = Settings.SelectRange == 0 ? Player.m_localPlayer.m_maxInteractDistance : Settings.SelectRange;
      var hovered = Helper.GetHovered(Player.m_localPlayer, range);
      if (hovered == null) {
        Helper.AddMessage(context, "Nothing is being hovered.");
        return null;
      }
      return hovered.Obj;
    }
    private static void PrintSelected(Terminal terminal, GameObject obj, Vector3 scale) {
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
      }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
      new Terminal.ConsoleCommand("hammer_undo", "Reverts object placing or removing.", delegate (Terminal.ConsoleEventArgs args) {
        if (!Player.m_localPlayer) return;
        if (!Settings.EnableUndo) return;
        UndoWrapper.Undo(args.Context);
      });
      new Terminal.ConsoleCommand("hammer_redo", "Restores reverted object placing or removing.", delegate (Terminal.ConsoleEventArgs args) {
        if (!Player.m_localPlayer) return;
        if (!Settings.EnableUndo) return;
        UndoWrapper.Redo(args.Context);

      });
      new Terminal.ConsoleCommand("hammer_scale_up", "Scales up the selection (if the object supports it).", delegate (Terminal.ConsoleEventArgs args) {
        if (Settings.ScaleStep <= 0f) return;
        Scaling.ScaleUp();
        Scaling.PrintScale(args.Context);
      });
      new Terminal.ConsoleCommand("hammer_scale_down", "Scales down the selection (if the object supports it).", delegate (Terminal.ConsoleEventArgs args) {
        if (Settings.ScaleStep <= 0f) return;
        Scaling.ScaleDown();
        Scaling.PrintScale(args.Context);
      });
      new Terminal.ConsoleCommand("hammer_move_x", "[value] - Moves the X offset.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        Offset.MoveX(Helper.ParseFloat(args[1], 0f));
        Offset.Print(args.Context);
      });
      new Terminal.ConsoleCommand("hammer_move_y", "[value] - Moves the Y offset.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        Offset.MoveY(Helper.ParseFloat(args[1], 0f));
        Offset.Print(args.Context);
      });
      new Terminal.ConsoleCommand("hammer_move_z", "[value] - Moves the Z offset.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        Offset.MoveZ(Helper.ParseFloat(args[1], 0f));
        Offset.Print(args.Context);
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
      new Terminal.ConsoleCommand("hammer_offset_x", "[value] - Sets the X offset.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        Offset.SetX(Helper.ParseFloat(args[1], 0f));
        Offset.Print(args.Context);
      });
      new Terminal.ConsoleCommand("hammer_offset_y", "[value] - Sets the Y offset.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        Offset.SetY(Helper.ParseFloat(args[1], 0f));
        Offset.Print(args.Context);
      });
      new Terminal.ConsoleCommand("hammer_offset_z", "[value] - Sets the Z offset.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        Offset.SetZ(Helper.ParseFloat(args[1], 0f));
        Offset.Print(args.Context);
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
      new Terminal.ConsoleCommand("hammer_scale", "[scale=1] - Sets scale of the selection (if the object supports it).", delegate (Terminal.ConsoleEventArgs args) {
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
      new Terminal.ConsoleCommand("hammer_config", "[key] [value] - Toggles or sets config value.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        if (args.Length == 2)
          Settings.UpdateValue(args.Context, args[1], "");
        else
          Settings.UpdateValue(args.Context, args[1], args[2]);
      }, optionsFetcher: () => Settings.Options);
      new Terminal.ConsoleCommand("hammer_setup_binds", "Sets recommended key bindings.", delegate (Terminal.ConsoleEventArgs args) {
        BindGeneral(args.Context);
        var isDev = InfinityHammer.IsDev;
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
      new Terminal.ConsoleCommand("hammer_add_piece_components", "Adds the Piece component to every prefab to allow copying them with PlanBuild.", delegate (Terminal.ConsoleEventArgs args) {
        if (!ZNetScene.instance) return;
        if (!Settings.IsCheats) {
          Helper.AddMessage(args.Context, "Error: Cheats are not allowed.");
          return;
        }
        foreach (var prefab in ZNetScene.instance.m_prefabs) {
          if (prefab.GetComponent<Piece>()) continue;
          var piece = prefab.AddComponent<Piece>();
          piece.m_name = Utils.GetPrefabName(piece.gameObject);
          piece.m_clipEverything = true;
        }
        foreach (var instance in ZNetScene.instance.m_instances.Values) {
          if (instance.gameObject.GetComponent<Piece>()) continue;
          var piece = instance.gameObject.AddComponent<Piece>();
          piece.m_name = Utils.GetPrefabName(piece.gameObject);
          piece.m_clipEverything = true;
        }
        Helper.AddMessage(args.Context, "Piece component added to every prefab.");
      });
    }
  }
}
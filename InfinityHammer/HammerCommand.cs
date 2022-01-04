using Service;
using UnityEngine;

namespace InfinityHammer {

  public class HammerCommand {
    ///<summary>Returns the hovered object within 50 meters.</summary>
    public static ZNetView GetHovered(Terminal context) {
      if (Player.m_localPlayer == null) return null;
      var interact = Player.m_localPlayer.m_maxInteractDistance;
      Player.m_localPlayer.m_maxInteractDistance = Settings.SelectRange;
      Player.m_localPlayer.FindHoverObject(out var obj, out var creature);
      Player.m_localPlayer.m_maxInteractDistance = interact;
      if (obj == null) {
        Helper.AddMessage(context, "Nothing is being hovered.");
        return null;
      }
      var view = obj.GetComponentInParent<ZNetView>();
      if (view == null) {
        Helper.AddMessage(context, "Nothing is being hovered.");
        return null;
      }
      return view;
    }
    private static void PrintScale(Terminal terminal, GameObject obj) {
      if (obj)
        Helper.AddMessage(terminal, $"Scale set to {obj.transform.localScale.y.ToString("P0")}.");
      else
        Helper.AddMessage(terminal, "Selected object doesn't support scaling.");
    }
    private static void PrintSelected(Terminal terminal, GameObject obj) {
      var view = obj?.GetComponent<ZNetView>();
      var name = obj ? Utils.GetPrefabName(obj) : "";
      if (view == null)
        Helper.AddMessage(terminal, "Invalid object.");
      else if (view && view.m_syncInitialScale)
        Helper.AddMessage(terminal, $"Selected {name} (size {obj.transform.localScale.y.ToString("P0")}).");
      else
        Helper.AddMessage(terminal, $"Selected {name}.");
    }
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
      Hammer.Rotate(view.gameObject);
      return view.gameObject;

    }
    public HammerCommand() {
      new Terminal.ConsoleCommand("hammer", "[item id] - Adds an object to the hammer placement (hovered object by default).", delegate (Terminal.ConsoleEventArgs args) {
        if (!Player.m_localPlayer) return;
        if (!Settings.Enabled) return;
        GameObject selected = null;
        if (args.Length > 1)
          selected = SetItem(args.Context, args[1]);
        else
          selected = SetHoveredItem(args.Context);
        PrintSelected(args.Context, selected);
      }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
      new Terminal.ConsoleCommand("hammer_undo", "Reverts object placing or removing.", delegate (Terminal.ConsoleEventArgs args) {
        if (!Player.m_localPlayer) return;
        if (!Settings.EnableUndo) return;
        UndoManager.Undo();
      });
      new Terminal.ConsoleCommand("hammer_redo", "Restores reverted object placing or removing.", delegate (Terminal.ConsoleEventArgs args) {
        if (!Player.m_localPlayer) return;
        if (!Settings.EnableUndo) return;
        UndoManager.Redo();
      });
      new Terminal.ConsoleCommand("hammer_scale_up", "Scales up the selection (if the object supports it).", delegate (Terminal.ConsoleEventArgs args) {
        if (Settings.ScaleStep <= 0f) return;
        PrintScale(args.Context, Hammer.ScaleUp());
      });
      new Terminal.ConsoleCommand("hammer_scale_down", "Scales down the selection (if the object supports it).", delegate (Terminal.ConsoleEventArgs args) {
        if (Settings.ScaleStep <= 0f) return;
        PrintScale(args.Context, Hammer.ScaleDown());
      });
      new Terminal.ConsoleCommand("hammer_scale", "[value=1] - Sets scale of the selection (if the object supports it).", delegate (Terminal.ConsoleEventArgs args) {
        if (Settings.ScaleStep <= 0f) return;
        GameObject scaled = null;
        if (args.Length < 2)
          scaled = Hammer.SetScale(1f);
        else if (args[1].Contains(",")) {
          var scale = Vector3.one;
          var split = args[1].Split(',');
          if (split.Length > 0) scale.x = Helper.ParseFloat(split[0], 1f);
          if (split.Length > 1) scale.y = Helper.ParseFloat(split[2], 1f);
          if (split.Length > 2) scale.z = Helper.ParseFloat(split[3], 1f);
          if (scale.x == 0f) scale.x = 1f;
          if (scale.y == 0f) scale.y = 1f;
          if (scale.z == 0f) scale.z = 1f;
          scaled = Hammer.SetScale(scale);
        } else
          scaled = Hammer.SetScale(Helper.ParseFloat(args[1], 1f));
        PrintScale(args.Context, scaled);
      });
      new Terminal.ConsoleCommand("hammer_config", "[key] [value] - Toggles or sets config value.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        if (args.Length == 2)
          Settings.UpdateValue(args.Context, args[1], "");
        else
          Settings.UpdateValue(args.Context, args[1], args[2]);
      }, optionsFetcher: () => Settings.Options);
    }

  }
}
using Service;
using UnityEngine;

namespace InfinityHammer {

  public class HammerCommand {
    public static GameObject GetPrefab(string name) {
      var prefab = ZNetScene.instance.GetPrefab(name);
      if (!prefab)
        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Missing object " + name, 0, null);
      return prefab;
    }

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
    public HammerCommand() {
      new Terminal.ConsoleCommand("hammer", "[item id] - Adds an object to the hammer placement (hovered object by default).", delegate (Terminal.ConsoleEventArgs args) {
        if (!Player.m_localPlayer) return;
        if (!Settings.Enabled) return;
        if (args.Length > 1) {
          var name = args[1];
          var prefab = GetPrefab(name);
          if (prefab)
            Hammer.Set(Player.m_localPlayer, prefab, null);
        } else {
          var view = GetHovered(args.Context);
          if (!view) return;
          var name = Utils.GetPrefabName(view.gameObject);
          if (Hammer.Set(Player.m_localPlayer, view.gameObject, view.GetZDO())) {
            Hammer.Rotate(view.gameObject);
            Helper.AddMessage(args.Context, $"Selected {name} (size {view.transform.localScale.y.ToString("P0")}).");
          } else {
            Helper.AddMessage(args.Context, "Invalid object.");
          }
        }
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
        else
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
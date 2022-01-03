using UnityEngine;

namespace InfinityHammer {

  public class HammerCommand {
    public static void AddMessage(Terminal context, string message) {
      context.AddString(message);
      Player.m_localPlayer?.Message(MessageHud.MessageType.TopLeft, message);
    }
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
      Player.m_localPlayer.m_maxInteractDistance = 50f;
      Player.m_localPlayer.FindHoverObject(out var obj, out var creature);
      Player.m_localPlayer.m_maxInteractDistance = interact;
      if (obj == null) {
        AddMessage(context, "Nothing is being hovered.");
        return null;
      }
      var view = obj.GetComponentInParent<ZNetView>();
      if (view == null) {
        AddMessage(context, "Nothing is being hovered.");
        return null;
      }
      return view;
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
            var rotation = view.gameObject.transform.rotation;
            Player.m_localPlayer.m_placeRotation = Mathf.RoundToInt(rotation.eulerAngles.y / 22.5f);
            var gizmo = GameObject.Find("GizmoRoot(Clone)");
            if (gizmo)
              gizmo.transform.rotation = rotation;
            AddMessage(args.Context, "Selected " + name + ".");
          } else {
            AddMessage(args.Context, "Invalid object.");
          }
        }
      }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
    }
  }
}
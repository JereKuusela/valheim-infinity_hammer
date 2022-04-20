using UnityEngine;
using System;
namespace InfinityHammer;
public class HammerCommand {
  ///<summary>Returns the hovered object.</summary>
  private static ZNetView GetHovered() {
    if (Player.m_localPlayer == null) throw new InvalidOperationException("Error: No player.");
    var range = Settings.SelectRange == 0 ? Player.m_localPlayer.m_maxInteractDistance : Settings.SelectRange;
    var hovered = Helper.GetHovered(Player.m_localPlayer, range, Settings.SelectBlacklist);
    if (hovered == null) throw new InvalidOperationException("Nothing is being hovered.");
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
    if (!prefab) throw new InvalidOperationException("Error: Invalid prefab.");
    if (!Hammer.Set(Player.m_localPlayer, prefab, null)) throw new InvalidOperationException("Error: Invalid prefab.");
    return prefab;
  }
  private static GameObject SetHoveredItem() {
    var view = GetHovered();
    var name = Utils.GetPrefabName(view.gameObject);
    if (!Hammer.Set(Player.m_localPlayer, view.gameObject, view.GetZDO())) throw new InvalidOperationException("Error: Invalid prefab.");
    Rotating.UpdatePlacementRotation(view.gameObject);
    return view.gameObject;
  }

  public HammerCommand() {
    CommandWrapper.Register("hammer", (int index, int subIndex) => {
      if (index == 0) return CommandWrapper.ObjectIds();
      if (index == 1) return CommandWrapper.Scale("Size of the object (if the object can be scaled).", subIndex);
      return null;
    }, new() {
      { "scale", (int index) => CommandWrapper.Scale("scale", "Size of the object (if the object can be scaled).", index) }
    });
    new Terminal.ConsoleCommand("hammer", "[item id] [scale=1] - Selects the object to be placed (the hovered object by default).", (Terminal.ConsoleEventArgs args) => {
      if (!Player.m_localPlayer) return;
      if (!Settings.Enabled) return;
      Hammer.Equip();

      try {
        if (args.Length > 1) {
          var selected = SetItem(args.Context, args[1]);

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
          var selected = SetHoveredItem();
          PrintSelected(args.Context, selected);
        }
      } catch (InvalidOperationException e) {
        Helper.AddMessage(args.Context, e.Message);
      }
    }, optionsFetcher: CommandWrapper.ObjectIds);
  }
}

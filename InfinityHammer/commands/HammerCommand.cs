using System;
using System.Collections.Generic;
using UnityEngine;
namespace InfinityHammer;
public class HammerCommand {
  ///<summary>Returns the hovered object.</summary>
  private static ZNetView GetHovered(Terminal context) {
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

  public HammerCommand() {
    CommandWrapper.Register("hammer", (int index, int subIndex) => {
      if (index == 0) return CommandWrapper.ObjectIds();
      if (index == 1) return CommandWrapper.Scale("Size of the object (if the object can be scaled).", subIndex);
      return null;
    }, new Dictionary<string, Func<int, List<string>>>{
        { "scale", (int index) => CommandWrapper.Scale("scale", "Size of the object (if the object can be scaled).", index)}
      });
    new Terminal.ConsoleCommand("hammer", "[item id] [scale=1] - Selects the object to be placed (the hovered object by default).", (Terminal.ConsoleEventArgs args) => {
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
  }
}

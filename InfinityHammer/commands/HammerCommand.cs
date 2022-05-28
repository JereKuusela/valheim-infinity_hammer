using System;
using UnityEngine;
namespace InfinityHammer;
public class HammerCommand {
  ///<summary>Returns the hovered object.</summary>
  private static ZNetView GetHovered() {
    var range = Settings.SelectRange == 0 ? Player.m_localPlayer.m_maxInteractDistance : Settings.SelectRange;
    var hovered = Helper.GetHovered(Player.m_localPlayer, range, Settings.SelectBlacklist);
    if (hovered == null) throw new InvalidOperationException("Nothing is being hovered.");
    return hovered.Obj;
  }
  private static void PrintSelected(Terminal terminal, GameObject obj) {
    if (Settings.DisableSelectMessages) return;
    var scale = obj.transform.localScale;
    var view = obj.GetComponent<ZNetView>();
    var name = Utils.GetPrefabName(obj);
    if (view && view.m_syncInitialScale) {
      if (scale.x == scale.y && scale.y == scale.z)
        Helper.AddMessage(terminal, $"Selected {name} (size {scale.y.ToString("P0")}).");
      else
        Helper.AddMessage(terminal, $"Selected {name} (scale {scale.ToString("F2")}).");
    } else
      Helper.AddMessage(terminal, $"Selected {name}.");
  }

  public HammerCommand() {
    CommandWrapper.Register("hammer", (int index, int subIndex) => {
      if (index == 0) return CommandWrapper.ObjectIds();
      if (index == 1) return CommandWrapper.Scale("Size of the object (if the object can be scaled).", subIndex);
      return null;
    }, new() {
      { "scale", (int index) => CommandWrapper.Scale("scale", "Size of the object (if the object can be scaled).", index) }
    });
    Helper.Command("hammer", "[item id] [scale=1] - Selects the object to be placed (the hovered object by default).", (args) => {
      Helper.EnabledCheck();
      Hammer.Equip();
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
      var selected = args.Length > 1 ? Selection.Set(args[1]) : Selection.Set(GetHovered());
      PrintSelected(args.Context, selected);
    }, CommandWrapper.ObjectIds);
  }
}

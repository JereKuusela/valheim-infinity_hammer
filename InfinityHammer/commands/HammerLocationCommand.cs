using System;
using UnityEngine;
namespace InfinityHammer;
public class HammerLocationCommand {
  private static void PrintSelected(Terminal terminal, GameObject obj, Vector3 scale) {
    if (Settings.DisableSelectMessages) return;
    var name = obj ? Utils.GetPrefabName(obj) : "";
    Helper.AddMessage(terminal, $"Selected {name}.");
  }
  private static void PrintSelected(Terminal terminal, GameObject obj) => PrintSelected(terminal, obj, obj?.transform.localScale ?? Vector3.one);
  private static GameObject SetItem(Terminal terminal, string name, int seed) {
    var prefab = Helper.GetLocation(name).m_prefab;
    if (!prefab) throw new InvalidOperationException("Error: Invalid prefab.");
    if (!Hammer.SetLocation(Player.m_localPlayer, prefab, seed)) throw new InvalidOperationException("Error: Invalid prefab.");
    return prefab;
  }

  public HammerLocationCommand() {
    CommandWrapper.Register("hammer_location", (int index, int subIndex) => {
      if (index == 0) return CommandWrapper.LocationIds();
      if (index == 1) return CommandWrapper.Scale("Size of the object (if the object can be scaled).", subIndex);
      return null;
    }, new() {
      { "scale", (int index) => CommandWrapper.Scale("scale", "Size of the object (if the object can be scaled).", index) }
    });
    new Terminal.ConsoleCommand("hammer_location", "[location id] [scale=1] - Selects the location to be placed.", (Terminal.ConsoleEventArgs args) => {
      if (!Player.m_localPlayer) return;
      if (!Settings.Enabled) return;
      if (args.Length < 2) return;
      Hammer.Equip();
      var rng = new System.Random();
      try {
        var seed = args.TryParameterInt(2, rng.Next());
        var selected = SetItem(args.Context, args[1], seed);

        PrintSelected(args.Context, selected, Scaling.Scale);
      } catch (InvalidOperationException e) {
        Helper.AddMessage(args.Context, e.Message);
      }
    }, optionsFetcher: CommandWrapper.LocationIds);
  }
}

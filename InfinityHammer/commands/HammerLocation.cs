using System;
using UnityEngine;
namespace InfinityHammer;
public class HammerLocationCommand {
  private static void PrintSelected(Terminal terminal, GameObject obj) {
    if (Settings.DisableSelectMessages) return;
    var name = obj ? Utils.GetPrefabName(obj) : "";
    Helper.AddMessage(terminal, $"Selected {name}.");
  }
  private static GameObject SetItem(Terminal terminal, string name, int seed) {
    var location = ZoneSystem.instance.GetLocation(name);
    if (location == null) throw new InvalidOperationException("Error: Location not found.");
    if (!location.m_prefab) throw new InvalidOperationException("Error: Invalid location");
    if (!Hammer.SetLocation(Player.m_localPlayer, location, seed)) throw new InvalidOperationException("Error: Invalid location.");
    return location.m_prefab;
  }

  public HammerLocationCommand() {
    CommandWrapper.Register("hammer_location", (int index, int subIndex) => {
      if (index == 0) return CommandWrapper.LocationIds();
      if (index == 1) return CommandWrapper.Info("Seed for the random output. 0 = random, all = enable all objects.");
      if (index == 2) return CommandWrapper.Info("Any value forces random damage on structures (disabled by default).");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_location", "[location id] [seed=0] [random damage] - Selects the location to be placed.", (args) => {
      if (!Player.m_localPlayer) return;
      if (!Settings.IsCheats) {
        Helper.AddMessage(args.Context, "Error: This command is disabled.");
        return;
      }
      if (args.Length < 2) return;
      Hammer.Equip();
      try {
        Hammer.AllLocationsObjects = args.Length > 2 && args[2] == "all";
        Hammer.RandomLocationDamage = args.Length > 3 && args[3] == "1";
        var rng = new System.Random();
        var seed = args.TryParameterInt(2, rng.Next());
        if (seed == 0) seed = rng.Next();
        var selected = SetItem(args.Context, args[1], seed);

        PrintSelected(args.Context, selected);
      } catch (InvalidOperationException e) {
        Helper.AddMessage(args.Context, e.Message);
      }
    }, optionsFetcher: CommandWrapper.LocationIds);
  }
}

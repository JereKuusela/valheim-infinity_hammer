using System;
using UnityEngine;
namespace InfinityHammer;
public class HammerLocationCommand
{
  private static void PrintSelected(Terminal terminal, GameObject obj)
  {
    if (Configuration.DisableSelectMessages) return;
    var name = obj ? Utils.GetPrefabName(obj) : "";
    Helper.AddMessage(terminal, $"Selected {name}.");
  }

  public HammerLocationCommand()
  {
    CommandWrapper.Register("hammer_location", (int index, int subIndex) =>
    {
      if (index == 0) return CommandWrapper.LocationIds();
      if (index == 1) return CommandWrapper.Info("Seed for the random output. 0 = random, all = enable all objects.");
      if (index == 2) return CommandWrapper.Info("Any value forces random damage on structures (disabled by default).");
      return null;
    });
    Helper.Command("hammer_location", "[location id] [seed=0] [random damage] - Selects the location to be placed.", (args) =>
    {
      Helper.CheatCheck();
      Helper.ArgsCheck(args, 2, "Missing the location id.");
      Hammer.Equip(Tool.Hammer);
      try
      {
        Hammer.AllLocationsObjects = args.Length > 2 && args[2] == "all";
        Hammer.RandomLocationDamage = args.Length > 3;
        var rng = new System.Random();
        var seed = args.TryParameterInt(2, rng.Next());
        if (seed == 0) seed = rng.Next();
        var location = ZoneSystem.instance.GetLocation(args[1].GetStableHashCode());
        var selected = Selection.Set(location, seed);

        PrintSelected(args.Context, selected);
      }
      catch (InvalidOperationException e)
      {
        Helper.AddMessage(args.Context, e.Message);
      }
    }, CommandWrapper.LocationIds);
  }
}

using System;
using ServerDevcommands;
using UnityEngine;

namespace InfinityHammer;

public class HammerLocationCommand
{
    private static void PrintSelected(Terminal terminal, GameObject obj)
    {
        if (Configuration.DisableSelectMessages) return;
        var name = obj ? Utils.GetPrefabName(obj.gameObject) : "";
        HammerHelper.Message(terminal, $"Selected {name}.");
    }

    public HammerLocationCommand()
    {
        AutoComplete.Register("hammer_location", (int index, int subIndex) =>
        {
            if (index == 0) return ParameterInfo.LocationIds;
            if (index == 1)
                return ParameterInfo.Create(
                    "Seed for the random output. 0 = random, all = enable all objects.");
            if (index == 2)
                return ParameterInfo.Create(
                    "Any value forces random damage on structures (disabled by default).");
            return ParameterInfo.None;
        });
        Helper.Command("hammer_location",
            "[location id] [seed=0] [random damage] - Selects the location to be placed.",
            (args) =>
            {
                HammerHelper.CheatCheck();
                Helper.ArgsCheck(args, 2, "Missing the location id.");
                Hammer.Equip();
                try
                {
                    Hammer.AllLocationsObjects =
                        args.Length > 2 && args[2] == "all";
                    Hammer.RandomLocationDamage = args.Length > 3;
                    var rng = new System.Random();
                    var seed = args.TryParameterInt(2, rng.Next());
                    if (seed == 0) seed = rng.Next();
                    var location =
                        ZoneSystem.instance.GetLocation(args[1]
                            .GetStableHashCode());
                    var ghost =
                        Selection.CreateGhost(
                            new LocationSelection(location, seed));
                    ghost.name = args[1];
                    PrintSelected(args.Context, ghost);
                }
                catch (InvalidOperationException e)
                {
                    HammerHelper.Message(args.Context, e.Message);
                }
            });
    }
}
using System;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace InfinityHammer;

public class HammerRoomCommand
{
  private static void PrintSelected(Terminal terminal, GameObject obj)
  {
    if (Configuration.DisableSelectMessages) return;
    var name = obj ? Utils.GetPrefabName(obj.gameObject) : "";
    HammerHelper.Message(terminal, $"Selected {name}.");
  }

  public HammerRoomCommand()
  {
    AutoComplete.Register("hammer_room", (int index, int subIndex) =>
    {
      if (index == 0) return ParameterInfo.RoomIds;
      if (index == 1) return ["true", "false"];
      return ParameterInfo.None;
    });
    Helper.Command("hammer_room", "[room id] [empty_room] - Selects the room to be placed.", (args) =>
    {
      HammerHelper.CheatCheck();
      Helper.ArgsCheck(args, 2, "Missing the room id.");
      HammerHelper.Init();
      var placeEmptyRoom = args.Length > 2 ? Parse.Boolean(args[2]) ?? Configuration.PlaceEmptyRooms : Configuration.PlaceEmptyRooms;
      try
      {
        var ghost = Selection.CreateGhost(new RoomSelection(args[1].GetStableHashCode(), placeEmptyRoom));
        Hammer.SelectEmpty();
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

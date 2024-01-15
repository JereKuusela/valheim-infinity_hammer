using ServerDevcommands;
using UnityEngine;

namespace InfinityHammer;
public class HammerRepairCommand
{
  public HammerRepairCommand()
  {
    AutoComplete.RegisterEmpty("hammer_repair");
    Helper.Command("hammer_repair", "Selects the repair tool.", (args) =>
    {
      HammerHelper.Init();
      var player = Helper.GetPlayer();
      player.SetSelectedPiece(new Vector2Int(0, 0));
      player.SetupPlacementGhost();
    });
  }
}

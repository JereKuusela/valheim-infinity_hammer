using ServerDevcommands;

namespace InfinityHammer;
public class HammerRepairCommand
{
  public HammerRepairCommand()
  {
    AutoComplete.RegisterEmpty("hammer_repair");
    Helper.Command("hammer_repair", "Selects the repair tool.", (args) =>
    {
      HammerHelper.Init();
      Helper.GetPlayer().SetupPlacementGhost();
    });
  }
}

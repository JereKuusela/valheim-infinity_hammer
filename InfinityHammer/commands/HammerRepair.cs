namespace InfinityHammer;
public class HammerRepairCommand {
  public HammerRepairCommand() {
    CommandWrapper.RegisterEmpty("hammer_repair");
    Helper.Command("hammer_repair", "Selects the repair tool.", (args) => {
      Helper.EnabledCheck();
      Hammer.Clear();
    });
  }
}

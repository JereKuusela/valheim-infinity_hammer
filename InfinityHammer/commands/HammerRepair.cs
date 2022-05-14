namespace InfinityHammer;
public class HammerRepairCommand {
  public HammerRepairCommand() {
    CommandWrapper.RegisterEmpty("hammer_repair");
    new Terminal.ConsoleCommand("hammer_repair", "Selects the repair tool.", (args) => {
      if (!Player.m_localPlayer) return;
      if (!Settings.Enabled) return;
      Hammer.Clear();
    });
  }
}

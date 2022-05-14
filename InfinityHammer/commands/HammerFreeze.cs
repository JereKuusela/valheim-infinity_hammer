namespace InfinityHammer;
public class HammerFreezeCommand {
  public HammerFreezeCommand() {
    CommandWrapper.RegisterEmpty("hammer_freeze");
    new Terminal.ConsoleCommand("hammer_freeze", "Toggles whether the mouse affects placement position.", (args) => {
      Position.ToggleFreeze();
    });
  }
}

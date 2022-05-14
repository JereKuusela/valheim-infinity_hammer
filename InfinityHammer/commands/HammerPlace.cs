namespace InfinityHammer;
public class HammerPlaceCommand {
  public HammerPlaceCommand() {
    CommandWrapper.RegisterEmpty("hammer_place");
    new Terminal.ConsoleCommand("hammer_place", "Places the current object with a command.", (args) => {
      Hammer.Place();
    });
  }
}

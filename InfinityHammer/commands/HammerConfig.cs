using System.Linq;
namespace InfinityHammer;
public class HammerConfigCommand {
  public HammerConfigCommand() {
    CommandWrapper.Register("hammer_config", (int index) => {
      if (index == 0) return Settings.Options;
      if (index == 1) return CommandWrapper.Info("Value.");
      return null;
    });
    new Terminal.ConsoleCommand("hammer_config", "[key] [value] - Toggles or sets config value.", (Terminal.ConsoleEventArgs args) => {
      if (args.Length < 2) return;
      if (args.Length == 2)
        Settings.UpdateValue(args.Context, args[1], "");
      else
        Settings.UpdateValue(args.Context, args[1], string.Join(" ", args.Args.Skip(2)));
    }, optionsFetcher: () => Settings.Options);
  }
}

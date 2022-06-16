using System.Linq;
namespace InfinityHammer;
public class HammerCommandCommand {
  public HammerCommandCommand() {
    Helper.Command("hammer_command", "[command] - Executes command at a targeted position.", (args) => {
      Helper.ArgsCheck(args, 2, "Missing the command.");
      HammerCommandParameters pars = new HammerCommandParameters(args);
      Ruler.Create(pars.ToRuler());
      var command = string.Join(" ", args.Args.Skip(1));
      Selection.Set(command);
      Helper.AddMessage(args.Context, $"Selected command {command}.");
    });
    CommandWrapper.Register("hammer_command", (int index, int subIndex) => {
      return null;
    });
  }
}

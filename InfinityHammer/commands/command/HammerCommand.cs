using System.Linq;
using UnityEngine;
namespace InfinityHammer;
public class HammerCommandCommand {
  public HammerCommandCommand() {
    Helper.Command("hammer_command", "[command] - Executes command at a targeted position.", (args) => {
      Helper.ArgsCheck(args, 2, "Missing the command.");
      var original = string.Join(" ", args.Args.Skip(1));
      HammerCommandParameters pars = new HammerCommandParameters(args);
      var command = string.Join(" ", args.Args.Skip(1));
      Selection.Set(command, original);
      GizmoWrapper.SetRotation(Quaternion.identity);
      Ruler.Create(pars.ToRuler());
      Helper.AddMessage(args.Context, $"Selected command {original}.");
    });
    CommandWrapper.Register("hammer_command", (int index, int subIndex) => {
      return null;
    });
  }
}

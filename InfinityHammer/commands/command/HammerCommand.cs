using UnityEngine;
namespace InfinityHammer;
public class HammerCommandCommand {
  public HammerCommandCommand() {
    Helper.Command("hammer_command", "[command] - Executes command at a targeted position.", (args) => {
      Helper.ArgsCheck(args, 2, "Missing the command.");
      HammerCommandParameters pars = new HammerCommandParameters(args);
      Selection.Set(pars.Name, pars.Description, HammerCommandParameters.Join(args.Args));
      GizmoWrapper.SetRotation(Quaternion.identity);
      Ruler.Create(pars.ToRuler());
      Helper.AddMessage(args.Context, $"Selected command {pars.Name}.");
    });
    CommandWrapper.Register("hammer_command", (int index, int subIndex) => {
      return null;
    });
  }
}

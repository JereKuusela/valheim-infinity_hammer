using System;
using ServerDevcommands;
namespace InfinityHammer;
public class HammerScaleCommand
{
  private static void Scale(string amountStr, string direction, Action<float> action)
  {
    var amount = Parse.Direction(direction) * Parse.Float(amountStr, 1f);
    action(amount);
  }
  private static void CommandAxis(string name, string axis, Func<ScalingData, Action<float>> action)
  {
    name = $"{name}_{axis}";
    AutoComplete.Register(name, (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Amount.");
      return null;
    });
    Helper.Command(name, $"[amount] - Sets the scale of {axis} axis (if the object supports it).", (args) =>
    {
      HammerHelper.CheatCheck();
      Helper.ArgsCheck(args, 2, "Missing the amount.");
      var selection = Selection.Get();
      if (!Helper.GetPlayer().InPlaceMode()) return;
      if (!selection.IsScalingSupported())
      {
        HammerHelper.Message(args.Context, "Selected object doesn't support scaling.");
        return;
      }
      var direction = args.Length > 2 ? args[2] : "";
      Scale(args[1], direction, action(Scaling.Get()));
      Scaling.Print(args.Context);
    });
  }
  private static void Command(string name)
  {
    AutoComplete.Register(name, (int index, int subIndex) =>
    {
      if (index == 0) return ParameterInfo.XZY("Amount of scale.", subIndex);
      return null;
    });
    Helper.Command(name, "[amount or x,z,y] - Sets the scale (if the object supports it).", (args) =>
    {
      HammerHelper.CheatCheck();
      var selection = Selection.Get();
      if (!Helper.GetPlayer().InPlaceMode()) return;
      if (!selection.IsScalingSupported())
      {
        HammerHelper.Message(args.Context, "Selected object doesn't support scaling.");
        return;
      }
      selection.SetScale(Parse.Scale(Parse.Split(args[1])) * Parse.Direction(args.Args, 2));
      Scaling.Print(args.Context);
    });
  }
  public HammerScaleCommand()
  {
    var name = "hammer_scale";
    CommandAxis(name, "x", (scale) => scale.SetScaleX);
    CommandAxis(name, "y", (scale) => scale.SetScaleY);
    CommandAxis(name, "z", (scale) => scale.SetScaleZ);
    Command(name);
  }
}

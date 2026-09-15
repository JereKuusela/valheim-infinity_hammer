using System;
using ServerDevcommands;
namespace InfinityHammer;

public class HammerZoomCommand
{
  private static void Zoom(string amountStr, string direction, Action<float> amountAction, Action<float> percentageAction)
  {
    if (amountStr.EndsWith("%", StringComparison.Ordinal))
    {
      var amount = Parse.Direction(direction) * Parse.Float(amountStr.Substring(0, amountStr.Length - 1), 5f) / 100f;
      percentageAction(amount);
    }
    else
    {
      var amount = Parse.Direction(direction) * Parse.Float(amountStr, 1f);
      amountAction(amount);
    }
  }
  private static void CommandAxis(string name, string axis, Func<ScalingData, Action<float>> amountAction, Func<ScalingData, Action<float>> percentageAction)
  {
    name = $"{name}_{axis}";
    AutoComplete.Register(name, (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Flat amount or percentage to scale.");
      return ParameterInfo.None;
    });
    Helper.Command(name, $"[amount or percentage] - Zooms the {axis} axis (if the object supports it).", (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing the amount.");
      var selection = Selection.Get();
      if (!Helper.GetPlayer().InPlaceMode()) return;
      if (!selection.IsScalingSupported())
      {
        HammerHelper.Message(args.Context, "Selected object doesn't support scaling.");
        return;
      }
      var direction = args.Length > 2 ? args[2] : "";
      var scale = Scaling.Get();
      Zoom(args[1], direction, amountAction(scale), percentageAction(scale));
      selection.SetScale(scale.Vec3);
      Scaling.Print(args.Context);
    });
  }
  private static void Command(string name)
  {
    AutoComplete.Register(name, (int index, int subIndex) =>
    {
      if (index == 0) return ParameterInfo.Create("Flat amount or percentage to scale.");
      return ParameterInfo.None;
    });
    Helper.Command(name, "[amount/percentage or x,z,y] - Zooms the selection (if the object supports it).", (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing the amount.");
      var selection = Selection.Get();
      if (!Helper.GetPlayer().InPlaceMode()) return;
      if (!selection.IsScalingSupported())
      {
        HammerHelper.Message(args.Context, "Selected object doesn't support scaling.");
        return;
      }
      var scale = Scaling.Get();
      var split = args[1].Split(',');
      var direction = args.Length > 2 ? args[2] : "";
      if (split.Length == 1)
        Zoom(split[0], direction, scale.Zoom, scale.ZoomPercentage);
      else if (split.Length == 3)
      {
        Zoom(split[0], direction, scale.ZoomX, scale.ZoomXPercentage);
        Zoom(split[1], direction, scale.ZoomZ, scale.ZoomZPercentage);
        Zoom(split[2], direction, scale.ZoomY, scale.ZoomYPercentage);
      }
      else
        throw new InvalidOperationException("Must either have 1 or 3 values.");
      selection.SetScale(scale.Vec3);
      Scaling.Print(args.Context);

    });
  }
  public HammerZoomCommand()
  {
    var name = "hammer_zoom";
    CommandAxis(name, "x", (scale) => scale.ZoomX, (scale) => scale.ZoomXPercentage);
    CommandAxis(name, "y", (scale) => scale.ZoomY, (scale) => scale.ZoomYPercentage);
    CommandAxis(name, "z", (scale) => scale.ZoomZ, (scale) => scale.ZoomZPercentage);
    Command(name);
  }
}

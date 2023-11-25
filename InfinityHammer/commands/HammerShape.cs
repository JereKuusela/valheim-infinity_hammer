using System.Collections.Generic;
namespace InfinityHammer;
public class HammerShapeCommand
{
  public HammerShapeCommand()
  {
    List<string> shapes = [
      RulerShape.Circle.ToString(),
      RulerShape.Rectangle.ToString(),
      RulerShape.Square.ToString(),
    ];
    CommandWrapper.Register("hammer_shape", (int index, int subIndex) => index == 0 ? shapes : null);
    Helper.Command("hammer_shape", "[shape] - Toggles or sets the selection shape.", (args) =>
    {
      if (!Selection.IsTool()) return;
      var tool = Selection.Tool();
      if (args.Length > 1)
      {
        var arg = args[1].ToLower();
        if (arg == RulerShape.Circle.ToString().ToLower()) tool.Shape = RulerShape.Circle;
        else if (arg == RulerShape.Ring.ToString().ToLower()) tool.Shape = RulerShape.Ring;
        else if (arg == RulerShape.Rectangle.ToString().ToLower()) tool.Shape = RulerShape.Rectangle;
        else if (arg == RulerShape.Frame.ToString().ToLower()) tool.Shape = RulerShape.Frame;
        else if (arg == RulerShape.Square.ToString().ToLower()) tool.Shape = RulerShape.Square;
        else return;
      }
      else
      {
        if (tool.Shape == RulerShape.Circle) tool.Shape = RulerShape.Ring;
        else if (tool.Shape == RulerShape.Ring) tool.Shape = RulerShape.Square;
        else if (tool.Shape == RulerShape.Square) tool.Shape = RulerShape.Frame;
        else if (tool.Shape == RulerShape.Frame) tool.Shape = RulerShape.Rectangle;
        else if (tool.Shape == RulerShape.Rectangle) tool.Shape = RulerShape.Circle;

        Ruler.SanityCheckShape();
      }
      Helper.AddMessage(args.Context, $"Selection shape set to {tool.Shape}.");
    });
  }
}

using System.Collections.Generic;
using InfinityHammer;
using ServerDevcommands;
namespace InfinityTools;
public class ToolShapeCommand
{
  public ToolShapeCommand()
  {
    List<string> shapes = [
      RulerShape.Circle.ToString(),
      RulerShape.Rectangle.ToString(),
      RulerShape.Square.ToString(),
    ];
    AutoComplete.Register("tool_shape", (int index, int subIndex) => index == 0 ? shapes : null);
    Helper.Command("tool_shape", "[shape] - Toggles or sets the selection shape.", (args) =>
    {
      if (Selection.Get() is not ToolSelection selection) return;
      if (args.Length > 1)
      {
        var arg = args[1].ToLower();
        if (arg == RulerShape.Circle.ToString().ToLower()) Ruler.Shape = RulerShape.Circle;
        else if (arg == RulerShape.Ring.ToString().ToLower()) Ruler.Shape = RulerShape.Ring;
        else if (arg == RulerShape.Rectangle.ToString().ToLower()) Ruler.Shape = RulerShape.Rectangle;
        else if (arg == RulerShape.Frame.ToString().ToLower()) Ruler.Shape = RulerShape.Frame;
        else if (arg == RulerShape.Square.ToString().ToLower()) Ruler.Shape = RulerShape.Square;
        else return;
      }
      else
      {
        if (Ruler.Shape == RulerShape.Circle) Ruler.Shape = RulerShape.Ring;
        else if (Ruler.Shape == RulerShape.Ring) Ruler.Shape = RulerShape.Square;
        else if (Ruler.Shape == RulerShape.Square) Ruler.Shape = RulerShape.Frame;
        else if (Ruler.Shape == RulerShape.Frame) Ruler.Shape = RulerShape.Rectangle;
        else if (Ruler.Shape == RulerShape.Rectangle) Ruler.Shape = RulerShape.Circle;

        Ruler.SanityCheckShape();
      }
      HammerHelper.Message(args.Context, $"Selection shape set to {Ruler.Shape}.");
    });
  }
}

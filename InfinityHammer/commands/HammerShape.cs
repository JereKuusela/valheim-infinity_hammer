using System.Collections.Generic;
namespace InfinityHammer;
public class HammerShapeCommand
{
  public HammerShapeCommand()
  {
    List<string> shapes = new() {
      RulerShape.Circle.ToString(),
      RulerShape.Rectangle.ToString(),
      RulerShape.Square.ToString(),
    };
    CommandWrapper.Register("hammer_shape", (int index, int subIndex) =>
    {
      if (index == 0) return shapes;
      return null;
    });
    Helper.Command("hammer_shape", "[shape] - Toggles or sets the selection shape.", (args) =>
    {
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
        var projector = Ruler.Projector;
        if (projector == null) return;
        if (Ruler.Shape == RulerShape.Circle) Ruler.Shape = RulerShape.Ring;
        else if (Ruler.Shape == RulerShape.Ring) Ruler.Shape = RulerShape.Square;
        else if (Ruler.Shape == RulerShape.Square) Ruler.Shape = RulerShape.Frame;
        else if (Ruler.Shape == RulerShape.Frame) Ruler.Shape = RulerShape.Rectangle;
        else if (Ruler.Shape == RulerShape.Rectangle) Ruler.Shape = RulerShape.Circle;

        Ruler.SanityCheckShape();
      }
      Helper.AddMessage(args.Context, $"Selection shape set to {Ruler.Shape}.");
    });
  }
}

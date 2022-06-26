using System.Collections.Generic;
namespace InfinityHammer;
public class HammerShapeCommand {
  public HammerShapeCommand() {
    List<string> shapes = new() {
      RulerShape.Circle.ToString(),
      RulerShape.Rectangle.ToString(),
      RulerShape.Square.ToString(),
    };
    CommandWrapper.Register("hammer_shape", (int index, int subIndex) => {
      if (index == 0) return shapes;
      return null;
    });
    Helper.Command("hammer_shape", "[shape] - Toggles or sets the selection shape.", (args) => {
      if (args.Length > 1) {
        var arg = args[1].ToLower();
        if (arg == RulerShape.Circle.ToString().ToLower()) Ruler.Shape = RulerShape.Circle;
        else if (arg == RulerShape.Rectangle.ToString().ToLower()) Ruler.Shape = RulerShape.Rectangle;
        else if (arg == RulerShape.Square.ToString().ToLower()) Ruler.Shape = RulerShape.Square;
        else return;
      } else {
        var projector = Ruler.Projector;
        if (projector == null) return;
        var circle = Ruler.Circle;
        var square = Ruler.Square;
        var rect = Ruler.Rectangle;
        if (Ruler.Shape == RulerShape.Circle && square) Ruler.Shape = RulerShape.Square;
        else if (Ruler.Shape == RulerShape.Circle && rect) Ruler.Shape = RulerShape.Rectangle;
        else if (Ruler.Shape == RulerShape.Square && rect) Ruler.Shape = RulerShape.Rectangle;
        else if (Ruler.Shape == RulerShape.Square && circle) Ruler.Shape = RulerShape.Circle;
        else if (Ruler.Shape == RulerShape.Rectangle && circle) Ruler.Shape = RulerShape.Circle;
        else if (Ruler.Shape == RulerShape.Rectangle && square) Ruler.Shape = RulerShape.Square;
        else return;
      }
      Helper.AddMessage(args.Context, $"Selection shape set to {Ruler.Shape}.");
    });
  }
}

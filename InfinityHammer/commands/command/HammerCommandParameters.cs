using Service;
namespace InfinityHammer;
public class HammerCommandParameters {
  public float? Radius = null;
  public float? Width = null;
  public float? Depth = null;

  public HammerCommandParameters(Terminal.ConsoleEventArgs args) {
    ParseArgs(args.Args);
  }

  public RulerParameters ToRuler() => new() {
    Radius = Radius,
    Width = Width,
    Depth = Depth
  };

  protected void ParseArgs(string[] args) {
    var radius = Scaling.Value.x;
    var diameter = 2f * radius;
    var width = 2f * Scaling.Value.x;
    var depth = 2f * Scaling.Value.z;
    for (var i = 0; i < args.Length; i++) {
      var arg = args[i];
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (split.Length < 2) continue;
      var value = split[1].ToLower();
      var values = Parse.Split(value);
      if (name == "radius") {
        Radius = Parse.TryFloat(value, radius);
        args[i] = $"{name}=#radius";
      }
      if (name == "diameter" || name == "circle") {
        Radius = Parse.TryFloat(value, diameter) / 2f;
        args[i] = $"{name}=#diameter";
      }
      if (name == "rect") {
        if (values.Length == 1) {
          Width = Parse.TryFloat(value, width);
          Depth = Width;
        } else {
          Width = Parse.TryFloat(values, 0, width);
          Depth = Parse.TryFloat(values, 1, depth);
        }
        args[i] = $"{name}=#width,#depth";
      }
    }
  }
}

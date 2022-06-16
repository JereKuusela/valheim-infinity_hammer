using Service;
namespace InfinityHammer;
public class HammerCommandParameters {
  public float? Diameter = null;
  public float? Width = null;
  public float? Depth = null;

  public HammerCommandParameters(Terminal.ConsoleEventArgs args) {
    ParseArgs(args.Args);
  }

  public RulerParameters ToRuler() => new() {
    Diameter = Diameter,
    Width = Width,
    Depth = Depth
  };

  protected void ParseArgs(string[] args) {
    foreach (var arg in args) {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (split.Length < 2) continue;
      var value = split[1].ToLower();
      var values = Parse.Split(value);
      if (name == "radius")
        Diameter = Parse.TryFloat(value, 0f) * 2f;
      if (name == "diameter" || name == "circle")
        Diameter = Parse.TryFloat(value, 0f);
      if (name == "rect") {
        var size = Parse.TryScale(values);
        Width = size.x;
        Depth = size.z;
      }
    }
  }
}

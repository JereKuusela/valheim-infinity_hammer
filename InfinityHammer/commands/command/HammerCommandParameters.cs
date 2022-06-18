using System;
using Service;
namespace InfinityHammer;
public class HammerCommandParameters {
  public float? Radius = null;
  public float? Width = null;
  public float? Depth = null;
  public bool Angle = false;

  public HammerCommandParameters(Terminal.ConsoleEventArgs args) {
    ParseArgs(args.Args);
  }

  public RulerParameters ToRuler() => new() {
    Radius = Radius,
    Width = Width,
    Depth = Depth,
    RotateWithPlayer = !Angle
  };

  static bool IsParameter(string arg, string par) => arg == par || arg.EndsWith("=" + par, StringComparison.OrdinalIgnoreCase);
  static string ReplaceEnd(string arg, string par, int amount) => arg.Substring(0, arg.Length - amount) + par;

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
    var x = "#x";
    var y = "#y";
    var z = "#z";
    for (var i = 0; i < args.Length; i++) {
      var arg = args[i];
      if (IsParameter(arg, "a")) {
        Angle = true;
        args[i] = ReplaceEnd(arg, "#angle", 1);
      }
      if (IsParameter(arg, "x"))
        args[i] = ReplaceEnd(arg, x, 1);
      if (IsParameter(arg, "y"))
        args[i] = ReplaceEnd(arg, y, 1);
      if (IsParameter(arg, "z"))
        args[i] = ReplaceEnd(arg, z, 1);
      if (IsParameter(arg, "x,y"))
        args[i] = ReplaceEnd(arg, $"{x},{y}", 3);
      if (IsParameter(arg, "x,z"))
        args[i] = ReplaceEnd(arg, $"{x},{z}", 3);
      if (IsParameter(arg, "y,x"))
        args[i] = ReplaceEnd(arg, $"{y},{x}", 3);
      if (IsParameter(arg, "y,z"))
        args[i] = ReplaceEnd(arg, $"{y},{z}", 3);
      if (IsParameter(arg, "z,x"))
        args[i] = ReplaceEnd(arg, $"{z},{x}", 3);
      if (IsParameter(arg, "z,y"))
        args[i] = ReplaceEnd(arg, $"{z},{y}", 3);
      if (IsParameter(arg, "x,y,z"))
        args[i] = ReplaceEnd(arg, $"{x},{y},{z}", 5);
      if (IsParameter(arg, "x,z,y"))
        args[i] = ReplaceEnd(arg, $"{x},{z},{y}", 5);
      if (IsParameter(arg, "y,x,z"))
        args[i] = ReplaceEnd(arg, $"{y},{x},{z}", 5);
      if (IsParameter(arg, "y,z,x"))
        args[i] = ReplaceEnd(arg, $"{y},{z},{x}", 5);
      if (IsParameter(arg, "z,x,y"))
        args[i] = ReplaceEnd(arg, $"{z},{x},{y}", 5);
      if (IsParameter(arg, "z,y,x"))
        args[i] = ReplaceEnd(arg, $"{z},{y},{x}", 5);

    }
  }
}

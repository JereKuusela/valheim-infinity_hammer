using Service;
using UnityEngine;
namespace InfinityHammer;
public class HammerBlueprintParameters {
  public Vector3? Scale;

  public HammerBlueprintParameters(Terminal.ConsoleEventArgs args) {
    ParseArgs(args.Args);
  }

  protected void ParseArgs(string[] args) {
    foreach (var arg in args) {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (split.Length < 2) continue;
      var value = split[1].ToLower();
      var values = Parse.Split(value);
      if (name == "scale")
        Scale = Parse.TryScale(values);
    }
  }
}

using Service;
using UnityEngine;
namespace InfinityHammer;
public class HammerParameters {
  public Vector3 Position = Vector3.zero;
  public Vector3? Scale;
  public float? Radius;
  public string? Text;
  public int? Level;
  public float? Health;
  public string? Connect;

  public HammerParameters(Terminal.ConsoleEventArgs args) {
    if (Player.m_localPlayer)
      Position = Player.m_localPlayer.transform.position;
    ParseArgs(args.Args);
  }

  protected void ParseArgs(string[] args) {
    foreach (var arg in args) {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (split.Length < 2) continue;
      var value = split[1].ToLower();
      var values = Parse.Split(value);
      if (name == "connect")
        Connect = value;
      if (name == "level")
        Level = Parse.TryInt(value, 1);
      if (name == "stars")
        Level = Parse.TryInt(value, 0) + 1;
      if (name == "text")
        Text = value.Replace("_", " ");
      if (name == "health")
        Health = Parse.TryFloat(value, 0f);
      if (name == "radius")
        Radius = Parse.TryFloat(value, 0f);
      if (name == "from")
        Position = Parse.TryVectorXZY(values, Position);
      if (name == "scale")
        Scale = Parse.TryScale(values);
    }
  }
}

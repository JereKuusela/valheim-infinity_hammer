using System;
using Service;
using UnityEngine;
namespace InfinityHammer;
public enum Growth {
  Default,
  HealthyGrown,
  UnhealthyGrown,
  Healthy,
  Unhealthy
}
public enum Wear {
  Default,
  Broken,
  Damaged,
  Healthy
}
public enum Fall {
  Default,
  Off,
  Terrain,
  Solid
}
public class HammerParameters {
  public Vector3 Position = Vector3.zero;
  public Vector3? Scale;
  public string? Text;
  public int? Level;
  public float? Health;
  public float Angle = 0f;
  public bool Connect;
  public float? Radius;
  public float? Width;
  public float? Depth;
  public bool Show = true;
  public bool Collision = true;
  public bool Interact = true;
  public bool Restrict = true;
  public float Height = 0f;
  public ObjectType ObjectType = ObjectType.All;
  public Wear Wear = Wear.Default;
  public Growth Growth = Growth.Default;
  public Fall Fall = Fall.Default;

  public HammerParameters(Terminal.ConsoleEventArgs args) {
    if (Player.m_localPlayer)
      Position = Player.m_localPlayer.transform.position;
    ParseArgs(args.Args);
  }

  protected void ParseArgs(string[] args) {
    foreach (var arg in args) {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (name == "connect")
        Connect = true;
      if (split.Length < 2) continue;
      var value = split[1].ToLower();
      var values = Parse.Split(value);
      if (name == "level")
        Level = Parse.TryInt(value, 1);
      if (name == "stars")
        Level = Parse.TryInt(value, 0) + 1;
      if (name == "text")
        Text = value.Replace("_", " ");
      if (name == "health")
        Health = Parse.TryFloat(value, 0f);
      if (name == "from")
        Position = Parse.TryVectorXZY(values, Position);
      if (name == "scale")
        Scale = Parse.TryScale(values);
      if (name == "circle")
        Radius = Parse.TryFloat(value, 0f);
      if (name == "rect") {
        var size = Parse.TryScale(values);
        Width = size.x;
        Depth = size.z;
      }
      if (name == "height")
        Height = Parse.TryFloat(value, 0f);
      if (name == "angle")
        Angle = Parse.TryFloat(value, 0f) * Mathf.PI / 180f;
      if (name == "type" && value == "creature") ObjectType = ObjectType.Character;
      if (name == "type" && value == "structure") ObjectType = ObjectType.Structure;
      if (name == "wear" && value == "broken") Wear = Wear.Broken;
      if (name == "wear" && value == "damaged") Wear = Wear.Damaged;
      if (name == "wear" && value == "healthy") Wear = Wear.Healthy;
      if (name == "growth" && value == "big") Growth = Growth.HealthyGrown;
      if (name == "growth" && value == "big_bad") Growth = Growth.UnhealthyGrown;
      if (name == "growth" && value == "small") Growth = Growth.Healthy;
      if (name == "growth" && value == "small_bad") Growth = Growth.Unhealthy;
      if (name == "fall" && value == "solid") Fall = Fall.Solid;
      if (name == "fall" && value == "terrain") Fall = Fall.Terrain;
      if (name == "fall" && value == "off") Fall = Fall.Off;
      if (name == "show") Show = Parse.Boolean(value) ?? true;
      if (name == "collision") Collision = Parse.Boolean(value) ?? true;
      if (name == "interact") Interact = Parse.Boolean(value) ?? true;
      if (name == "restrict") Restrict = Parse.Boolean(value) ?? true;
    }
    if (Radius.HasValue && Depth.HasValue)
      throw new InvalidOperationException($"<color=yellow>circle</color> and <color=yellow>rect</color> parameters can't be used together.");
  }
}

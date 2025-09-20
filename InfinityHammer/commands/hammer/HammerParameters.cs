using System;
using System.Collections.Generic;
using ServerDevcommands;
using Service;
using UnityEngine;
namespace InfinityHammer;

public enum Growth
{
  Default,
  HealthyGrown,
  UnhealthyGrown,
  Healthy,
  Unhealthy
}
public enum Wear
{
  Default,
  Broken,
  Damaged,
  Healthy
}
public enum Fall
{
  Default,
  Off,
  Terrain,
  Solid
}
public class HammerParameters
{
  public Vector3 Position = Vector3.zero;
  public Vector3? Scale;
  public string? Text;
  public int? Level;
  public float? Health;
  public float Angle = 0f;
  public bool Connect;
  public bool Freeze;
  public bool Pick;
  public string? Data;
  public Range<float>? Radius;
  public Range<float>? Width;
  public Range<float>? Depth;
  public bool Show = true;
  public bool Collision = true;
  public bool Interact = true;
  public bool Restrict = true;
  public float Height = 0f;
  public HashSet<string> Components = [];
  public Wear Wear = Wear.Default;
  public Growth Growth = Growth.Default;
  public Fall Fall = Fall.Default;
  public string[] Ignored = [];
  public string[] Included = [];
  public float? Terrain;

  public HammerParameters(Terminal.ConsoleEventArgs args)
  {
    if (Player.m_localPlayer)
      Position = Player.m_localPlayer.transform.position;
    ParseArgs(args.Args);
  }
  protected void ParseArgs(string[] args)
  {
    foreach (var arg in args)
    {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (name == "connect")
        Connect = true;
      if (name == "freeze")
        Freeze = true;
      if (name == "pick")
        Pick = true;
      if (name == "terrain")
        Terrain = 0f;
      if (split.Length < 2) continue;
      var value = split[1].ToLower();
      var values = Parse.Split(value);
      if (name == "data")
        Data = split[1];
      if (name == "connect")
        Connect = HammerHelper.IsDown(value);
      if (name == "freeze")
        Freeze = HammerHelper.IsDown(value);
      if (name == "pick")
        Pick = HammerHelper.IsDown(value);
      if (name == "level")
        Level = Parse.Int(value, 1);
      if (name == "stars")
        Level = Parse.Int(value, 0) + 1;
      if (name == "text")
        Text = split[1].Replace("_", " ");
      if (name == "health")
        Health = Parse.Float(value);
      if (name == "from")
        Position = Parse.VectorXZY(values, Position);
      if (name == "scale")
        Scale = Parse.Scale(values);
      if (name == "circle")
        Radius = Parse.FloatRange(value);
      if (name == "rect")
      {
        var size = Parse.ScaleRange(value);
        Width = new(size.Min.x, size.Max.x);
        Depth = new(size.Min.z, size.Max.z);
      }
      if (name == "height")
        Height = Parse.Float(value);
      if (name == "angle")
        Angle = Parse.Float(value) * Mathf.PI / 180f;
      if (name == "type") AddComponents(values);
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
      if (name == "include") Included = values;
      if (name == "id") Included = values;
      if (name == "ignore") Ignored = values;
      if (name == "terrain") Terrain = Parse.Float(value);
    }
    if (Ignored.Length == 0)
      Ignored = Configuration.IgnoredIds;
    else if (Configuration.IgnoredIds.Length > 0)
      Ignored = [.. Ignored, .. Configuration.IgnoredIds];
    if (Radius != null && Depth != null)
      throw new InvalidOperationException($"<color=yellow>circle</color> and <color=yellow>rect</color> parameters can't be used together.");
  }
  private void AddComponents(string[] values)
  {
    foreach (var value in values) Components.Add(value);
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Service;
using UnityEngine;
namespace InfinityHammer;
public class CommandParameters {
  public static string CmdName = "cmd_name";
  public static string CmdDesc = "cmd_desc";
  public static string CmdIcon = "cmd_icon";
  public static string CmdR = "cmd_r";
  public static string CmdW = "cmd_w";
  public static string CmdD = "cmd_d";
  public static string CmdH = "cmd_h";
  public Range<float> RadiusCap = new(float.MinValue, float.MaxValue);
  public Range<float> WidthCap = new(float.MinValue, float.MaxValue);
  public Range<float> DepthCap = new(float.MinValue, float.MaxValue);
  public Range<float> HeightCap = new(float.MinValue, float.MaxValue);
  public float? Radius = null;
  public float? Width = null;
  public float? Depth = null;
  public float? Height = null;
  public bool Angle = false;
  public bool IsTargeted = false;
  public string Name = "Command";
  public string Description = "";
  public Sprite? Icon = null;
  public string Command = "";

  public static string Join(string command) => string.Join(";", command.Split(';').Select(s => s.Trim()).Select(s => string.Join(" ", s.Split(' ')
    .Where(s => !s.StartsWith($"{CmdName}=", StringComparison.OrdinalIgnoreCase))
    .Where(s => !s.StartsWith($"{CmdDesc}=", StringComparison.OrdinalIgnoreCase))
    .Where(s => !s.StartsWith($"{CmdR}=", StringComparison.OrdinalIgnoreCase))
    .Where(s => !s.StartsWith($"{CmdW}=", StringComparison.OrdinalIgnoreCase))
    .Where(s => !s.StartsWith($"{CmdD}=", StringComparison.OrdinalIgnoreCase))
    .Where(s => !s.StartsWith($"{CmdH}=", StringComparison.OrdinalIgnoreCase))
  )));
  public CommandParameters(string command, bool showCommand) {
    var split = command.Split(';').Select(s => s.Trim()).ToArray();
    Command = string.Join(";", split.Select(ParseArgs));
    if (Radius.HasValue)
      Radius = Mathf.Clamp(Radius.Value, RadiusCap.Min, RadiusCap.Max);
    if (Height.HasValue)
      Height = Mathf.Clamp(Height.Value, HeightCap.Min, HeightCap.Max);
    if (Width.HasValue)
      Width = Mathf.Clamp(Width.Value, WidthCap.Min, WidthCap.Max);
    if (Depth.HasValue)
      Depth = Mathf.Clamp(Depth.Value, DepthCap.Min, DepthCap.Max);
    if (showCommand || Description == "") {
      if (Description != "") Description += "\n";
      Description += Join(command);
    }
  }


  public RulerParameters ToRuler() => new() {
    Radius = Radius,
    Width = Width,
    Depth = Depth,
    RotateWithPlayer = !Angle,
    IsTargeted = IsTargeted,
    Height = Height
  };

  static bool IsParameter(string arg, string par) => arg == par || arg.EndsWith("=" + par, StringComparison.OrdinalIgnoreCase);
  static string ReplaceEnd(string arg, string par, int amount) => arg.Substring(0, arg.Length - amount) + par;

  private string Replace(string arg, string par) {
    while (IsParameter(arg, par)) {
      var str = string.Join(",", par.Split(',').Select(s => $"#{s}"));
      return ReplaceEnd(arg, str, par.Length);
    }
    return arg;
  }
  private static Dictionary<string, int> PrefabNames = new();
  public static Sprite? FindSprite(string name) {
    if (PrefabNames.Count == 0) {
      PrefabNames = ZNetScene.instance.m_namedPrefabs.GroupBy(kvp => kvp.Value.name.ToLower()).ToDictionary(kvp => kvp.Key, kvp => kvp.First().Key);
    }

    name = name.ToLower();
    Sprite? sprite;
    if (PrefabNames.TryGetValue(name, out var hash)) {
      var prefab = ZNetScene.instance.GetPrefab(hash);
      sprite = prefab?.GetComponent<Piece>()?.m_icon;
      if (sprite) return sprite;
      sprite = prefab?.GetComponent<ItemDrop>()?.m_itemData?.m_shared?.m_icons.FirstOrDefault();
      if (sprite) return sprite;
    }
    var effect = ObjectDB.instance.m_StatusEffects.Find(se => se.name.ToLower() == name);
    sprite = effect?.m_icon;
    if (sprite) return sprite;
    var skill = Player.m_localPlayer.m_skills.m_skills.Find(skill => skill.m_skill.ToString().ToLower() == name);
    sprite = skill?.m_icon;
    if (sprite) return sprite;
    return null;
  }
  protected string ParseArgs(string command) {
    var scale = Scaling.Command;
    var args = command.Split(' ').ToArray();
    foreach (var arg in args) {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (split.Length < 2) continue;
      var value = split[1].ToLower();
      var range = Parse.TryFloatRange(value);
      if (name == CmdName) Name = split[1].Replace("_", " ");
      if (name == CmdDesc) Description = split[1].Replace("_", " ");
      if (name == CmdIcon) Icon = FindSprite(split[1]);
      if (name == CmdR) RadiusCap = Parse.TryFloatRange(value);
      if (name == CmdW) WidthCap = Parse.TryFloatRange(value);
      if (name == CmdD) DepthCap = Parse.TryFloatRange(value);
      if (name == CmdH) HeightCap = Parse.TryFloatRange(value);
    }
    var parameters = new[]{
      "r", "d", "w", "h", "a", "w,d", "x", "y", "z", "tx", "ty", "tz",
      "x,y", "x,z", "y,x", "y,z", "z,x", "z,y", "tx,ty", "tx,tz", "ty,tx", "ty,tz", "tz,tx", "tz,ty",
      "x,y,z", "x,z,y", "y,x,z", "y,z,x", "z,x,y", "z,y,x",
      "tx,ty,tz", "tx,tz,ty", "ty,tx,tz", "ty,tz,tx", "tz,tx,ty", "tz,ty,tx",
    };
    for (var i = 0; i < args.Length; i++) {
      foreach (var par in parameters)
        args[i] = Replace(args[i], par);
      if (args[i].Contains("#a"))
        Angle = true;
      if (args[i].Contains("#r"))
        Radius = scale.Value.x;
      if (args[i].Contains("#w"))
        Width = scale.Value.z;
      if (args[i].Contains("#d"))
        Depth = scale.Value.x;
      if (args[i].Contains("#h"))
        Height = scale.Value.y;
      if (args[i].Contains("#tx"))
        IsTargeted = true;
      if (args[i].Contains("#ty"))
        IsTargeted = true;
      if (args[i].Contains("#tz"))
        IsTargeted = true;
    }
    return string.Join(" ", args);
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Service;
using UnityEngine;
namespace InfinityHammer;
public class CommandParameters {
  public const string CmdName = "cmd_name";
  public const string CmdDesc = "cmd_desc";
  public const string CmdIcon = "cmd_icon";
  public const string CmdR = "cmd_r";
  public const string CmdW = "cmd_w";
  public const string CmdD = "cmd_d";
  public const string CmdH = "cmd_h";
  public const string CmdContinuous = "cmd_cont";
  public const string CmdMod1 = "cmd_mod1";
  public const string CmdMod2 = "cmd_mod2";
  public Range<float?> RadiusCap = new(null);
  public Range<float?> WidthCap = new(null);
  public Range<float?> DepthCap = new(null);
  public Range<float?> HeightCap = new(null);
  public float? Radius;
  public float? Ring;
  public float? Width;
  public float? Grid;
  public float? Depth;
  public float? Height;
  public bool Angle = false;
  public bool IsTargeted = false;
  public bool IsId = false;
  public string Continuous = "";
  public string Name = "Command";
  public string Description = "";
  public Sprite? Icon;
  public string IconValue = "";
  public string Command = "";

  public static string RemoveCmdParameters(string command) => string.Join(";", command.Split(';').Select(s => s.Trim()).Select(s => string.Join(" ", FilterCmd(s.Split(' ')))));
  private static IEnumerable<string> FilterCmd(IEnumerable<string> args) => args
    .Where(s => !s.StartsWith($"{CmdName}=", StringComparison.OrdinalIgnoreCase))
    .Where(s => !s.StartsWith($"{CmdDesc}=", StringComparison.OrdinalIgnoreCase))
    .Where(s => !s.StartsWith($"{CmdIcon}=", StringComparison.OrdinalIgnoreCase))
    .Where(s => !s.StartsWith($"{CmdContinuous}=", StringComparison.OrdinalIgnoreCase))
    .Where(s => !s.StartsWith($"{CmdR}=", StringComparison.OrdinalIgnoreCase))
    .Where(s => !s.StartsWith($"{CmdW}=", StringComparison.OrdinalIgnoreCase))
    .Where(s => !s.StartsWith($"{CmdD}=", StringComparison.OrdinalIgnoreCase))
    .Where(s => !s.StartsWith($"{CmdH}=", StringComparison.OrdinalIgnoreCase));
  public CommandParameters(string command, bool showCommand, bool replaceKeys = true) {
    var split = command.Split(';').Select(s => s.Trim()).ToArray();
    Command = string.Join(";", split.Select(s => ParseArgs(s, replaceKeys)));
    if (Radius.HasValue)
      Radius = Mathf.Clamp(Radius.Value, RadiusCap.Min ?? float.MinValue, RadiusCap.Max ?? float.MaxValue);
    if (Ring.HasValue)
      Ring = Mathf.Clamp(Ring.Value, RadiusCap.Min ?? float.MinValue, RadiusCap.Max ?? float.MaxValue);
    if (Height.HasValue)
      Height = Mathf.Clamp(Height.Value, HeightCap.Min ?? float.MinValue, HeightCap.Max ?? float.MaxValue);
    if (Width.HasValue)
      Width = Mathf.Clamp(Width.Value, WidthCap.Min ?? float.MinValue, WidthCap.Max ?? float.MaxValue);
    if (Grid.HasValue)
      Grid = Mathf.Clamp(Grid.Value, WidthCap.Min ?? float.MinValue, WidthCap.Max ?? float.MaxValue);
    if (Depth.HasValue)
      Depth = Mathf.Clamp(Depth.Value, DepthCap.Min ?? float.MinValue, DepthCap.Max ?? float.MaxValue);
    if (showCommand || Description == "")
      Description = RemoveCmdParameters(command);
  }


  public RulerParameters ToRuler() => new() {
    Radius = Radius,
    Ring = Ring,
    Width = Width,
    Grid = Grid,
    Depth = Depth,
    RotateWithPlayer = !Angle,
    IsTargeted = IsTargeted,
    Height = Height,
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
    if (!ZNetScene.instance) return null;
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
  protected string ParseArgs(string command, bool replaceKeys) {
    var scale = Scaling.Command;
    if (replaceKeys) {
      command = command.Replace(CmdMod1, Configuration.ModifierKey1());
      command = command.Replace(CmdMod2, Configuration.ModifierKey2());
    }
    var args = command.Split(' ').ToArray();
    foreach (var arg in args) {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (name == CmdContinuous) Continuous = "true";
      if (split.Length < 2) continue;
      var value = split[1].ToLower();
      if (name == CmdName) Name = split[1].Replace("_", " ");
      if (name == CmdDesc) Description = split[1].Replace("_", " ");
      if (name == CmdIcon) IconValue = split[1];
      if (name == CmdContinuous) Continuous = value;
      if (name == CmdR) RadiusCap = Parse.TryFloatNullRange(value);
      if (name == CmdW) WidthCap = Parse.TryFloatNullRange(value);
      if (name == CmdD) DepthCap = Parse.TryFloatNullRange(value);
      if (name == CmdH) HeightCap = Parse.TryFloatNullRange(value);
    }
    Icon = FindSprite(IconValue);
    var parameters = new[]{
      "id", "r", "r1-r2", "d", "w", "w1-w2", "h", "a", "w,d", "w1-w2,d", "x", "y", "z", "tx", "ty", "tz",
      "x,y", "x,z", "y,x", "y,z", "z,x", "z,y", "tx,ty", "tx,tz", "ty,tx", "ty,tz", "tz,tx", "tz,ty",
      "x,y,z", "x,z,y", "y,x,z", "y,z,x", "z,x,y", "z,y,x",
      "tx,ty,tz", "tx,tz,ty", "ty,tx,tz", "ty,tz,tx", "tz,tx,ty", "tz,ty,tx",
      "ignore"
    };
    for (var i = 0; i < args.Length; i++) {
      foreach (var par in parameters)
        args[i] = Replace(args[i], par);
      if (args[i].Contains("#id"))
        IsId = true;
      if (args[i].Contains("#a"))
        Angle = true;
      if (args[i].Contains("#r"))
        Radius = scale.Value.x;
      if (args[i].Contains("#r1-r2")) {
        Radius = scale.Value.x;
        Ring = scale.Value.z;
      }
      if (args[i].Contains("#w"))
        Width = scale.Value.x;
      if (args[i].Contains("#w1-w2")) {
        Width = scale.Value.x;
        Grid = scale.Value.z;
      }
      if (args[i].Contains("#d"))
        Depth = scale.Value.z;
      if (args[i].Contains("#h"))
        Height = scale.Value.y;
      if (args[i].Contains("#tx"))
        IsTargeted = true;
      if (args[i].Contains("#ty"))
        IsTargeted = true;
      if (args[i].Contains("#tz"))
        IsTargeted = true;
    }
    return string.Join(" ", FilterCmd(args));
  }
}

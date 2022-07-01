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
  public float? Radius = null;
  public float? Width = null;
  public float? Depth = null;
  public float? Height = null;
  public bool Angle = false;
  public string Name = "Command";
  public string Description = "";
  public Sprite? Icon = null;
  public string Command = "";

  public static string Join(string[] args) => string.Join(" ", args
    .Where(s => !s.StartsWith($"{CmdName}=", StringComparison.OrdinalIgnoreCase))
    .Where(s => !s.StartsWith($"{CmdDesc}=", StringComparison.OrdinalIgnoreCase))
    .Where(s => !s.StartsWith($"{CmdIcon}=", StringComparison.OrdinalIgnoreCase))
  );
  public CommandParameters(string[] args) {
    Description = Join(args);
    ParseArgs(args);
    Command = Join(args);
  }


  public RulerParameters ToRuler() => new() {
    Radius = Radius,
    Width = Width,
    Depth = Depth,
    RotateWithPlayer = !Angle,
    Height = Height
  };

  static bool IsParameter(string arg, string par) => arg == par || arg.EndsWith("=" + par, StringComparison.OrdinalIgnoreCase);
  static string ReplaceEnd(string arg, string par, int amount) => arg.Substring(0, arg.Length - amount) + par;

  private string Replace(string arg, string par) {
    if (IsParameter(arg, par)) {
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
  protected void ParseArgs(string[] args) {
    var scale = Scaling.Command;
    var radius = scale.Value.x;
    var width = scale.Value.x;
    var depth = scale.Value.z;
    var height = scale.Value.y;
    foreach (var arg in args) {
      var split = arg.Split('=');
      var name = split[0].ToLower();
      if (split.Length < 2) continue;
      var value = split[1].ToLower();
      var range = Parse.TryFloatRange(value);
      if (name == CmdName) Name = split[1].Replace("_", " ");
      if (name == CmdDesc) Description = split[1].Replace("_", " ");
      if (name == CmdIcon) Icon = FindSprite(split[1]);
      if (name == CmdR) radius = Mathf.Clamp(radius, range.Min, range.Max);
      if (name == CmdW) width = Mathf.Clamp(width, range.Min, range.Max);
      if (name == CmdD) depth = Mathf.Clamp(depth, range.Min, range.Max);
      if (name == CmdH) height = Mathf.Clamp(height, range.Min, range.Max);
    }
    var parameters = new[]{
      "r", "d", "w", "h", "a", "w,d", "x", "y", "z",
      "x,y", "x,z", "y,x", "y,z", "z,x", "z,y",
      "x,y,z", "x,z,y", "y,x,z", "y,z,x", "z,x,y", "z,y,x",
    };
    for (var i = 0; i < args.Length; i++) {
      foreach (var par in parameters)
        args[i] = Replace(args[i], par);
      if (args[i].Contains("#a"))
        Angle = true;
      if (args[i].Contains("#r"))
        Radius = radius;
      if (args[i].Contains("#w"))
        Width = width;
      if (args[i].Contains("#d"))
        Depth = depth;
      if (args[i].Contains("#h"))
        Height = height;
    }
  }
}

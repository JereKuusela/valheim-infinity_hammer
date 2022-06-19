using System;
using System.Linq;
using Service;
using UnityEngine;
namespace InfinityHammer;
public class CommandParameters {
  public static string CmdName = "cmd_name";
  public static string CmdDesc = "cmd_desc";
  public static string CmdIcon = "cmd_icon";
  public float? Radius = null;
  public float? Width = null;
  public float? Depth = null;
  public bool Angle = false;
  public string Name = "Command";
  public string Description = "";
  public Sprite? Icon = null;

  public static string Join(string[] args) => string.Join(" ", args
    .Where(s => !s.StartsWith($"{CmdName}=", StringComparison.OrdinalIgnoreCase))
    .Where(s => !s.StartsWith($"{CmdDesc}=", StringComparison.OrdinalIgnoreCase))
    .Where(s => !s.StartsWith($"{CmdIcon}=", StringComparison.OrdinalIgnoreCase))
  );
  public CommandParameters(string[] args) {
    Description = Join(args);
    ParseArgs(args);
  }


  public RulerParameters ToRuler() => new() {
    Radius = Radius,
    Width = Width,
    Depth = Depth,
    RotateWithPlayer = !Angle
  };

  static bool IsParameter(string arg, string par) => arg == par || arg.EndsWith("=" + par, StringComparison.OrdinalIgnoreCase);
  static string ReplaceEnd(string arg, string par, int amount) => arg.Substring(0, arg.Length - amount) + par;

  public static Sprite? FindSprite(string name) {
    name = name.ToLower();
    var prefab = ZNetScene.instance.GetPrefab(name);
    var sprite = prefab?.GetComponent<Piece>()?.m_icon;
    if (sprite) return sprite;
    sprite = prefab?.GetComponent<ItemDrop>()?.m_itemData?.m_shared?.m_icons.FirstOrDefault();
    if (sprite) return sprite;
    var effect = ObjectDB.instance.m_StatusEffects.Find(se => se.name.ToLower() == name);
    sprite = effect?.m_icon;
    if (sprite) return sprite;
    var skill = Player.m_localPlayer.m_skills.m_skills.Find(skill => skill.m_skill.ToString().ToLower() == name);
    sprite = skill?.m_icon;
    if (sprite) return sprite;
    return null;
  }
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
      if (name == CmdName) Name = split[1].Replace("_", " ");
      if (name == CmdDesc) Description = split[1].Replace("_", " ");
      if (name == CmdIcon) Icon = FindSprite(split[1]);
      if (name == "radius") {
        Radius = Parse.TryFloat(value, radius);
        args[i] = $"{name}=#radius";
      }
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

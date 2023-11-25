using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace InfinityHammer;

public class ToolData
{
  [DefaultValue("")]
  public string name = "";
  [DefaultValue("")]
  public string description = "";
  [DefaultValue("")]
  public string icon = "";
  public string command = "";
  [DefaultValue("")]
  public string continuous = "";
  [DefaultValue("")]
  public string initialHeight = "";
  [DefaultValue("")]
  public string initialSize = "";
  [DefaultValue("true")]
  public string snapGround = "true";
  [DefaultValue("")]
  public string playerHeight = "";
  [DefaultValue("false")]
  public string highlight = "false";
  [DefaultValue(null)]
  public bool? instant = null;
  [DefaultValue(null)]
  public int? tabIndex;
  [DefaultValue(null)]
  public int? index;
}

public class Tool
{
  public string Name;
  public string Command;
  private readonly string description;
  public string Description => ReplaceKeys(description);
  private readonly string iconName;
  private Sprite? icon;
  // Lazy load needed because the sprites are not instantly available.
  public Sprite? Icon => icon ??= Helper.FindSprite(iconName);
  private readonly string continuous;
  public bool Continuous => continuous == "true" || Helper.IsDown(ReplaceKeys(continuous));
  public string InitialHeight;
  public string InitialSize;
  private readonly string snapGround;
  public bool SnapGround => snapGround == "true" || Helper.IsDown(ReplaceKeys(snapGround));
  private readonly string playerHeight;
  public bool PlayerHeight => playerHeight == "true" || Helper.IsDown(ReplaceKeys(playerHeight));
  private readonly string highlight;
  public bool Highlight => highlight == "true" || Helper.IsDown(ReplaceKeys(highlight));
  public bool Instant;
  public int? TabIndex;
  public int? Index;

  // For ruler
  public bool Radius;
  public bool Ring;
  public bool Width;
  public bool Grid;
  public bool Depth;
  public bool Height;
  public bool RotateWithPlayer;
  public bool IsTargeted;
  public bool IsId;
  public RulerShape Shape = RulerShape.Circle;
  public Tool(ToolData data)
  {
    Name = data.name;
    Command = Parametrize(data.command);
    description = data.description.Replace("\\n", "\n");
    iconName = data.icon;
    continuous = data.continuous;
    InitialHeight = data.initialHeight;
    InitialSize = data.initialSize;
    snapGround = data.snapGround;
    playerHeight = data.playerHeight;
    highlight = data.highlight;
    TabIndex = data.tabIndex;
    Index = data.index;
    Instant = !Command.Contains("#");
    Instant = data.instant == null ? Instant : (bool)data.instant;
    ParseParameters(Command);
  }
  private void ParseParameters(string command)
  {
    var args = command.Split(' ').ToArray();

    RotateWithPlayer = true;
    for (var i = 0; i < args.Length; i++)
    {
      if (args[i].Contains("#id"))
        IsId = true;
      if (args[i].Contains("#a"))
        RotateWithPlayer = false;
      if (args[i].Contains("#r"))
        Radius = true;
      if (args[i].Contains("#r1-r2"))
      {
        Radius = true;
        Ring = true;
      }
      if (args[i].Contains("#w"))
        Width = true;
      if (args[i].Contains("#w1-w2"))
      {
        Width = true;
        Grid = true;
      }
      if (args[i].Contains("#d"))
        Depth = true;
      if (args[i].Contains("#h"))
        Height = true;
      if (args[i].Contains("#tx"))
        IsTargeted = true;
      if (args[i].Contains("#ty"))
        IsTargeted = true;
      if (args[i].Contains("#tz"))
        IsTargeted = true;
    }
  }
  private static string ReplaceKeys(string text) => text.Replace(ToolManager.CmdMod1, Configuration.ModifierKey1()).Replace(ToolManager.CmdMod2, Configuration.ModifierKey2());
  private static string Parametrize(string command)
  {
    command = CommandWrapper.Plain(command.Replace("hoe_", "hammer_").Replace("hammer_command", "hammer_tool"));
    command = command.Replace("hammer_tool ", "");
    var args = command.Split(' ').ToArray();
    string[] parameters = [
      "id", "r", "r1-r2", "d", "w", "w1-w2", "h", "a", "w,d", "w1-w2,d", "x", "y", "z", "tx", "ty", "tz",
      "x,y", "x,z", "y,x", "y,z", "z,x", "z,y", "tx,ty", "tx,tz", "ty,tx", "ty,tz", "tz,tx", "tz,ty",
      "x,y,z", "x,z,y", "y,x,z", "y,z,x", "z,x,y", "z,y,x",
      "tx,ty,tz", "tx,tz,ty", "ty,tx,tz", "ty,tz,tx", "tz,tx,ty", "tz,ty,tx",
      "ignore"
    ];
    for (var i = 0; i < args.Length; i++)
    {
      foreach (var par in parameters)
        args[i] = Replace(args[i], par);
    }
    return string.Join(" ", args);
  }
  static bool IsParameter(string arg, string par) => arg == par || arg.EndsWith("=" + par, StringComparison.OrdinalIgnoreCase);
  static string ReplaceEnd(string arg, string par, int amount) => arg.Substring(0, arg.Length - amount) + par;

  static string Replace(string arg, string par)
  {
    while (IsParameter(arg, par))
    {
      var str = string.Join(",", par.Split(',').Select(s => $"#{s}"));
      return ReplaceEnd(arg, str, par.Length);
    }
    return arg;
  }
}

public class InitialData
{
  public static string Get()
  {
    return @"hammer:
- name: Pipette
  description: |-
    Press cmd_mod1 to pick up.
    Press cmd_mod2 to freeze.
  icon: hammer
  command: hammer pick=cmd_mod1 freeze=cmd_mod2
  index: 1
- name: Building pipette
  description: |-
    Select entire buildings.
    Press cmd_mod1 to pick up.
    Press cmd_mod2 to freeze.
  icon: hammer
  command: hammer connect pick=cmd_mod1 freeze=cmd_mod2
- name: Area pipette
  description: |-
    Select multiple objects.
    Press cmd_mod1 to pick up.
    Press cmd_mod2 to freeze.
  icon: hammer
  command: hammer_area pick=cmd_mod1 freeze=cmd_mod2
  initialHeight: 0
  highlight: true
  snapGround: false
hoe:
- name: Level
  description: |-
    Flattens terrain.
    Hold cmd_mod1 to smooth.
  icon: mud_road
  command: hammer_terrain level smooth=cmd_mod1?.5:0
  index: 10
- name: Raise
  description: |-
    Raises terrain.
    Hold cmd_mod1 to smooth.
  icon: raise
  command: hammer_terrain raise=#h smooth=cmd_mod1?.5:0
  initialHeight: 0
- name: Pave
  description: |-
    Paves terrain.
    Hold cmd_mod1 for single use.
  icon: paved_road
  command: hammer_terrain paint=paved
  continuous: -cmd_mod1
- name: Grass
  description: |-
    Grass.
    Hold cmd_mod1 for single use.
  icon: replant
  command: hammer_terrain paint=grass
  continuous: -cmd_mod1
- name: Dirt
  description: |-
    Dirt.
    Hold cmd_mod1 for single use.
  icon: Hoe
  command: hammer_terrain paint=dirt
  continuous: -cmd_mod1
- name: Cultivate
  description: |-
    Cultivates terrain.
    Hold cmd_mod1 for single use.
  icon: cultivate
  command: hammer_terrain Area paint=cultivated
  continuous: -cmd_mod1
- name: DarkGrass
  description: |-
    Dark Grass.
    Hold cmd_mod1 for single use.
  icon: trophyabomination
  command: hammer_terrain paint=grass_dark
  continuous: -cmd_mod1
- name: PatchyGrass
  description: |-
    Patchy Grass.
    Hold cmd_mod1 for single use.
  icon: iron_wall_2x2
  command: hammer_terrain paint=patches
  continuous: -cmd_mod1
- name: MossyPaving
  description: |-
    Paving with moss.
    Hold cmd_mod1 for single use.
  icon: trophygreydwarfshaman
  command: hammer_terrain paint=paved_moss
  continuous: -cmd_mod1
- name: DarkPaving
  description: |-
    Dark Paving.
    Hold cmd_mod1 for single use.
  icon: tar
  command: hammer_terrain paint=paved_dark
  continuous: -cmd_mod1
- name: Reset
  description: |-
    Resets terrain.
    Hold cmd_mod1 for single use.
  icon: Hoe
  command: hammer_terrain reset
  continuous: -cmd_mod1
- name: Slope
  description: Slope between you and aim point.
  icon: wood_wall_roof_45
  command: hammer_slope
- name: Remove
  description: |-
    Removes objects.
    Hold cmd_mod1 to also reset the terrain.
  icon: softdeath
  command: hammer_object remove id=*;hammer_terrain keys=cmd_mod1 reset
- name: Tame
  description: |-
    Tames creatures.
    Hold cmd_mod1 to untame
  icon: Carrot
  command: hammer_object tame keys=-cmd_mod1;hammer_object wild keys=cmd_mod1
";
  }
}
using System;
using System.ComponentModel;
using System.Linq;
using InfinityHammer;
using ServerDevcommands;
using UnityEngine;

namespace InfinityTools;

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
  [DefaultValue("")]
  public string initialShape = "";
  [DefaultValue("false")]
  public string targetEdge = "false";
  [DefaultValue("false")]
  public string snapGround = "false";
  [DefaultValue("")]
  public string playerHeight = "";
  [DefaultValue("false")]
  public string highlight = "false";
  [DefaultValue("false")]
  public string terrainGrid = "false";
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
  public Sprite? Icon => icon ??= HammerHelper.FindSprite(iconName);
  private readonly string continuous;
  public bool Continuous => continuous == "true" || HammerHelper.IsDown(ReplaceKeys(continuous));
  public float? InitialHeight;
  public float? InitialSize;
  public string InitialShape;
  private readonly string snapGround;
  public bool SnapGround => snapGround == "true" || HammerHelper.IsDown(ReplaceKeys(snapGround));
  private readonly string playerHeight;
  public bool PlayerHeight => playerHeight == "true" || HammerHelper.IsDown(ReplaceKeys(playerHeight));
  private readonly string highlight;
  public bool Highlight => highlight == "true" || HammerHelper.IsDown(ReplaceKeys(highlight));
  private readonly string terrainGrid;
  public bool TerrainGrid => terrainGrid == "true" || HammerHelper.IsDown(ReplaceKeys(terrainGrid));
  public bool Instant;
  public int? TabIndex;
  public int? Index;

  // For ruler
  public bool Radius;
  public bool Ring;
  public bool Width;
  public bool Frame;
  public bool Depth;
  public bool Height;
  public bool RotateWithPlayer;
  private readonly string targetEdge;
  public bool IsTargetEdge => targetEdge == "true" || HammerHelper.IsDown(ReplaceKeys(targetEdge));
  public bool IsId;
  public Tool(ToolData data)
  {
    Name = data.name;
    Command = Plain(MultiCommands.Split(ReplaceHelpers(data.command)));
    description = data.description.Replace("\\n", "\n");
    iconName = data.icon;
    continuous = data.continuous;
    InitialHeight = data.initialHeight == "" ? null : Parse.Float(data.initialHeight);
    InitialSize = data.initialSize == "" ? null : Parse.Float(data.initialSize);
    InitialShape = data.initialShape;
    snapGround = data.snapGround;
    playerHeight = data.playerHeight;
    highlight = data.highlight;
    terrainGrid = data.terrainGrid;
    TabIndex = data.tabIndex;
    targetEdge = data.targetEdge;
    Index = data.index;
    Instant = !Command.Contains("<") && !Command.Contains("hammer ");
    Instant = data.instant == null ? Instant : (bool)data.instant;
    ParseParameters(Command);
  }
  private string ReplaceHelpers(string command) => command
    .Replace("hoe_", "tool_")
    .Replace("hammer_command", "")
    .Replace("<area>", "from=<x>,<z>,<y> circle=<r>-<r2> angle=<a> rect=<w>-<w2>,<d>")
    .Replace("<place>", "from=<x>,<z>,<y>")
    .Replace("<to>", "to=<x>,<z>,<y> circle=<r>-<r2> rect=<w>-<w2>,<d>");

  private void ParseParameters(string command)
  {
    var args = command.Split(' ').ToArray();

    RotateWithPlayer = true;
    for (var i = 0; i < args.Length; i++)
    {
      if (args[i].Contains("<id>"))
        IsId = true;
      if (args[i].Contains("<a>"))
        RotateWithPlayer = false;
      if (args[i].Contains("<r>"))
        Radius = true;
      if (args[i].Contains("<r2>"))
        Ring = true;
      if (args[i].Contains("<w>"))
        Width = true;
      if (args[i].Contains("<w2>"))
        Frame = true;
      if (args[i].Contains("<d>"))
        Depth = true;
      if (args[i].Contains("<h>"))
        Height = true;
    }
  }
  private static string ReplaceKeys(string text)
  {
    var alt = ZInput.instance.GetButtonDef("AltPlace").m_key.ToString().ToLowerInvariant();
    return text.Replace(ToolManager.CmdMod1, Configuration.ModifierKey1()).Replace(ToolManager.CmdMod2, Configuration.ModifierKey2()).Replace(ToolManager.CmdAlt, alt);
  }
  private static string Plain(string[] commands)
  {
    for (var i = 0; i < commands.Length; i++)
      commands[i] = Aliasing.Plain(commands[i]);
    return string.Join("; ", commands);
  }
}

public class InitialData
{
  public static string Get()
  {
    return @"hammer:
- name: Pipette
  description: |-
    Press <mod1> to pick up.
    Press <mod2> to freeze.
  icon: hammer
  command: hammer pick=<mod1> freeze=<mod2>
  index: 1
- name: Building pipette
  description: |-
    Select entire buildings.
    Press <mod1> to pick up.
    Press <mod2> to freeze.
  icon: hammer
  command: hammer connect pick=<mod1> freeze=<mod2>
- name: Area pipette
  description: |-
    Select multiple objects.
    Press <mod1> to pick up.
    Press <mod2> to freeze.
  icon: hammer
  command: hammer pick=<mod1> freeze=<mod2> height=<h> <area>
  initialHeight: 0
  highlight: true
hoe:
- name: Level
  description: |-
    Flattens terrain.
    Hold <mod1> to smooth.
    Hold <mod2> for free mode.
    Hold <alt> for player height.
  icon: mud_road
  command: terrain level smooth=<mod1>?.5:0 <area>
  index: 10
  terrainGrid: -<mod2>
  playerHeight: <alt>
- name: Raise
  description: |-
    Raises terrain.
    Hold <mod1> to smooth.
    Hold <mod2> for free mode.
  icon: raise
  command: terrain raise=<h> smooth=<mod1>?.5:0 <area>
  initialHeight: 0.2
  terrainGrid: -<mod2>
  snapGround : true
- name: Pave
  description: |-
    Paves terrain.
    Hold <mod1> for single use.
    Hold <mod2> for free mode.
  icon: paved_road
  command: terrain paint=paved <area>
  continuous: -<mod1>
  terrainGrid: -<mod2>
  snapGround : true
- name: Grass
  description: |-
    Grass.
    Hold <mod1> for single use.
    Hold <mod2> for free mode.
  icon: replant
  command: terrain paint=grass <area>
  continuous: -<mod1>
  terrainGrid: -<mod2>
  snapGround : true
- name: Dirt
  description: |-
    Dirt.
    Hold <mod1> for single use.
    Hold <mod2> for free mode.
  icon: Hoe
  command: terrain paint=dirt <area>
  continuous: -<mod1>
  terrainGrid: -<mod2>
  snapGround : true
- name: Cultivate
  description: |-
    Cultivates terrain.
    Hold <mod1> for single use.
    Hold <mod2> for free mode.
  icon: cultivate
  command: terrain paint=cultivated <area>
  continuous: -<mod1>
  terrainGrid: -<mod2>
  snapGround : true
- name: DarkGrass
  description: |-
    Dark Grass.
    Hold <mod1> for single use.
    Hold <mod2> for free mode.
  icon: trophyabomination
  command: terrain paint=grass_dark <area>
  continuous: -<mod1>
  terrainGrid: -<mod2>
  snapGround : true
- name: PatchyGrass
  description: |-
    Patchy Grass.
    Hold <mod1> for single use.
    Hold <mod2> for free mode.
  icon: iron_wall_2x2
  command: terrain paint=patches <area>
  continuous: -<mod1>
  terrainGrid: -<mod2>
  snapGround : true
- name: MossyPaving
  description: |-
    Paving with moss.
    Hold <mod1> for single use.
    Hold <mod2> for free mode.
  icon: trophygreydwarfshaman
  command: terrain paint=paved_moss <area>
  continuous: -<mod1>
  terrainGrid: -<mod2>
  snapGround : true
- name: DarkPaving
  description: |-
    Dark Paving.
    Hold <mod1> for single use.
    Hold <mod2> for free mode.
  icon: tar
  command: terrain paint=paved_dark <area>
  continuous: -<mod1>
  terrainGrid: -<mod2>
  snapGround : true
- name: Reset
  description: |-
    Resets terrain.
    Hold <mod1> for single use.
    Hold <mod2> for free mode.
  icon: Hoe
  command: terrain reset <area>
  continuous: -<mod1>
  terrainGrid: -<mod2>
  snapGround : true
- name: Slope
  description: Slope between you and aim point.
  icon: wood_wall_roof_45
  command: terrain slope <to>
  targetEdge: true
  initialShape: rectangle
- name: Remove
  description: |-
    Removes objects.
    Hold <mod1> to also reset the terrain.
  icon: softdeath
  command: object remove id=* <area>;terrain keys=<mod1> reset <area>
  highlight: true
- name: Tame
  description: |-
    Tames creatures.
    Hold <mod1> to untame
  icon: Carrot
  command: object tame keys=-<mod1> <area>;object wild keys=<mod1> <area>
";
  }
}
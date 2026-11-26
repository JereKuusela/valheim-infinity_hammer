using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ServerDevcommands;
using Service;
using UnityEngine;
using UnityEngine.InputSystem;

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
  public CommandData[]? commands;
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
  [DefaultValue("false")]
  public string snapPiece = "false";
  [DefaultValue("")]
  public string playerHeight = "";
  [DefaultValue("false")]
  public string highlight = "false";
  [DefaultValue("false")]
  public string terrainGrid = "false";
  [DefaultValue("")]
  public string finish = "";
  [DefaultValue(null)]
  public bool? instant = null;
  [DefaultValue("")]
  public string categories = "";
  [DefaultValue("")]
  public string materials = "";
  [DefaultValue(null)]
  public int? tabIndex;
  [DefaultValue("")]
  public string index = "";
  // Non-public so YamlDotNet's default field serialization skips it.
  internal bool IsDefaultData = false;
}

public class CommandData
{
  public string command = "";
  public string keys = "";
}

public class Tool
{
  public string Name;
  private List<CommandValue> Commands;
  private readonly string description;
  public string Description => DisplayKeys(description);
  private readonly string iconName;
  private Sprite? icon;
  // Lazy load needed because the sprites are not instantly available.
  public Sprite? Icon => icon ??= HammerHelper.FindSprite(iconName);
  private readonly string continuous;
  public bool Continuous => continuous == "true" || HammerHelper.IsDown(continuous);
  public float? InitialHeight;
  public float? InitialSize;
  public string InitialShape;
  private readonly string snapGround;
  public bool SnapGround => snapGround == "true" || HammerHelper.IsDown(snapGround);
  private readonly string playerHeight;
  public bool PlayerHeight => playerHeight == "true" || HammerHelper.IsDown(playerHeight);
  private readonly string highlight;
  public bool Highlight => highlight == "true" || HammerHelper.IsDown(highlight);
  private readonly string terrainGrid;
  public bool TerrainGrid => terrainGrid == "true" || HammerHelper.IsDown(terrainGrid);
  private readonly string snapPiece;
  public bool SnapPiece => snapPiece == "true" || HammerHelper.IsDown(snapPiece);
  public bool Instant;
  public int? TabIndex;
  private readonly string index;
  public bool FollowsPiece => index != "" && !int.TryParse(index, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
  public string TargetPiece => index.StartsWith("-", StringComparison.Ordinal) ? index.Substring(1) : index;
  // Number from the start, negative from the end, piece or tool name to go after it, or -name to go before it.
  // Returns -1 if the named piece is missing.
  public int GetSlot(IList<Piece> pieces, int previous)
  {
    var slot = previous + 1;
    if (int.TryParse(index, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
      slot = number < 0 ? pieces.Count + 1 + number : number;
    else if (index != "")
    {
      var before = index.StartsWith("-", StringComparison.Ordinal);
      var name = TargetPiece;
      slot = -1;
      for (var i = 0; i < pieces.Count; i++)
      {
        if (!IsNamed(pieces[i], name)) continue;
        slot = before ? i : i + 1;
        break;
      }
      if (slot < 0) return -1;
    }
    return Math.Max(0, Math.Min(slot, pieces.Count));
  }
  public static bool IsNamed(Piece piece, string name) => piece is BuildMenuTool { tool: { } tool }
    ? tool.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
    : Utils.GetPrefabName(piece.gameObject).Equals(name, StringComparison.OrdinalIgnoreCase)
      || Localization.instance.Localize(piece.m_name).Equals(name, StringComparison.OrdinalIgnoreCase);
  public Piece.UsageTagFlags Usage;
  public bool ShowInAll;
  // Lower case item prefab names.
  public HashSet<string>? Materials;

  // For ruler
  public bool Radius;
  public bool Ring;
  public bool Width;
  public bool Frame;
  public bool Depth;
  public bool Height;
  public bool RotateWithPlayer;
  private readonly string targetEdge;
  public bool IsTargetEdge => targetEdge == "true" || HammerHelper.IsDown(targetEdge);
  public bool IsId;
  // Number of positions to capture before running the command (from <xN>, <yN>, <zN>, <pN>).
  public int PointCount;
  // <p*> repeats its argument for every point after the numbered ones.
  public bool Variadic;
  public bool UsesPoints => PointCount > 0 || Variadic;
  private readonly string finish;
  public bool Finish => finish == "true" || HammerHelper.IsDown(finish);

  public Tool(ToolData data)
  {
    Name = data.name;
    finish = data.finish;
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
    snapPiece = data.snapPiece;
    TabIndex = data.tabIndex;
    Usage = ParseUsage(data.categories);
    var categories = SplitNames(data.categories);
    ShowInAll = categories.Count == 0 || categories.Contains("all", StringComparer.OrdinalIgnoreCase);
    Materials = ParseMaterials(data.materials);
    targetEdge = data.targetEdge;
    index = data.index.Trim();
    Commands = data.commands == null ? GetCommands(data.command) : [.. data.commands.Select(c => new CommandValue(c.command, c.keys))];
    Instant = Commands.All(c => !c.Command.Contains("<") && !c.Command.Contains("hammer "));
    Instant = data.instant == null ? Instant : (bool)data.instant;
    RotateWithPlayer = true;
    foreach (var cmd in Commands)
    {
      ParseParameters(cmd.Command);
      if (cmd.Command.Contains(RepeatPlaceholder))
        Variadic = true;
      foreach (Match match in PointRegex.Matches(cmd.Command))
        PointCount = Math.Max(PointCount, int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture));
    }
  }
  private static List<string> SplitNames(string value) => [.. value.Split(',').Select(s => s.Trim()).Where(s => s != "")];
  private static Piece.UsageTagFlags ParseUsage(string value)
  {
    Piece.UsageTagFlags result = 0;
    foreach (var name in SplitNames(value))
    {
      if (name.Equals("all", StringComparison.OrdinalIgnoreCase)) continue;
      // Numbers would be accepted as raw flag values.
      if (!int.TryParse(name, out _) && Enum.TryParse<Piece.UsageTagFlags>(name, true, out var usage)) result |= usage;
      else Log.Warning($"Tool category {name} is not a valid usage tag.");
    }
    return result;
  }
  private static HashSet<string>? ParseMaterials(string value)
  {
    var names = SplitNames(value);
    return names.Count == 0 ? null : [.. names.Select(s => s.ToLowerInvariant())];
  }
  public static readonly Regex PointRegex = new(@"<([xyz])(\d+)>");
  public const string RepeatPlaceholder = "<p*>";

  public string GetCommand()
  {
    var cmds = Commands.Where(c => c.IsDown());
    return string.Join("; ", cmds.Select(c => c.Command));
  }
  private void ParseParameters(string command)
  {
    var args = command.Split(' ').ToArray();

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
  // keys= is legacy but has to be supported for now.
  // Requires splitting the single command into multiple commands.
  private List<CommandValue> GetCommands(string rawCommand)
  {
    var commands = rawCommand.Split(';');
    Commands = [];
    foreach (var cmd in commands)
    {
      var keysStart = cmd.IndexOf("keys=", StringComparison.Ordinal);
      if (keysStart == -1)
      {
        Commands.Add(new CommandValue(cmd, ""));
        continue;
      }
      var keysEnd = cmd.IndexOf(" ", keysStart + 5, StringComparison.Ordinal);
      if (keysEnd == -1)
        keysEnd = cmd.Length;
      var keyStr = cmd.Substring(keysStart + 5, keysEnd - keysStart - 5);
      // -1 to get rid of the space before keys=
      var command = cmd.Substring(0, keysStart - 1) + cmd.Substring(keysEnd);
      Commands.Add(new CommandValue(command, keyStr));
    }
    return Commands;
  }
  private static string DisplayKeys(string text)
  {
    var def = ZInput.instance.GetButtonDef("AltPlace");
    InputBinding left = def.ButtonAction.bindings.FirstOrDefault();
    var str = "";
    if (left != null)
    {
      str = ZInput.instance.MapKeyFromPath(left.effectivePath).ToLowerInvariant();
    }
    return text.Replace(ToolManager.CmdMod1, Configuration.ModifierKey1()).Replace(ToolManager.CmdMod2, Configuration.ModifierKey2()).Replace(ToolManager.CmdAlt, str);
  }
}

public class CommandValue(string command, string keys)
{
  public string Command = Plain(MultiCommands.Split(ReplaceHelpers(command)));
  public string[] Keys = [.. Parse.Split(keys).Where(k => k[0] != '-')];
  public string[] BannedKeys = [.. Parse.Split(keys).Where(k => k[0] == '-').Select(k => k.Substring(1))];

  public bool IsDown() => Keys.All(HammerHelper.IsDown) && !BannedKeys.Any(HammerHelper.IsDown);
  private static string ReplaceHelpers(string command) => Regex.Replace(command, @"<p(\d+)>", "<x$1>,<z$1>,<y$1>")
    .Replace("hoe_", "tool_")
    .Replace("hammer_command", "")
    .Replace("<area>", "from=<x>,<z>,<y> circle=<r>-<r2> angle=<a> rect=<w>-<w2>,<d> ignore=<ignore> id=<include>")
    .Replace("<place>", "from=<x>,<z>,<y>")
    .Replace("<to>", "to=<x>,<z>,<y> circle=<r>-<r2> rect=<w>-<w2>,<d>");

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
  commands:
  - command: object remove id=* <area>
  - command: terrain reset <area>
    keys: <mod1>
  highlight: true
- name: Tame
  description: |-
    Tames creatures.
    Hold <mod1> to untame
  icon: Carrot
  commands:
  - command: object tame <area>
    keys: -<mod1>
  - command: object wild <area>
    keys: <mod1>
";
  }
}
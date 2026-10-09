
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ServerDevcommands;
using Service;
using UnityEngine;

namespace InfinityHammer;

public class ToolSelection : BaseSelection
{
  public Tool Tool;
  // Positions captured so far for tools with <xN>/<yN>/<zN>/<pN> placeholders.
  private readonly List<Vector3> Points = [];
  private PathRuler? Path;
  public ToolSelection(Tool tool)
  {
    Tool = tool;
    var scaling = Scaling.Get(true);
    if (tool.InitialHeight.HasValue)
      scaling.SetScaleY(tool.InitialHeight.Value);
    if (tool.InitialSize.HasValue)
    {
      scaling.SetScaleX(tool.InitialSize.Value);
      scaling.SetScaleZ(tool.InitialSize.Value);
    }
    if (tool.InitialShape != "")
      Ruler.Shape = Enum.TryParse(tool.InitialShape, true, out RulerShape shape) ? shape : RulerShape.Circle;

    SelectedPrefab = new GameObject();
    var piece = SelectedPrefab.AddComponent<Piece>();
    piece.m_name = tool.Name;
    piece.m_icon = tool.Icon;
    piece.m_description = tool.Description;
    piece.m_clipEverything = true;
    if (tool.SnapPiece)
      Snapping.CreateSnapPoint(SelectedPrefab, Vector3.zero, "Snap");
    Ruler.Create(tool);
  }

  public override float MaxPlaceDistance(float value) => 1000f;
  public override float SnapMultiplier => 2f;

  public override bool IsScalingSupported() => true;
  public override bool IsTool => true;
  public override bool Continuous => Tool.Continuous;
  public override bool PlayerHeight => Tool.PlayerHeight;
  public override bool TerrainGrid => Tool.TerrainGrid;
  public override void AfterPlace(GameObject obj)
  {
    HandleCommand(obj);
  }

  private void HandleCommand(GameObject obj)
  {
    var placedCommand = obj.AddComponent<PlacedCommand>();
    var ghost = HammerHelper.GetPlacementGhost().transform;
    if (Tool.UsesPoints && !CapturePoint(ghost.position))
      return;
    var x = ghost.position.x.ToString(CultureInfo.InvariantCulture);
    var y = ghost.position.y.ToString(CultureInfo.InvariantCulture);
    var z = ghost.position.z.ToString(CultureInfo.InvariantCulture);
    var scale = Scaling.Get();
    var radius = scale.X.ToString(CultureInfo.InvariantCulture);
    var innerSize = Mathf.Min(scale.X, scale.Z).ToString(CultureInfo.InvariantCulture);
    var outerSize = Mathf.Max(scale.X, scale.Z).ToString(CultureInfo.InvariantCulture);
    var depth = scale.Z.ToString(CultureInfo.InvariantCulture);
    var width = scale.X.ToString(CultureInfo.InvariantCulture);
    var shape = Ruler.Shape;
    if (shape == RulerShape.Circle)
    {
      innerSize = radius;
      outerSize = radius;
    }
    if (shape != RulerShape.Rectangle)
      depth = width;
    if (shape == RulerShape.Square)
    {
      innerSize = radius;
      outerSize = radius;
    }
    if (shape == RulerShape.Rectangle)
    {
      innerSize = width;
      outerSize = width;
    }
    var height = scale.Y.ToString(CultureInfo.InvariantCulture);
    var angle = ghost.rotation.eulerAngles.y.ToString(CultureInfo.InvariantCulture);
    if (TerrainGrid) angle = "0";

    var command = Tool.GetCommand();
    var multiShape = command.Contains("<r>") && (command.Contains("<w>") || command.Contains("<d>"));
    if (multiShape)
      command = RemoveUnusedShapeParameters(command, shape);

    if (Tool.UsesPoints)
    {
      var points = Points.ToArray();
      Points.Clear();
      if (Tool.Variadic) command = ExpandRepeated(command, points.Length);
      command = Tool.PointRegex.Replace(command, match =>
      {
        var index = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) - 1;
        if (index < 0 || index >= points.Length) return match.Value;
        var point = points[index];
        var value = match.Groups[1].Value == "x" ? point.x : match.Groups[1].Value == "y" ? point.y : point.z;
        return value.ToString(CultureInfo.InvariantCulture);
      });
    }

    if (command.Contains("<id>"))
    {
      var hovered = Selector.GetHovered(Configuration.Range, [], Configuration.IgnoredIds);
      if (hovered == null)
      {
        Helper.AddError(Console.instance, "Nothing is being hovered.", true);
        return;
      }
      command = command.Replace("<id>", Utils.GetPrefabName(hovered.gameObject));
    }
    if (shape == RulerShape.Frame)
      command = command.Replace("<d>", $"{innerSize}-{outerSize}");
    else
      command = command.Replace("<d>", depth);
    command = command.Replace("<r>", radius);
    command = command.Replace("<r2>", outerSize);
    command = command.Replace("<w>", width);
    command = command.Replace("<w2>", outerSize);
    command = command.Replace("<a>", angle);
    command = command.Replace("<x>", x);
    command = command.Replace("<y>", y);
    command = command.Replace("<z>", z);
    command = command.Replace("<h>", height);
    command = command.Replace("<ignore>", Configuration.configToolIgnoredIds.Value);
    command = command.Replace("<include>", Configuration.configToolIncludedIds.Value);
    if (!Configuration.DisableMessages)
      Console.instance.AddString($"Hammering command: {command}");
    placedCommand.Command = command;
  }

  private string ExpandRepeated(string command, int pointCount)
  {
    var result = new List<string>();
    foreach (var arg in command.Split(' '))
    {
      if (!arg.Contains(Tool.RepeatPlaceholder))
      {
        result.Add(arg);
        continue;
      }
      for (var i = Tool.PointCount + 1; i <= pointCount; i++)
        result.Add(arg.Replace(Tool.RepeatPlaceholder, $"<x{i}>,<z{i}>,<y{i}>"));
    }
    return string.Join(" ", result);
  }

  // Placing on top of the previous point also finishes a repeating path.
  private const float RepeatDistance = 0.05f;
  private const int MaxPoints = 64;
  // Returns true when all points are captured and the command can run.
  private bool CapturePoint(Vector3 position)
  {
    var minimum = Tool.PointCount + (Tool.Variadic ? 1 : 0);
    var repeated = Tool.Variadic && Points.Count > 0 && Utils.DistanceXZ(Points[Points.Count - 1], position) < RepeatDistance;
    if (!repeated) Points.Add(position);
    var finished = Tool.Variadic ? repeated || Tool.Finish || Points.Count >= MaxPoints : Points.Count >= minimum;
    if (finished && Points.Count < minimum)
    {
      Helper.AddError(Console.instance, $"At least {minimum} points are needed.", true);
      return false;
    }
    if (!finished && !Configuration.DisableMessages)
      Console.instance.AddString($"Hammering point {Points.Count}" + (Tool.Variadic ? "" : $"/{minimum}"));
    return finished;
  }

  private string RemoveUnusedShapeParameters(string command, RulerShape shape)
  {
    var isCircle = shape == RulerShape.Circle || shape == RulerShape.Ring;
    var commands = MultiCommands.Split(command);
    for (var i = 0; i < commands.Length; i++)
    {
      var args = commands[i].Split(' ').ToList();
      for (var j = args.Count - 1; j > -1; j--)
      {
        if (isCircle && (args[j].Contains("<w>") || args[j].Contains("<d>")))
          args.RemoveAt(j);
        if (!isCircle && args[j].Contains("<r>"))
          args.RemoveAt(j);
      }
      commands[i] = string.Join(" ", args);
    }
    return string.Join("; ", commands);
  }
  public override void Activate()
  {
    base.Activate();
    BindCommand.SetMode("command");
    Ruler.Create(Tool);
    if (Tool.UsesPoints && Path == null)
    {
      Path = new GameObject("InfinityHammerPath").AddComponent<PathRuler>();
      Path.Visible = false;
    }
  }
  public string DescriptionPoints() => !Tool.UsesPoints ? "" : Tool.Variadic ? $"point: {Points.Count + 1}" : $"point: {Points.Count + 1}/{Tool.PointCount}";
  public void UpdatePath(Player player)
  {
    if (Path == null) return;
    var ghost = player.m_placementGhost;
    var visible = ghost && ghost.activeInHierarchy && player.InPlaceMode() && !Hud.IsPieceSelectionVisible();
    Path.Visible = visible && Points.Count > 0;
    if (!visible) return;
    Path.Points = Points;
    Path.Cursor = ghost!.transform.position;
    Path.Radius = Tool.Width || Tool.Radius ? Scaling.Get().X : 0.5f;
    BaseRuler.SnapToGround = true;
    BaseRuler.Offset = 0f;
    Path.Refresh();
  }
  public override void Deactivate()
  {
    base.Deactivate();
    Ruler.Remove();
    BindCommand.SetMode("");
    Points.Clear();
    if (Path != null) UnityEngine.Object.Destroy(Path.gameObject);
    Path = null;
  }
}

// Delaying the execution solves many issues (allows the piece placing to finish).
public class PlacedCommand : MonoBehaviour
{
  public string Command = "";
  public void Start()
  {
    if (Command != "")
      Console.instance.TryRunCommand(Command);
    Destroy(gameObject);
  }
}
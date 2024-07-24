
using System;
using System.Globalization;
using System.Linq;
using InfinityHammer;
using ServerDevcommands;
using Service;
using UnityEngine;

namespace InfinityTools;
public class ToolSelection : BaseSelection
{
  public Tool Tool;
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
    Ruler.Create(tool);
  }

  public override float MaxPlaceDistance(float value) => 1000f;

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

    var command = Tool.Command;
    var multiShape = command.Contains("<r>") && (command.Contains("<w>") || command.Contains("<d>"));
    if (multiShape)
      command = RemoveUnusedShapeParameters(command, shape);

    if (command.Contains("<id>"))
    {
      var hovered = Selector.GetHovered(InfinityHammer.Configuration.Range, [], InfinityHammer.Configuration.IgnoredIds);
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
    command = command.Replace("<ignore>", InfinityHammer.Configuration.configIgnoredIds.Value);
    if (!InfinityHammer.Configuration.DisableMessages)
      Console.instance.AddString($"Hammering command: {command}");
    placedCommand.Command = command;
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

  }
  public override void Deactivate()
  {
    base.Deactivate();
    Ruler.Remove();
    BindCommand.SetMode("");
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
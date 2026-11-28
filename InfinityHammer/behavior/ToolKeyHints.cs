using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace InfinityHammer;

public static class ToolKeyHints
{
  private static readonly List<GameObject> Hints = [];
  private static readonly List<GameObject> Hidden = [];
  private static Tool? ShownTool;
  private static RulerShape? ShownShape;
  private static bool ShownSnap;
  private static RectTransform? Row;
  private static Vector2 Size;
  private static Vector3 Scale;

  private static readonly Dictionary<KeyCode, string> KeyNames = new()
  {
    { KeyCode.LeftShift, "$button_lshift" },
    { KeyCode.RightShift, "$button_rshift" },
    { KeyCode.LeftAlt, "$button_lalt" },
    { KeyCode.RightAlt, "$button_ralt" },
    { KeyCode.LeftControl, "$button_lctrl" },
    { KeyCode.RightControl, "$button_rctrl" },
    { KeyCode.Mouse0, "$button_mouse0" },
    { KeyCode.Mouse1, "$button_mouse1" },
    { KeyCode.Mouse2, "$button_mouse2" },
    { KeyCode.Space, "$button_space" },
    { KeyCode.Return, "$button_return" },
  };

  public static void Update(Tool tool, RulerShape? shape)
  {
    var snap = tool.SnapPiece;
    if (ShownTool == tool && ShownShape == shape && ShownSnap == snap) return;
    Hide();
    var row = KeyHints.instance ? KeyHints.instance.m_buildHints.transform.Find("Keyboard") : null;
    var key = row ? row!.Find("AltPlace") : null;
    var wheel = row ? row!.Find("rotate") : null;
    var plus = row ? row!.Find("Copy/Text (1)") : null;
    if (!row || !key || !wheel || !plus) return;
    ShownTool = tool;
    ShownShape = shape;
    ShownSnap = snap;
    HideVanilla(wheel!);
    if (!snap)
    {
      HideVanilla(key!);
      HideVanilla(row!.Find("Snap"));
    }
    var circle = shape == RulerShape.Circle || shape == RulerShape.Ring;
    if (Shapes() > 1) Add(row!, key!, null, plus!, "Change shape", Configuration.shapeKey.Value);
    if (shape != null) Add(row!, key!, wheel, plus!, circle ? "Radius" : "Width", Configuration.commandRadius.Value);
    if (shape == RulerShape.Ring || shape == RulerShape.Frame || shape == RulerShape.Rectangle) Add(row!, key!, wheel, plus!, "Depth", Configuration.commandDepth.Value);
    if (tool.Height) Add(row!, key!, wheel, plus!, "Height", Configuration.commandHeight.Value);
    if (!tool.RotateWithPlayer && !circle) Add(row!, key!, wheel, plus!, "Rotate", Configuration.commandRotate.Value);
    Fit((RectTransform)row!);
  }

  public static void Hide()
  {
    foreach (var hint in Hints)
    {
      if (!hint) continue;
      hint.SetActive(false);
      Object.Destroy(hint);
    }
    Hints.Clear();
    foreach (var obj in Hidden)
      if (obj) obj.SetActive(true);
    Hidden.Clear();
    if (Row)
    {
      Row!.sizeDelta = Size;
      Row.localScale = Scale;
    }
    Row = null;
    ShownTool = null;
    ShownShape = null;
  }

  private static int Shapes() => new[] {
    Ruler.Circle && Configuration.ShapeCircle,
    Ruler.Ring && Configuration.ShapeRing,
    Ruler.Square && Configuration.ShapeSquare,
    Ruler.Frame && Configuration.ShapeFrame,
    Ruler.Rectangle && Configuration.ShapeRectangle,
  }.Count(on => on);

  private static void Fit(RectTransform row)
  {
    Row = row;
    Size = row.sizeDelta;
    Scale = row.localScale;
    LayoutRebuilder.ForceRebuildLayoutImmediate(row);
    var width = row.rect.width;
    var needed = LayoutUtility.GetPreferredWidth(row);
    if (needed <= width) return;
    row.sizeDelta += new Vector2(needed - width, 0f);
    row.localScale = Scale * (width / needed);
  }

  private static float FitIcon(RectTransform icon)
  {
    if (icon.GetComponent<Image>() is not { preserveAspect: true, sprite: { } sprite }) return 0f;
    var width = icon.rect.height * sprite.rect.width / sprite.rect.height;
    var element = icon.GetComponent<LayoutElement>() ?? icon.gameObject.AddComponent<LayoutElement>();
    element.preferredWidth = width;
    var xs = sprite.vertices.Select(v => v.x).ToList();
    var drawn = width * (xs.Max() - xs.Min()) * sprite.pixelsPerUnit / sprite.rect.width;
    return (width - drawn) / 2f;
  }

  private static void HideVanilla(Transform? obj)
  {
    if (!obj || !obj!.gameObject.activeSelf) return;
    obj.gameObject.SetActive(false);
    Hidden.Add(obj.gameObject);
  }

  private static void Add(Transform row, Transform key, Transform? wheel, Transform plus, string label, KeyboardShortcut shortcut)
  {
    var hint = Object.Instantiate((wheel ?? key).gameObject, row);
    hint.name = "IH " + label;
    hint.SetActive(true);
    var text = hint.GetComponentInChildren<TMP_Text>();
    text.text = label;
    if (text.GetComponent<LayoutElement>() is { } textSize) textSize.preferredWidth = -1f;
    var keys = shortcut.Modifiers.Prepend(shortcut.MainKey).Where(k => k != KeyCode.None).Select(KeyName).ToList();
    if (wheel)
    {
      var index = hint.transform.Find("mousew_icon").GetSiblingIndex();
      foreach (var name in keys)
      {
        var box = Object.Instantiate(key.Find("key_bkg").gameObject, hint.transform);
        box.transform.SetSiblingIndex(index++);
        box.transform.Find("Key").GetComponent<TMP_Text>().text = name;
      }
      var margin = FitIcon((RectTransform)hint.transform.Find("mousew_icon"));
      if (keys.Count > 0)
      {
        var sign = (RectTransform)Object.Instantiate(plus.gameObject, hint.transform).transform;
        sign.SetSiblingIndex(index);
        sign.GetComponent<LayoutElement>().preferredWidth += margin;
      }
    }
    else
      hint.transform.Find("key_bkg/Key").GetComponent<TMP_Text>().text = string.Join(" + ", keys);
    Hints.Add(hint);
  }

  private static string KeyName(KeyCode key) => KeyNames.TryGetValue(key, out var name) ? Localization.instance.Localize(name) : key.ToString();
}

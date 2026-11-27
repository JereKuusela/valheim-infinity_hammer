using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ServerDevcommands;
using TMPro;
using UnityEngine;

namespace InfinityHammer;

public static class SpriteHelper
{
  private static Dictionary<string, int> PrefabNames = [];
  private static Dictionary<string, Sprite>? StatusEffectIcons;
  private static readonly Dictionary<string, Sprite> SpriteCache = [];
#nullable disable
  private static GameObject _cachedCameraObj;
#nullable enable
  private static GameObject CameraObj => _cachedCameraObj ??= CreateCamera();
  private static Camera Camera => CameraObj.GetComponent<Camera>();
  private static GameObject CreateCamera()
  {
    var obj = new GameObject("CachedTextRenderCamera");
    var camera = obj.AddComponent<Camera>();
    camera.orthographic = true;
    camera.nearClipPlane = 0.1f;
    camera.farClipPlane = 10f;
    camera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.4f);
    camera.clearFlags = CameraClearFlags.SolidColor;

    // Isolate the camera to prevent interference with main rendering
    camera.enabled = false; // Disable by default, only enable during rendering
    camera.cullingMask = LayerMask.GetMask("ghost"); // Don't render any layers by default
    camera.depth = -1000; // Very low depth to avoid conflicts

    // Don't destroy on load to keep it cached
    UnityEngine.Object.DontDestroyOnLoad(obj);
    return obj;
  }

  public static Sprite? FindSprite(string name)
  {
    if (SpriteCache.TryGetValue(name, out var cached)) return cached;
    if (!CanResolve()) return null;
    var sprite = FindExistingSprite(name);
    if (sprite) return SpriteCache[name] = sprite!;
    CreateTextSprites([name]);
    return SpriteCache.TryGetValue(name, out cached) ? cached : null;
  }

  // Text sprites are rendered in batches because each GPU readback stalls.
  public static void PrepareSprites(IEnumerable<string> names)
  {
    if (!CanResolve()) return;
    List<string> texts = [];
    HashSet<string> seen = [];
    foreach (var name in names)
    {
      if (SpriteCache.ContainsKey(name) || !seen.Add(name)) continue;
      var sprite = FindExistingSprite(name);
      if (sprite) SpriteCache[name] = sprite!;
      else texts.Add(name);
    }
    CreateTextSprites(texts);
  }

  // Missing data would cache text sprites for names that have a real icon.
  private static bool CanResolve() => ZNetScene.instance && ObjectDB.instance && Player.m_localPlayer && Hud.instance;

  private static Sprite? FindExistingSprite(string name)
  {
    if (PrefabNames.Count == 0)
      PrefabNames = ZNetScene.instance.m_namedPrefabs.GroupBy(kvp => kvp.Value.name.ToLower()).ToDictionary(kvp => kvp.Key, kvp => kvp.First().Key);
    StatusEffectIcons ??= ObjectDB.instance.m_StatusEffects.Where(se => se && se.m_icon).GroupBy(se => se.name.ToLower()).ToDictionary(g => g.Key, g => g.First().m_icon);

    var lower = name.ToLower();
    int spriteIndex = 1;
    var name_parts = Parse.Split(lower);
    if (name_parts.Length > 1)
    {
      lower = name_parts[0];
      spriteIndex = int.TryParse(name_parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var index) ? index : 1;
    }

    if (PrefabNames.TryGetValue(lower, out var hash))
    {
      var prefab = ZNetScene.instance.GetPrefab(hash);
      var sprite = prefab?.GetComponent<Piece>()?.m_icon;
      if (sprite) return sprite;
      sprite = prefab?.GetComponent<ItemDrop>()?.m_itemData?.m_shared?.m_icons.ElementAtOrDefault(spriteIndex - 1);
      if (sprite) return sprite;
    }
    if (StatusEffectIcons.TryGetValue(lower, out var effectIcon)) return effectIcon;
    var skill = Player.m_localPlayer.m_skills.m_skills.Find(skill => skill.m_skill.ToString().ToLowerInvariant() == lower);
    return skill?.m_icon;
  }

  private const int TextureSize = 64;
  private const int AtlasColumns = 16;
  // Far apart so that each viewport only sees its own text.
  private const float TextSpacing = 100f;
  private static readonly Vector3 RenderOrigin = new(10000f, 10000f, 0f);

  private static void CreateTextSprites(List<string> texts)
  {
    texts = [.. texts.Where(t => t.Length > 0)];
    for (var start = 0; start < texts.Count; start += AtlasColumns * AtlasColumns)
      RenderBatch([.. texts.Skip(start).Take(AtlasColumns * AtlasColumns)]);
  }

  // Each text gets its own viewport in a shared texture, so only one readback is needed.
  private static void RenderBatch(List<string> texts)
  {
    var columns = Math.Min(AtlasColumns, texts.Count);
    var rows = (texts.Count + columns - 1) / columns;
    var width = columns * TextureSize;
    var height = rows * TextureSize;
    List<GameObject> objects = [];
    var renderTexture = new RenderTexture(width, height, 24) { format = RenderTextureFormat.ARGB32 };
    try
    {
      Camera.enabled = true;
      Camera.targetTexture = renderTexture;
      CameraObj.transform.rotation = Quaternion.identity;
      for (var i = 0; i < texts.Count; i++)
      {
        var text = texts[i];
        var isHuge = text.StartsWith("+");
        if (isHuge) text = text.Substring(1);
        var obj = new GameObject("TempTextSprite");
        objects.Add(obj);
        obj.transform.position = RenderOrigin + new Vector3(i * TextSpacing, 0f, 0f);
        obj.SetActive(false);
        var mesh = obj.AddComponent<TextMeshPro>();
        mesh.font = Hud.instance.m_buildSelection.font;
        mesh.fontSize = 8;
        mesh.color = Color.white;
        mesh.alignment = TextAlignmentOptions.Center;
        mesh.text = GenerateDisplayName(text);
        obj.SetActive(true);
        obj.layer = LayerMask.NameToLayer("ghost");
        mesh.ForceMeshUpdate();

        var center = obj.transform.position + mesh.bounds.center;
        Camera.rect = new Rect((float)(i % columns) / columns, 1f - (float)(i / columns + 1) / rows, 1f / columns, 1f / rows);
        Camera.aspect = 1f;
        Camera.orthographicSize = mesh.text.Length <= 2 ? 0.75f : 2f;
        if (isHuge) Camera.orthographicSize *= 0.25f;
        CameraObj.transform.position = center - new Vector3(0f, 0f, 5f);
        Camera.Render();
      }

      RenderTexture.active = renderTexture;
      var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
      texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
      texture.Apply();
      RenderTexture.active = null;

      for (var i = 0; i < texts.Count; i++)
      {
        var rect = new Rect(i % columns * TextureSize, height - (i / columns + 1) * TextureSize, TextureSize, TextureSize);
        SpriteCache[texts[i]] = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
      }
    }
    finally
    {
      Camera.targetTexture = null;
      Camera.rect = new Rect(0f, 0f, 1f, 1f);
      Camera.enabled = false;
      renderTexture.Release();
      UnityEngine.Object.Destroy(renderTexture);
      foreach (var obj in objects)
        UnityEngine.Object.DestroyImmediate(obj);
    }
  }

  public static string GenerateDisplayName(string name)
  {
    if (string.IsNullOrEmpty(name)) return "";

    // Step 1: Remove '_' prefix if present
    if (name.StartsWith("_"))
      name = name.Substring(1);

    // Step 2: Split at underscores and uppercase letters
    var parts = new List<string>();
    var currentPart = "";

    for (int i = 0; i < name.Length; i++)
    {
      char c = name[i];

      if (c == '_' || c == ' ' || c == ':')
      {
        if (currentPart.Length > 0)
        {
          parts.Add(currentPart);
          currentPart = "";
        }
      }

      // Split at uppercase letter (but keep the uppercase letter)
      else if (char.IsUpper(c) && currentPart.Length > 0)
      {
        // If previous also is also upper dont split yet
        if (i > 0 && char.IsUpper(name[i - 1]))
        {
          currentPart += c;
          continue;
        }
        parts.Add(currentPart);
        currentPart = c.ToString();
      }
      else
      {
        currentPart += c;
      }
    }

    // Add the last part
    if (currentPart.Length > 0)
      parts.Add(currentPart);

    // Step 3: Trim each part to maximum 10 letters
    for (int i = 0; i < parts.Count; i++)
    {
      if (parts[i].Length > 10)
        parts[i] = parts[i].Substring(0, 10);
    }

    // Step 4: Capitalize first letter of each part
    for (int i = 0; i < parts.Count; i++)
    {
      if (parts[i].Length > 0)
        parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
    }

    return string.Join("\n", parts.Take(3));
  }

}
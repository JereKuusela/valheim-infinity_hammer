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
  private static readonly Dictionary<string, Sprite> GeneratedSpriteCache = [];
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
    camera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.7f); // Semi-transparent dark background
    camera.clearFlags = CameraClearFlags.SolidColor;
    // Don't destroy on load to keep it cached
    UnityEngine.Object.DontDestroyOnLoad(obj);
    return obj;
  }

  public static Sprite? FindSprite(string name)
  {
    if (!ZNetScene.instance) return null;
    if (PrefabNames.Count == 0)
    {
      PrefabNames = ZNetScene.instance.m_namedPrefabs.GroupBy(kvp => kvp.Value.name.ToLower()).ToDictionary(kvp => kvp.Key, kvp => kvp.First().Key);
    }

    var lower = name.ToLower();
    Sprite? sprite;
    int spriteIndex = 1;
    var name_parts = Parse.Split(lower);
    if (name_parts.Length > 1)
    {
      lower = name_parts[0];
      spriteIndex = int.TryParse(name_parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var index) ? index : 1;
    }

    // Try to find existing sprite first
    if (PrefabNames.TryGetValue(lower, out var hash))
    {
      var prefab = ZNetScene.instance.GetPrefab(hash);
      sprite = prefab?.GetComponent<Piece>()?.m_icon;
      if (sprite) return sprite;
      sprite = prefab?.GetComponent<ItemDrop>()?.m_itemData?.m_shared?.m_icons.ElementAtOrDefault(spriteIndex - 1);
      if (sprite) return sprite;
    }
    var effect = ObjectDB.instance.m_StatusEffects.Find(se => se.name.ToLower() == lower);
    sprite = effect?.m_icon;
    if (sprite) return sprite;
    var skill = Player.m_localPlayer.m_skills.m_skills.Find(skill => skill.m_skill.ToString().ToLowerInvariant() == lower);
    sprite = skill?.m_icon;
    if (sprite) return sprite;

    // Auto-generate text sprite if nothing found (with caching)
    return GetOrCreateTextSprite(name);
  }

  private static Sprite? GetOrCreateTextSprite(string text)
  {
    if (text.Length == 0) return null;
    if (GeneratedSpriteCache.TryGetValue(text, out var cachedSprite))
    {
      return cachedSprite;
    }

    // Generate new sprite and cache it
    var sprite = CreateTextSprite(text);
    GeneratedSpriteCache[text] = sprite;
    return sprite;
  }

  private static Sprite CreateTextSprite(string text)
  {
    // Create a temporary GameObject with TextMeshPro component
    GameObject tempObj = new GameObject("TempTextSprite");
    try
    {
      tempObj.SetActive(false);
      var mesh = tempObj.AddComponent<TextMeshPro>();
      mesh.font = Hud.instance.m_buildSelection.font;
      mesh.text = GenerateDisplayName(text);
      mesh.fontSize = 12;
      mesh.color = Color.white;
      mesh.alignment = TextAlignmentOptions.Center;
      mesh.enableAutoSizing = true;
      mesh.fontSizeMin = 8;
      mesh.fontSizeMax = 16;
      tempObj.SetActive(true);

      // Set up the transform
      tempObj.transform.position = Vector3.zero;

      // Force mesh generation
      mesh.ForceMeshUpdate();

      // Get text bounds
      var bounds = mesh.bounds;

      // Update camera settings for current text
      Camera.orthographicSize = Mathf.Max(bounds.size.x, bounds.size.y) * 0.6f;
      CameraObj.transform.position = new Vector3(bounds.center.x, bounds.center.y, -5f);
      CameraObj.transform.LookAt(bounds.center);

      // Create RenderTexture
      int textureSize = 64;
      RenderTexture renderTexture = new RenderTexture(textureSize, textureSize, 24);
      renderTexture.format = RenderTextureFormat.ARGB32;
      Camera.targetTexture = renderTexture;

      // Render the text
      Camera.Render();

      // Convert RenderTexture to Texture2D
      RenderTexture.active = renderTexture;
      Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
      texture.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
      texture.Apply();
      RenderTexture.active = null;

      // Create sprite from texture
      Sprite sprite = Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f));

      // Cleanup render texture (but keep camera cached)
      renderTexture.Release();

      return sprite;
    }
    finally
    {
      // Always cleanup the temporary GameObject
      UnityEngine.Object.DestroyImmediate(tempObj);
    }
  }


  /// <summary>
  /// Generates a display name from a prefab name following specific formatting rules:
  /// 1. Removes '_' prefix if present
  /// 2. Splits at underscores and uppercase letters (keeping uppercase letters)
  /// 3. Trims each part to maximum 10 letters
  /// 4. Capitalizes first letter of each part
  /// 5. Combines parts up to 10 total letters
  /// </summary>
  /// <param name="name">The original name to process</param>
  /// <returns>The formatted display name</returns>
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

      // Split at underscore
      if (c == '_')
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
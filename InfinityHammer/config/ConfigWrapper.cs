using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace Service;
public class ConfigWrapper
{

  private readonly ConfigFile ConfigFile;
  public ConfigWrapper(string command, ConfigFile configFile)
  {
    ConfigFile = configFile;

    new Terminal.ConsoleCommand(command, "[key] [value] - Toggles or sets a config value.", (Terminal.ConsoleEventArgs args) =>
    {
      if (args.Length < 2) return;
      if (!SettingHandlers.TryGetValue(args[1].ToLower(), out var handler)) return;
      if (args.Length == 2)
        handler(args.Context, "");
      else
        handler(args.Context, string.Join(" ", args.Args, 2, args.Length - 2));
    }, optionsFetcher: () => SettingHandlers.Keys.ToList());
  }
  private ConfigEntry<T> Create<T>(string group, string name, T value, ConfigDescription description)
  {
    var configEntry = ConfigFile.Bind(group, name, value, description);
    return configEntry;
  }
  private ConfigEntry<T> Create<T>(string group, string name, T value, string description) => Create(group, name, value, new ConfigDescription(description));
  public ConfigEntry<T> Bind<T>(string group, string name, T value, ConfigDescription description)
  {
    var configEntry = Create(group, name, value, description);
    if (configEntry is ConfigEntry<bool> boolEntry)
      Register(boolEntry);
    else if (configEntry is ConfigEntry<KeyboardShortcut> keyEntry)
      Register(keyEntry);
    else
      Register(configEntry);
    return configEntry;
  }
  public ConfigEntry<string> BindList(string group, string name, string value, string description) => Bind(group, name, value, new ConfigDescription(description));
  public ConfigEntry<string> BindList(string group, string name, string value, ConfigDescription description)
  {
    var configEntry = Create(group, name, value, description);
    RegisterList(configEntry);
    return configEntry;
  }
  public ConfigEntry<KeyboardShortcut> BindCommand(Func<string> command, string group, string name, KeyboardShortcut value, string description, string mode = "")
  {
    var configEntry = Create(group, name, value, description);
    RegisterCommand(configEntry, command, mode);
    return configEntry;
  }
  public ConfigEntry<KeyboardShortcut> BindCommand(string command, string group, string name, KeyboardShortcut value, string description, string mode = "")
  {
    var configEntry = Create(group, name, value, description);
    RegisterCommand(configEntry, () => command, mode);
    return configEntry;
  }
  public ConfigEntry<KeyboardShortcut> BindWheelCommand(string command, string group, string name, KeyboardShortcut value, string description, string mode = "")
  {
    var configEntry = Create(group, name, value, description);
    RegisterWheelCommand(configEntry, command, mode);
    return configEntry;
  }
  public ConfigEntry<T> Bind<T>(string group, string name, T value, string description) => Bind(group, name, value, new ConfigDescription(description));
  private static void AddMessage(Terminal context, string message)
  {
    context.AddString(message);
    Player.m_localPlayer?.Message(MessageHud.MessageType.TopLeft, message);
  }
  private readonly Dictionary<string, Action<Terminal, string>> SettingHandlers = [];
  private void Register(ConfigEntry<bool> setting)
  {
    var name = setting.Definition.Key;
    var key = ToKey(name);
    SettingHandlers.Add(key, (Terminal terminal, string value) => Toggle(terminal, setting, name, value));
  }
  public static string GetKeys(KeyboardShortcut key)
  {
    // Empty value would mess up Server Devcommands logic.
    if (key.MainKey == KeyCode.None) return "unbound";
    var keys = key.MainKey.ToString().ToLower();
    if (key.Modifiers.Count() > 0) keys += "," + string.Join(",", key.Modifiers);
    return keys;
  }
  private static void UpdateKey(string name, KeyboardShortcut key, Func<string> command, string mode = "")
  {
    Console.instance.TryRunCommand($"unbind {name} silent");
    if (key.MainKey == KeyCode.None) return;
    var keys = GetKeys(key);
    if (mode != "")
      keys += $",{mode}";
    var bind = $"bind {keys} tag={name} {command()}";
    Console.instance.TryRunCommand(bind);
  }
  private readonly List<Action> BindCalls = [];
  public void SetupBinds()
  {
    foreach (var call in BindCalls) call();
  }
  private string ToKey(string name) => name.ToLower().Replace(' ', '_').Replace("(", "").Replace(")", "");
  private void RegisterCommand(ConfigEntry<KeyboardShortcut> setting, Func<string> command, string mode = "")
  {
    var name = setting.Definition.Key;
    var key = ToKey(name);
    setting.SettingChanged += (s, e) => UpdateKey(key, setting.Value, command, mode);
    BindCalls.Add(() => UpdateKey(key, setting.Value, command, mode));
    SettingHandlers.Add(key, (Terminal terminal, string value) => SetKey(terminal, setting, name, value));
  }
  private static void UpdateWheelKey(string name, KeyboardShortcut key, string command, string mode = "")
  {
    Console.instance.TryRunCommand($"unbind {name} silent");
    if (key.MainKey == KeyCode.None) return;
    var keys = key.MainKey.ToString().ToLower();
    if (key.Modifiers.Count() > 0) keys += "," + string.Join(",", key.Modifiers);
    if (mode != "")
      keys += $",{mode}";
    var bind = $"bind wheel,{keys} tag={name} {command}";
    Console.instance.TryRunCommand(bind);
  }
  private void RegisterWheelCommand(ConfigEntry<KeyboardShortcut> setting, string command, string mode = "")
  {
    var name = setting.Definition.Key;
    var key = ToKey(name);
    setting.SettingChanged += (s, e) => UpdateWheelKey(key, setting.Value, command, mode);
    BindCalls.Add(() => UpdateWheelKey(key, setting.Value, command, mode));
    SettingHandlers.Add(key, (Terminal terminal, string value) => SetKey(terminal, setting, name, value));
  }
  private void Register(ConfigEntry<KeyboardShortcut> setting)
  {
    var name = setting.Definition.Key;
    var key = ToKey(name);
    SettingHandlers.Add(key, (Terminal terminal, string value) => SetKey(terminal, setting, name, value));
  }
  private void RegisterList(ConfigEntry<string> setting)
  {
    var name = setting.Definition.Key;
    var key = ToKey(name);
    SettingHandlers.Add(key, (Terminal terminal, string value) => ToggleFlag(terminal, setting, name, value));
  }
  private void Register<T>(ConfigEntry<T> setting)
  {
    var name = setting.Definition.Key;
    var key = ToKey(name);
    SettingHandlers.Add(key, (Terminal terminal, string value) => SetValue(terminal, setting, name, value));
  }
  private static string State(bool value) => value ? "enabled" : "disabled";
  private static string Flag(bool value) => value ? "Removed" : "Added";
  private static readonly HashSet<string> Truthies = [
    "1",
    "true",
    "yes",
    "on"
  ];
  private static bool IsTruthy(string value) => Truthies.Contains(value);
  private static readonly HashSet<string> Falsies = [
    "0",
    "false",
    "no",
    "off"
  ];
  private static bool IsFalsy(string value) => Falsies.Contains(value);

  private static void Toggle(Terminal context, ConfigEntry<bool> setting, string name, string value)
  {
    if (value == "") setting.Value = !setting.Value;
    else if (IsTruthy(value)) setting.Value = true;
    else if (IsFalsy(value)) setting.Value = false;
    AddMessage(context, $"{name} {State(setting.Value)}.");
  }
  private static void SetKey(Terminal context, ConfigEntry<KeyboardShortcut> setting, string name, string value)
  {
    if (value == "")
    {
      AddMessage(context, $"{name}: {setting.Value}.");
      return;
    }
    if (!Enum.TryParse<KeyCode>(value, true, out var keyCode))
      throw new InvalidOperationException("'" + value + "' is not a valid UnityEngine.KeyCode.");
    setting.Value = new(keyCode);
    AddMessage(context, $"{name} set to {value}.");
  }
  public static int TryParseInt(string value, int defaultValue)
  {
    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)) return result;
    return defaultValue;
  }
  public static int TryParseInt(ConfigEntry<string> setting)
  {
    if (int.TryParse(setting.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)) return result;
    return TryParseInt((string)setting.DefaultValue, 0);
  }
  private static float TryParseFloat(string value, float defaultValue)
  {
    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)) return result;
    return defaultValue;
  }
  public static float TryParseFloat(ConfigEntry<string> setting)
  {
    if (float.TryParse(setting.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)) return result;
    return TryParseFloat((string)setting.DefaultValue, 0f);
  }
  private static HashSet<string> ParseList(string value) => value.Split(',').Select(s => s.Trim().ToLower()).ToHashSet();
  private static void ToggleFlag(Terminal context, ConfigEntry<string> setting, string name, string value)
  {
    if (value == "")
    {
      AddMessage(context, $"{name}: {setting.Value}\".");
      return;
    }
    var list = ParseList(setting.Value);
    var newList = ParseList(value);
    foreach (var flag in newList)
    {
      var remove = list.Contains(flag);
      if (remove) list.Remove(flag);
      else list.Add(flag);
      setting.Value = string.Join(",", list);
      AddMessage(context, $"{name}: {Flag(remove)} \"{flag}\".");
    }
  }
  private static void SetValue<T>(Terminal context, ConfigEntry<T> setting, string name, string value)
  {
    if (value == "")
    {
      AddMessage(context, $"{name}: {setting.Value}.");
      return;
    }
    setting.Value = (T)(object)value;
    AddMessage(context, $"{name} set to {value}.");
  }
}


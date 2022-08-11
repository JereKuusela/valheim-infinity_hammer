using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
namespace InfinityHammer;
public static class CommandWrapper {
  public static Assembly? ServerDevcommands = null;
  public static Assembly? WorldEditCommands = null;
  public static Assembly? StructureTweaks = null;
  public static void Init() {
    if (Chainloader.PluginInfos.TryGetValue("server_devcommands", out var info)) {
      if (info.Metadata.Version.Major == 1 && info.Metadata.Version.Minor < 25)
        InfinityHammer.Log.LogWarning($"Server Devcommands v{info.Metadata.Version.Major}.{info.Metadata.Version.Minor} is outdated. Please update!");
      else
        ServerDevcommands = info.Instance.GetType().Assembly;

    }
    if (Chainloader.PluginInfos.TryGetValue("world_edit_commands", out info)) {
      if (info.Metadata.Version.Major == 1 && info.Metadata.Version.Minor < 8)
        InfinityHammer.Log.LogWarning($"World Edit Commands v{info.Metadata.Version.Major}.{info.Metadata.Version.Minor} is outdated. Please update!");
      else
        WorldEditCommands = info.Instance.GetType().Assembly;
    }
    if (Chainloader.PluginInfos.TryGetValue("structure_tweaks", out info)) {
      StructureTweaks = info.Instance.GetType().Assembly;
    }
  }
#nullable disable
  private static BindingFlags PublicBinding = BindingFlags.Static | BindingFlags.Public;
  private static Type Type() => ServerDevcommands.GetType("ServerDevcommands.AutoComplete");
  private static Type InfoType() => ServerDevcommands.GetType("ServerDevcommands.ParameterInfo");
  private static MethodInfo GetMethod(Type type, string name, Type[] types) => type.GetMethod(name, PublicBinding, null, CallingConventions.Standard, types, null);
  public static void Register(string command, Func<int, int, List<string>> action) {
    if (ServerDevcommands == null) return;
    GetMethod(Type(), "Register", new[] { typeof(string), typeof(Func<int, int, List<string>>) }).Invoke(null, new object[] { command, action });
  }
  public static void Register(string command, Func<int, List<string>> action) {
    if (ServerDevcommands == null) return;
    GetMethod(Type(), "Register", new[] { typeof(string), typeof(Func<int, List<string>>) }).Invoke(null, new object[] { command, action });
  }
  public static void Register(string command, Func<int, int, List<string>> action, Dictionary<string, Func<int, List<string>>> named) {
    if (ServerDevcommands == null) return;
    GetMethod(Type(), "Register", new[] { typeof(string), typeof(Func<int, int, List<string>>), typeof(Dictionary<string, Func<int, List<string>>>) }).Invoke(null, new object[] { command, action, named });
  }
  public static void Register(string command, Func<int, List<string>> action, Dictionary<string, Func<int, List<string>>> named) {
    if (ServerDevcommands == null) return;
    GetMethod(Type(), "Register", new[] { typeof(string), typeof(Func<int, List<string>>), typeof(Dictionary<string, Func<int, List<string>>>) }).Invoke(null, new object[] { command, action, named });
  }
  public static void RegisterEmpty(string command) {
    if (ServerDevcommands == null) return;
    Type().GetMethod("RegisterEmpty", PublicBinding).Invoke(null, new[] { command });
  }
  public static List<string> Info(string value) {
    if (ServerDevcommands == null) return null;
    return GetMethod(InfoType(), "Create", new[] { typeof(string) }).Invoke(null, new[] { value }) as List<string>;
  }
  public static List<string> ObjectIds() {
    if (ServerDevcommands == null) return ZNetScene.instance.GetPrefabNames();
    return InfoType().GetProperty("ObjectIds", PublicBinding).GetValue(null) as List<string>;
  }
  public static List<string> LocationIds() {
    if (ServerDevcommands == null) return ZoneSystem.instance.m_locations.Select(loc => loc.m_prefabName).ToList();
    return InfoType().GetProperty("LocationIds", PublicBinding).GetValue(null) as List<string>;
  }
  public static List<string> Scale(string name, string description, int index) {
    if (ServerDevcommands == null) return null;
    return GetMethod(InfoType(), "Scale", new[] { typeof(string), typeof(string), typeof(int) }).Invoke(null, new object[] { name, description, index }) as List<string>;
  }
  public static List<string> Scale(string description, int index) {
    if (ServerDevcommands == null) return null;
    return GetMethod(InfoType(), "Scale", new[] { typeof(string), typeof(int) }).Invoke(null, new object[] { description, index }) as List<string>;
  }
  public static List<string> XYZ(string name, string description, int index) {
    if (ServerDevcommands == null) return null;
    return GetMethod(InfoType(), "XYZ", new[] { typeof(string), typeof(string), typeof(int) }).Invoke(null, new object[] { name, description, index }) as List<string>;
  }
  public static List<string> XYZ(string description, int index) {
    if (ServerDevcommands == null) return null;
    return GetMethod(InfoType(), "XYZ", new[] { typeof(string), typeof(int) }).Invoke(null, new object[] { description, index }) as List<string>;
  }
  public static List<string> XZY(string description, int index) {
    if (ServerDevcommands == null) return null;
    return GetMethod(InfoType(), "XZY", new[] { typeof(string), typeof(int) }).Invoke(null, new object[] { description, index }) as List<string>;
  }
  public static List<string> FRU(string description, int index) {
    if (ServerDevcommands == null) return null;
    return GetMethod(InfoType(), "XYZ", new[] { typeof(string), typeof(int) }).Invoke(null, new object[] { description, index }) as List<string>;
  }
  public static void AddCompositeCommand(string command) {
    if (ServerDevcommands == null) return;
    GetMethod(InfoType(), "AddCompositeCommand", new[] { typeof(string) }).Invoke(null, new object[] { command });
  }
}

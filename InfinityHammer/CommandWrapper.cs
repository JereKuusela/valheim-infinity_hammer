using System;
using System.Collections.Generic;
using System.Reflection;

namespace InfinityHammer {
  public static class CommandWrapper {
    private static BindingFlags PublicBinding = BindingFlags.Static | BindingFlags.Public;
    private static Type Type() => InfinityHammer.ServerDevcommands.GetType("ServerDevcommands.AutoComplete");
    private static Type InfoType() => InfinityHammer.ServerDevcommands.GetType("ServerDevcommands.ParameterInfo");
    private static MethodInfo GetMethod(Type type, string name, Type[] types) => type.GetMethod(name, PublicBinding, null, CallingConventions.Standard, types, null);
    public static void Register(string command, Func<int, int, List<string>> action) {
      if (!InfinityHammer.IsServerDevcommands) return;
      GetMethod(Type(), "Register", new[] { typeof(string), typeof(Func<int, int, List<string>>) }).Invoke(null, new object[] { command, action });
    }
    public static void Register(string command, Func<int, List<string>> action) {
      if (!InfinityHammer.IsServerDevcommands) return;
      GetMethod(Type(), "Register", new[] { typeof(string), typeof(Func<int, List<string>>) }).Invoke(null, new object[] { command, action });
    }
    public static void Register(string command, Func<int, int, List<string>> action, Dictionary<string, Func<int, List<string>>> named) {
      if (!InfinityHammer.IsServerDevcommands) return;
      GetMethod(Type(), "Register", new[] { typeof(string), typeof(Func<int, int, List<string>>), typeof(Dictionary<string, Func<int, List<string>>>) }).Invoke(null, new object[] { command, action, named });
    }
    public static void Register(string command, Func<int, List<string>> action, Dictionary<string, Func<int, List<string>>> named) {
      if (!InfinityHammer.IsServerDevcommands) return;
      GetMethod(Type(), "Register", new[] { typeof(string), typeof(Func<int, List<string>>), typeof(Dictionary<string, Func<int, List<string>>>) }).Invoke(null, new object[] { command, action, named });
    }
    public static void RegisterEmpty(string command) {
      if (!InfinityHammer.IsServerDevcommands) return;
      Type().GetMethod("RegisterEmpty", PublicBinding).Invoke(null, new[] { command });
    }
    public static List<string> Info(string value) {
      if (!InfinityHammer.IsServerDevcommands) return null;
      return InfoType().GetMethod("Create", PublicBinding).Invoke(null, new[] { value }) as List<string>;
    }
    public static List<string> ObjectIds() {
      if (!InfinityHammer.IsServerDevcommands) return ZNetScene.instance.GetPrefabNames();
      return InfoType().GetProperty("ObjectIds", PublicBinding).GetValue(null) as List<string>;
    }
    public static List<string> Scale(string name, string description, int index) {
      if (!InfinityHammer.IsServerDevcommands) return ZNetScene.instance.GetPrefabNames();
      return GetMethod(InfoType(), "Scale", new[] { typeof(string), typeof(string), typeof(int) }).Invoke(null, new object[] { name, description, index }) as List<string>;
    }
    public static List<string> Scale(string description, int index) {
      if (!InfinityHammer.IsServerDevcommands) return ZNetScene.instance.GetPrefabNames();
      return GetMethod(InfoType(), "Scale", new[] { typeof(string), typeof(int) }).Invoke(null, new object[] { description, index }) as List<string>;
    }
    public static List<string> XYZ(string name, string description, int index) {
      if (!InfinityHammer.IsServerDevcommands) return ZNetScene.instance.GetPrefabNames();
      return GetMethod(InfoType(), "XYZ", new[] { typeof(string), typeof(string), typeof(int) }).Invoke(null, new object[] { name, description, index }) as List<string>;
    }
    public static List<string> XYZ(string description, int index) {
      if (!InfinityHammer.IsServerDevcommands) return ZNetScene.instance.GetPrefabNames();
      return GetMethod(InfoType(), "XYZ", new[] { typeof(string), typeof(int) }).Invoke(null, new object[] { description, index }) as List<string>;
    }
  }
}

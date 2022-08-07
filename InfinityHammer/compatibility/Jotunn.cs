using System.Reflection;
using BepInEx.Bootstrap;
namespace InfinityHammer;
public static class JotunnWrapper {
  public const string GUID = "com.jotunn.jotunn";
  private static Assembly? Jotunn;
  public static void Run() {
    if (!Chainloader.PluginInfos.TryGetValue(GUID, out var info)) return;
    Jotunn = info.Instance.GetType().Assembly;
    if (Jotunn == null) return;
  }
#nullable disable
  private static BindingFlags PublicBinding = BindingFlags.Static | BindingFlags.Public;
  public static void IsCustomlocation(string name) {
    if (Jotunn == null) return;
    var type = Jotunn.GetType("Jotunn.Managers.KeyHintManager");
    if (type == null) return;
    var field = type.GetField("Instance", PublicBinding);
    if (field == null) return;
    var method = type.GetField("Instance", PublicBinding);
    //return (bool)method.Invoke(null, new object[] { name });
  }
}

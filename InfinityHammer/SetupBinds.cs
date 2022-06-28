using System.Linq;
using HarmonyLib;
namespace InfinityHammer;
[HarmonyPatch(typeof(Chat), nameof(Chat.Awake))]
public class SetupBinds {

  private static void SetupThing(string oldBindsStr, string newBindsStr) {
    var oldBinds = oldBindsStr.Split('|').Select(s => s.Trim()).Where(s => s != "").ToArray();
    var newBinds = newBindsStr.Split('|').Select(s => s.Trim()).Where(s => s != "").ToArray();
  }
  private static void Setup() {
    if (Configuration.CustomBinds == "") return;
    var binds = Configuration.CustomBinds.Split('|').Select(s => s.Trim()).ToArray();
    var keys = binds.Select(bind => bind.Split(' ').First().Split(',').First()).ToHashSet();
    foreach (var key in keys) Console.instance.TryRunCommand($"unbind {key}");
    foreach (var bind in binds) Console.instance.TryRunCommand($"bind {bind}");
  }
  private static string MigrateScaleUp(string bind) {
    if (!bind.Contains("hammer_scale_up ")) return bind;
    return bind.Replace("hammer_scale_up ", "hammer_scale build 5%");
  }
  private static string MigrateScaleDown(string bind) {
    if (!bind.Contains("hammer_scale_down ")) return bind;
    return bind.Replace("hammer_scale_down ", "hammer_scale build -5%");
  }
  private static void MigrateBinds() {
    if (!Terminal.m_bindList.Any(bind => bind.Contains("hammer_scale_up ") || bind.Contains("hammer_scale_down "))) return;
    for (var i = 0; i < Terminal.m_bindList.Count; i++) {
      Terminal.m_bindList[i] = MigrateScaleUp(Terminal.m_bindList[i]);
      Terminal.m_bindList[i] = MigrateScaleDown(Terminal.m_bindList[i]);
    }
    Terminal.updateBinds();
  }
  static void Postfix() {
    MigrateBinds();
    Setup();
    Configuration.Wrapper.SetupBinds();
  }
}

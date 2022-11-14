using System.Linq;
using HarmonyLib;
namespace InfinityHammer;
[HarmonyPatch(typeof(Chat), nameof(Chat.Awake))]
public class SetupBinds
{

  private static void SetupThing(string oldBindsStr, string newBindsStr)
  {
    var oldBinds = oldBindsStr.Split('|').Select(s => s.Trim()).Where(s => s != "").ToArray();
    var newBinds = newBindsStr.Split('|').Select(s => s.Trim()).Where(s => s != "").ToArray();
  }
  private static void Setup()
  {
    if (Configuration.CustomBinds == "") return;
    var binds = Configuration.CustomBinds.Split('|').Select(s => s.Trim()).ToArray();
    var keys = binds.Select(bind => bind.Split(' ').First().Split(',').First()).ToHashSet();
    foreach (var key in keys) Console.instance.TryRunCommand($"unbind {key}");
    foreach (var bind in binds) Console.instance.TryRunCommand($"bind {bind}");
  }
  private static string MigrateHammerScale(string bind)
  {
    if (!bind.Contains("hammer_scale build ")) return bind;
    return bind.Replace("hammer_scale build ", "hammer_zoom ");
  }
  private static string MigrateHammerScaleCmd(string bind)
  {
    if (!bind.Contains("hammer_scale command ")) return bind;
    return bind.Replace("hammer_scale command ", "hammer_zoom_cmd ");
  }
  private static void MigrateBinds()
  {
    if (!Terminal.m_bindList.Any(bind => bind.Contains("hammer_scale build ") || bind.Contains("hammer_scale command "))) return;
    for (var i = 0; i < Terminal.m_bindList.Count; i++)
    {
      Terminal.m_bindList[i] = MigrateHammerScale(Terminal.m_bindList[i]);
      Terminal.m_bindList[i] = MigrateHammerScaleCmd(Terminal.m_bindList[i]);
    }
    Terminal.updateBinds();
  }
  static void Postfix()
  {
    MigrateBinds();
    Setup();
    Configuration.Wrapper.SetupBinds();
  }
}

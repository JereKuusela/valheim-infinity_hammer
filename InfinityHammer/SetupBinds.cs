using System.Linq;
using HarmonyLib;
namespace InfinityHammer;
[HarmonyPatch(typeof(Chat), nameof(Chat.Awake))]
public class SetupBinds {
  public static void Setup() {
    if (Configuration.Binds == "") return;
    var binds = Configuration.Binds.Split('|').Select(s => s.Trim()).ToArray();
    var keys = binds.Select(bind => bind.Split(' ').First().Split(',').First()).ToHashSet();
    foreach (var key in keys) Console.instance.TryRunCommand($"unbind {key}");
    foreach (var bind in binds) Console.instance.TryRunCommand($"bind {bind}");
  }
  static void Postfix() => Setup();
}

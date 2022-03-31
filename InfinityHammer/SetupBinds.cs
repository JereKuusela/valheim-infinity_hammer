using System.Linq;
using HarmonyLib;
namespace InfinityHammer;
[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
public class SetupBinds {
  public static void Setup() {
    if (Settings.Binds == "") return;
    var binds = Settings.Binds.Split(';').Select(s => s.Trim()).ToArray();
    var keys = binds.Select(bind => bind.Split(' ').First().Split(',').First()).ToHashSet();
    foreach (var key in keys) Console.instance.TryRunCommand($"unbind {key}");
    foreach (var bind in binds) Console.instance.TryRunCommand($"bind {bind}");
  }
  static void Postfix() => Setup();
}

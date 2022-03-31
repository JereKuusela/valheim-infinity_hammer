using System.Linq;
using HarmonyLib;
namespace InfinityHammer;

[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
public class FejdStartupAwake {
  static void Postfix() {
    if (Settings.AutoExec == "") return;
    var commands = Settings.AutoExec.Split(';').Select(s => s.Trim()).ToArray();
    foreach (var command in commands) Console.instance.TryRunCommand(command);
  }
}

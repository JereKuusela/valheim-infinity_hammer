using HarmonyLib;
// Prevents script error by awake functions trying to do ZNetView stuff.
namespace InfinityHammer;

[HarmonyPatch(typeof(BaseAI), nameof(BaseAI.Awake))]
public class BaseAIAwake {
  static bool Prefix() => !ZNetView.m_forceDisableInit;
}
[HarmonyPatch(typeof(TreeBase), nameof(TreeBase.Awake))]
public class TreeBaseAwake {
  static bool Prefix() => !ZNetView.m_forceDisableInit;
}
[HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.Awake))]
public class MonsterAIAwake {
  static bool Prefix() => !ZNetView.m_forceDisableInit;
}
[HarmonyPatch(typeof(TombStone), nameof(TombStone.Awake))]
public class TombStoneAwake {
  static bool Prefix() => !ZNetView.m_forceDisableInit;
}
[HarmonyPatch(typeof(LocationProxy), nameof(LocationProxy.Awake))]
public class LocationProxyAwake {
  static bool Prefix() => !ZNetView.m_forceDisableInit;
}

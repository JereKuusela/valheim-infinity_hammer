using HarmonyLib;
// Prevents script error by awake functions trying to do ZNetView stuff.
namespace InfinityHammer;

[HarmonyPatch(typeof(BaseAI), nameof(BaseAI.Awake))]
public class BaseAIAwake
{
  static bool Prefix() => !ZNetView.m_forceDisableInit;
}
[HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.Awake))]
public class DungeonGeneratorAwake
{
  static bool Prefix() => !ZNetView.m_forceDisableInit;
}
[HarmonyPatch(typeof(TreeBase), nameof(TreeBase.Awake))]
public class TreeBaseAwake
{
  static bool Prefix() => !ZNetView.m_forceDisableInit;
}
[HarmonyPatch(typeof(TreeLog), nameof(TreeLog.Awake))]
public class TreeLogAwake
{
  static bool Prefix() => !ZNetView.m_forceDisableInit;
}
[HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.Awake))]
public class MonsterAIAwake
{
  static bool Prefix() => !ZNetView.m_forceDisableInit;
}
[HarmonyPatch(typeof(TombStone), nameof(TombStone.Awake))]
public class TombStoneAwake
{
  static bool Prefix() => !ZNetView.m_forceDisableInit;
}
[HarmonyPatch(typeof(LocationProxy), nameof(LocationProxy.Awake))]
public class LocationProxyAwake
{
  static bool Prefix() => !ZNetView.m_forceDisableInit;
}
[HarmonyPatch(typeof(Character), nameof(Character.Awake))]
public class CharacterAwake
{
  static void Postfix(Character __instance)
  {
    if (ZNetView.m_forceDisableInit) Character.s_characters.Remove(__instance);
  }
}

// For some reason, vanilla messes up the object name.
[HarmonyPatch(typeof(MineRock5), nameof(MineRock5.Awake))]
public class MineRock5_NameFix
{
  static void Prefix(MineRock5 __instance, ref string __state)
  {
    __state = __instance.name;
  }
  static void Postfix(MineRock5 __instance, string __state)
  {
    __instance.name = __state;
  }
}


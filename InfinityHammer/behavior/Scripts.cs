using HarmonyLib;

// Prevents script error by creature awake functions trying to do ZNetView stuff.
namespace InfinityHammer {

  [HarmonyPatch(typeof(BaseAI), "Awake")]
  public class BaseAIAwake {
    public static bool Prefix() => !ZNetView.m_forceDisableInit;
  }
  [HarmonyPatch(typeof(TreeBase), "Awake")]
  public class TreeBaseAwake {
    public static bool Prefix() => !ZNetView.m_forceDisableInit;
  }
  [HarmonyPatch(typeof(MonsterAI), "Awake")]
  public class MonsterAIAwake {
    public static bool Prefix() => !ZNetView.m_forceDisableInit;
  }
  [HarmonyPatch(typeof(TombStone), "Awake")]
  public class TombStoneAwake {
    public static bool Prefix() => !ZNetView.m_forceDisableInit;
  }
  [HarmonyPatch(typeof(LocationProxy), "Awake")]
  public class LocationProxyAwake {
    public static bool Prefix() => !ZNetView.m_forceDisableInit;
  }
}
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace InfinityHammer;

[HarmonyPatch]
public class JotunnRefreshCategories
{
  static bool Prepare() => Chainloader.PluginInfos.ContainsKey("com.jotunn.jotunn");
  static MethodBase TargetMethod() => AccessTools.Method("Jotunn.Managers.PieceManager:RefreshCategories");
  static bool Prefix() => !Player.m_localPlayer || !Hammer.IsInfinityHammer(Player.m_localPlayer.m_buildPieces);
}

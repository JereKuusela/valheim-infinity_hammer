namespace InfinityHammer;

public class NoCreator()
{
  private static readonly int XRaySteamName = "steamName".GetStableHashCode();
  private static readonly int XRaySteamID = "steamID".GetStableHashCode();
  private static readonly int XRayCreatedID = "xray_created".GetStableHashCode();
  public static void Set(ZNetView view, Piece piece)
  {
    // Creator data is only interesting for actual targets. Dummy components will have these both as false.
    if (!piece.m_randomTarget && !piece.m_primaryTarget) return;
    var zdo = view.GetZDO();
    if (Configuration.NoCreator)
    {
      zdo.RemoveLong(ZDOVars.s_creator);
      // String doesn't have RemoveString.
      ZDOExtraData.s_strings.Remove(zdo.m_uid, ZDOVars.s_creatorName);
      ZDOExtraData.s_strings.Remove(zdo.m_uid, XRaySteamName);
      ZDOExtraData.s_strings.Remove(zdo.m_uid, XRaySteamID);
      zdo.RemoveLong(XRayCreatedID);
    }
    else
      piece.SetCreator(Game.instance.GetPlayerProfile().GetPlayerID());
    if (Configuration.NoPrimaryTarget && piece.m_primaryTarget)
    {
      piece.m_primaryTarget = false;
      zdo.Set("HasFields", true);
      zdo.Set("HasFieldsStaticTarget", true);
      zdo.Set("StaticTarget.m_primaryTarget", false);
      view.LoadFields();
    }
    if (Configuration.NoSecondaryTarget && piece.m_randomTarget)
    {
      piece.m_randomTarget = false;
      zdo.Set("HasFields", true);
      zdo.Set("HasFieldsStaticTarget", true);
      zdo.Set("StaticTarget.m_randomTarget", false);
      view.LoadFields();
    }
  }
}
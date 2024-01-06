namespace InfinityHammer;

public class NoCreator()
{

  public static void Set(ZNetView view, Piece piece)
  {
    // Creator data is only interesting for actual targets. Dummy components will have these both as false.
    if (!piece.m_randomTarget && !piece.m_primaryTarget) return;
    var zdo = view.GetZDO();
    if (Configuration.NoCreator)
    {
      zdo.Set(ZDOVars.s_creator, 0L);
      if (zdo.GetString(ZDOVars.s_creatorName) != "")
        zdo.Set(ZDOVars.s_creatorName, "");
      piece.m_creator = 0;
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
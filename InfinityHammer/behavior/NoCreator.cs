using Service;

namespace InfinityHammer;

public class NoCreator()
{


  public static void Set(ZNetView view, Piece piece)
  {
    var zdo = view.GetZDO();
    if (Configuration.NoCreator)
    {
      zdo.RemoveLong(ZDOVars.s_creator);
      // String doesn't have RemoveString.
      ZDOExtraData.s_strings.Remove(zdo.m_uid, ZDOVars.s_creatorName);
      ZDOExtraData.s_strings.Remove(zdo.m_uid, Hashes.XRaySteamName);
      ZDOExtraData.s_strings.Remove(zdo.m_uid, Hashes.XRaySteamID);
      zdo.RemoveLong(Hashes.XRayCreatedID);
    }
    else
      piece.SetCreator(Game.instance.GetPlayerProfile().GetPlayerID());
    if (Configuration.NoTarget && (piece.m_primaryTarget || piece.m_randomTarget))
    {
      zdo.Set(Hashes.HasFields, true);
      zdo.Set(Hashes.HasFieldsStaticTarget, true);
      if (piece.m_primaryTarget)
      {
        piece.m_primaryTarget = false;
        zdo.Set(Hashes.StaticTargetPrimaryTarget, false);
      }
      if (piece.m_randomTarget)
      {
        piece.m_randomTarget = false;
        zdo.Set(Hashes.StaticTargetRandomTarget, false);
      }
      view.LoadFields();
    }
    if (Configuration.NoPhysics && piece.TryGetComponent<StaticPhysics>(out var sp))
    {
      if (sp.m_fall || sp.m_pushUp)
      {
        zdo.Set(Hashes.HasFields, true);
        zdo.Set(Hashes.HasFieldsStaticPhysics, true);
        if (sp.m_fall)
        {
          sp.m_fall = false;
          zdo.Set(Hashes.StaticPhysicsFall, false);
        }
        if (sp.m_pushUp)
        {
          sp.m_pushUp = false;
          zdo.Set(Hashes.StaticPhysicsPushUp, false);
        }
        view.LoadFields();
      }
    }
    if (Configuration.NoRemove)
    {
      zdo.Set(Hashes.HasFields, true);
      zdo.Set(Hashes.HasFieldsPiece, true);
      if (piece.m_primaryTarget)
        zdo.Set(Hashes.PieceCanBeRemoved, false);
      piece.m_canBeRemoved = false;
      view.LoadFields();
    }
  }
}
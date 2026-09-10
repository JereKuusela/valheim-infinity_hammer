using System.Collections.Generic;
using System.Linq;

namespace InfinityHammer;

///<summary>Sums the resource cost of a group selection so vanilla HaveRequirements/ConsumeResources can handle it.</summary>
public static class ResourceCost
{
  public static Piece.Requirement[] Calculate(List<SelectedObject> objects)
  {
    Dictionary<ItemDrop, Piece.Requirement> totals = [];
    foreach (var obj in objects)
    {
      var prefab = ZNetScene.instance.GetPrefab(obj.Prefab);
      if (!prefab || !prefab.TryGetComponent<Piece>(out var piece) || piece.m_resources == null) continue;
      foreach (var requirement in piece.m_resources)
      {
        if (!requirement.m_resItem) continue;
        if (totals.TryGetValue(requirement.m_resItem, out var existing))
          existing.m_amount += requirement.m_amount;
        else
          totals[requirement.m_resItem] = new Piece.Requirement
          {
            m_resItem = requirement.m_resItem,
            m_amount = requirement.m_amount,
            m_recover = requirement.m_recover
          };
      }
    }
    return [.. totals.Values];
  }
}

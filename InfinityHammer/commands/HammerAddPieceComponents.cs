using System;
using ServerDevcommands;
namespace InfinityHammer;
public class HammerAddPieceComponentsCommand
{
  public HammerAddPieceComponentsCommand()
  {
    AutoComplete.RegisterEmpty("hammer_add_piece_components");
    Helper.Command("hammer_add_piece_components", "Adds the Piece component to every prefab to allow copying them with PlanBuild.", (args) =>
    {
      HammerHelper.CheatCheck();
      foreach (var prefab in ZNetScene.instance.m_prefabs)
      {
        if (prefab.name == "Player") continue;
        if (prefab.name.StartsWith("_", StringComparison.Ordinal)) continue;
        if (prefab.name.StartsWith("fx_", StringComparison.Ordinal)) continue;
        if (prefab.name.StartsWith("sfx_", StringComparison.Ordinal)) continue;
        if (prefab.GetComponent<Piece>()) continue;
        var piece = prefab.AddComponent<Piece>();
        piece.m_name = Utils.GetPrefabName(piece.gameObject);
        piece.m_clipEverything = true;
      }
      foreach (var instance in ZNetScene.instance.m_instances.Values)
      {
        if (instance.gameObject.name == "Player(Clone)") continue;
        if (instance.gameObject.name.StartsWith("_", StringComparison.Ordinal)) continue;
        if (instance.gameObject.name.StartsWith("fx_", StringComparison.Ordinal)) continue;
        if (instance.gameObject.name.StartsWith("sfx_", StringComparison.Ordinal)) continue;
        if (instance.gameObject.GetComponent<Piece>()) continue;
        var piece = instance.gameObject.AddComponent<Piece>();
        piece.m_name = Utils.GetPrefabName(piece.gameObject);
        piece.m_clipEverything = true;
      }
      Helper.AddMessage(args.Context, "Piece component added to every prefab.");
    });
  }
}

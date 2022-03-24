using System;

namespace InfinityHammer {
  public class HammerAddPieceComponentsCommand {
    public HammerAddPieceComponentsCommand() {
      CommandWrapper.RegisterEmpty("hammer_add_piece_components");
      new Terminal.ConsoleCommand("hammer_add_piece_components", "Adds the Piece component to every prefab to allow copying them with PlanBuild.", delegate (Terminal.ConsoleEventArgs args) {
        if (!ZNetScene.instance) return;
        if (!Settings.IsCheats) {
          Helper.AddMessage(args.Context, "Error: Cheats are not allowed.");
          return;
        }
        foreach (var prefab in ZNetScene.instance.m_prefabs) {
          if (prefab.GetComponent<Piece>()) continue;
          if (prefab.name == "Player") continue;
          if (prefab.name.StartsWith("_", StringComparison.Ordinal)) continue;
          if (prefab.name.StartsWith("fx_", StringComparison.Ordinal)) continue;
          if (prefab.name.StartsWith("sfx_", StringComparison.Ordinal)) continue;
          var piece = prefab.AddComponent<Piece>();
          piece.m_name = Utils.GetPrefabName(piece.gameObject);
          piece.m_clipEverything = true;
        }
        foreach (var instance in ZNetScene.instance.m_instances.Values) {
          if (instance.gameObject.GetComponent<Piece>()) continue;
          if (instance.gameObject.name == "Player(Clone)") continue;
          if (instance.gameObject.name.StartsWith("_", StringComparison.Ordinal)) continue;
          if (instance.gameObject.name.StartsWith("fx_", StringComparison.Ordinal)) continue;
          if (instance.gameObject.name.StartsWith("sfx_", StringComparison.Ordinal)) continue;
          var piece = instance.gameObject.AddComponent<Piece>();
          piece.m_name = Utils.GetPrefabName(piece.gameObject);
          piece.m_clipEverything = true;
        }
        Helper.AddMessage(args.Context, "Piece component added to every prefab.");
      });
    }
  }
}

namespace InfinityHammer;
public class HammerUndoCommand {
  public HammerUndoCommand() {
    CommandWrapper.RegisterEmpty("hammer_undo");
    new Terminal.ConsoleCommand("hammer_undo", "Reverts object placing or removing.", (Terminal.ConsoleEventArgs args) => {
      if (!Player.m_localPlayer) return;
      if (!Settings.EnableUndo) return;
      UndoWrapper.Undo(args.Context);
    });
    CommandWrapper.RegisterEmpty("hammer_redo");
    new Terminal.ConsoleCommand("hammer_redo", "Restores reverted object placing or removing.", (Terminal.ConsoleEventArgs args) => {
      if (!Player.m_localPlayer) return;
      if (!Settings.EnableUndo) return;
      UndoWrapper.Redo(args.Context);
    });
  }
}

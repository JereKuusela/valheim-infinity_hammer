namespace InfinityHammer;
public class HammerUndoCommand {
  public HammerUndoCommand() {
    CommandWrapper.RegisterEmpty("hammer_undo");
    Helper.Command("hammer_undo", "Reverts object placing or removing.", (args) => {
      if (!Settings.EnableUndo) return;
      UndoWrapper.Undo(args.Context);
    });
    CommandWrapper.RegisterEmpty("hammer_redo");
    Helper.Command("hammer_redo", "Restores reverted object placing or removing.", (args) => {
      if (!Settings.EnableUndo) return;
      UndoWrapper.Redo(args.Context);
    });
  }
}

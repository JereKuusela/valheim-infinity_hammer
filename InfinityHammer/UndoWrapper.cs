using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InfinityHammer {
  public static class UndoWrapper {
    private static BindingFlags PrivateBinding = BindingFlags.Static | BindingFlags.NonPublic;
    private static BindingFlags PublicBinding = BindingFlags.Static | BindingFlags.Public;
    private static Type Type() => InfinityHammer.ServerDevcommands.GetType("ServerDevcommands.UndoManager");

    public static void Place(IEnumerable<ZNetView> objs) {
      if (!Settings.EnableUndo || objs.Count() == 0) return;
      var action = new UndoPlace(objs);
      if (InfinityHammer.IsServerDevcommands) {
        Type().GetMethod("Add", PrivateBinding).Invoke(null, new[] { action });
      } else UndoManager.Add(action);
    }
    public static void Remove(IEnumerable<UndoData> objs) {
      if (!Settings.EnableUndo || objs.Count() == 0) return;
      var action = new UndoRemove(objs);
      if (InfinityHammer.IsServerDevcommands) {
        Type().GetMethod("Add", PrivateBinding).Invoke(null, new[] { action });
      } else UndoManager.Add(action);
    }
    public static void Undo(Terminal terminal) {
      if (InfinityHammer.IsServerDevcommands) {
        Type().GetMethod("Undo", PublicBinding).Invoke(null, new[] { terminal });
      } else UndoManager.Undo(terminal);
    }
    public static void Redo(Terminal terminal) {
      if (InfinityHammer.IsServerDevcommands) {
        Type().GetMethod("Redo", PublicBinding).Invoke(null, new[] { terminal });
      } else UndoManager.Redo(terminal);
    }
  }
}

using System;
using System.Reflection;

namespace InfinityHammer {
  public static class UndoWrapper {
    private static BindingFlags PrivateBinding = BindingFlags.Static | BindingFlags.NonPublic;
    private static BindingFlags PublicBinding = BindingFlags.Static | BindingFlags.Public;
    private static Type Type() => InfinityHammer.DEV.GetType("DEV.UndoManager");
    public static void Place(ZNetView obj) {
      if (!Settings.EnableUndo || !obj) return;
      var action = new UndoPlace(obj);
      if (InfinityHammer.IsDev) {
        Type().GetMethod("Add", PrivateBinding).Invoke(null, new[] { action });
      } else UndoManager.Add(action);
    }
    public static void Remove(UndoData obj) {
      if (!Settings.EnableUndo || obj == null) return;
      var action = new UndoRemove(obj);
      if (InfinityHammer.IsDev) {
        Type().GetMethod("Add", PrivateBinding).Invoke(null, new[] { action });
      } else UndoManager.Add(action);
    }
    public static void Undo(Terminal terminal) {
      if (InfinityHammer.IsDev) {
        Type().GetMethod("Undo", PublicBinding).Invoke(null, new[] { terminal });
      } else UndoManager.Undo(terminal);
    }
    public static void Redo(Terminal terminal) {
      if (InfinityHammer.IsDev) {
        Type().GetMethod("Redo", PublicBinding).Invoke(null, new[] { terminal });
      } else UndoManager.Redo(terminal);
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace InfinityHammer;
public static class UndoWrapper
{
  private static BindingFlags PrivateBinding = BindingFlags.Static | BindingFlags.NonPublic;
  private static BindingFlags PublicBinding = BindingFlags.Static | BindingFlags.Public;
  private static Type Type() => CommandWrapper.ServerDevcommands!.GetType("ServerDevcommands.UndoManager");

  public static void Place(IEnumerable<ZDO> objs)
  {
    if (!Configuration.EnableUndo || objs.Count() == 0) return;
    UndoPlace action = new(objs);
    if (CommandWrapper.ServerDevcommands != null && Configuration.ServerDevcommandsUndo)
    {
      Type().GetMethod("Add", PrivateBinding).Invoke(null, new[] { action });
    }
    else UndoManager.Add(action);
  }
  public static void Remove(IEnumerable<ZDO> objs)
  {
    if (!Configuration.EnableUndo || objs.Count() == 0) return;
    UndoRemove action = new(objs);
    if (CommandWrapper.ServerDevcommands != null && Configuration.ServerDevcommandsUndo)
    {
      Type().GetMethod("Add", PrivateBinding).Invoke(null, new[] { action });
    }
    else UndoManager.Add(action);
  }
  public static void Undo(Terminal terminal)
  {
    if (CommandWrapper.ServerDevcommands != null && Configuration.ServerDevcommandsUndo)
    {
      Type().GetMethod("Undo", PublicBinding).Invoke(null, new[] { terminal });
    }
    else UndoManager.Undo(terminal);
  }
  public static void Redo(Terminal terminal)
  {
    if (CommandWrapper.ServerDevcommands != null && Configuration.ServerDevcommandsUndo)
    {
      Type().GetMethod("Redo", PublicBinding).Invoke(null, new[] { terminal });
    }
    else UndoManager.Redo(terminal);
  }
}

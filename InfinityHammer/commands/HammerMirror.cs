using System;
using ServerDevcommands;
namespace InfinityHammer;
public class HammerMirrorCommand
{
  public HammerMirrorCommand()
  {
    AutoComplete.RegisterEmpty("hammer_mirror");
    Helper.Command("hammer_mirror", " Mirrors the selection.", (args) =>
    {
      var selected = (ObjectSelection?)Selection.Get();
      if (selected == null) throw new InvalidOperationException("Mirroring only works for multiple objects.");
      selected.Mirror();
      Helper.AddMessage(args.Context, "Mirrored the selection.");
    });
  }
}

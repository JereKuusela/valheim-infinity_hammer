using System;
namespace InfinityHammer;
public class HammerMirrorCommand
{
  public HammerMirrorCommand()
  {
    CommandWrapper.RegisterEmpty("hammer_mirror");
    Helper.Command("hammer_mirror", " Mirrors the selection.", (args) =>
    {
      if (Selection.Type != SelectedType.Multiple) throw new InvalidOperationException("Mirroring only works for multiple objects.");
      Selection.Mirror();
      Helper.AddMessage(args.Context, "Mirrored the selection.");
    });
  }
}

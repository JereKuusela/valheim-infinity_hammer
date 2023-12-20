using ServerDevcommands;

namespace InfinityHammer;
public class HammerFreezeCommand
{
  public HammerFreezeCommand()
  {
    AutoComplete.RegisterEmpty("hammer_freeze");
    new Terminal.ConsoleCommand("hammer_freeze", "[true/false] - Toggles whether the mouse affects placement position.", (args) =>
    {
      if (args.Length > 1)
      {
        if (Parse.Boolean(args[1]) == true)
          Position.Freeze();
        else if (Parse.Boolean(args[1]) == false)
          Position.Unfreeze();
        else
          Position.ToggleFreeze();
      }
      else
        Position.ToggleFreeze();
    });
  }
}

namespace InfinityHammer;
public class HammerPlaceCommand
{
  public HammerPlaceCommand()
  {
    CommandWrapper.RegisterEmpty("hammer_place");
    Helper.Command("hammer_place", "Places the current object with a command.", (args) =>
    {
      Hammer.Place();
    });
  }
}

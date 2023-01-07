using BepInEx.Configuration;
using Service;
namespace InfinityHammer;

public partial class Configuration
{
#nullable disable
  public static ConfigEntry<int> hammerMenuTab;
  public static int HammerMenuTab => hammerMenuTab.Value;
  public static ConfigEntry<int> hammerMenuIndex;
  public static int HammerMenuIndex => hammerMenuIndex.Value;
  public static ConfigEntry<int> hoeMenuTab;
  public static int HoeMenuTab => hoeMenuTab.Value;
  public static ConfigEntry<int> hoeMenuIndex;
  public static int HoeMenuIndex => hoeMenuIndex.Value;
  public static ConfigEntry<string> commandDefaultSize;
  public static float CommandDefaultSize => ConfigWrapper.TryParseFloat(commandDefaultSize);
#nullable enable

  private static void InitCommands(ConfigWrapper wrapper)
  {
    var section = "6. Commands";
    commandDefaultSize = wrapper.Bind(section, "Command default size", "10", "Default size for commands.");
    commandDefaultSize.SettingChanged += (s, e) =>
    {
      Scaling.Command.SetScaleX(CommandDefaultSize);
      Scaling.Command.SetScaleZ(CommandDefaultSize);
    };
    Scaling.Command.SetScaleX(CommandDefaultSize);
    Scaling.Command.SetScaleZ(CommandDefaultSize);
    Scaling.Command.SetScaleY(0f);

    hammerMenuTab = wrapper.Bind(section, "Hammer menu tab", 0, "Index of the menu tab.");
    hammerMenuIndex = wrapper.Bind(section, "Hammer menu index", 1, "Index on the menu.");
    hoeMenuTab = wrapper.Bind(section, "Hoe menu tab", 0, "Index of the menu tab.");
    hoeMenuIndex = wrapper.Bind(section, "Hoe menu index", 5, "Index on the menu.");
  }
}
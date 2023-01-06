using BepInEx.Configuration;
using Service;
namespace InfinityHammer;

public partial class Configuration
{
#nullable disable
  public static ConfigEntry<int> configHammerMenuTab;
  public static int HammerMenuTab => configHammerMenuTab.Value;
  public static ConfigEntry<int> configHammerMenuIndex;
  public static int HammerMenuIndex => configHammerMenuIndex.Value;
  public static ConfigEntry<int> configHoeMenuTab;
  public static int HoeMenuTab => configHoeMenuTab.Value;
  public static ConfigEntry<int> configHoeMenuIndex;
  public static int HoeMenuIndex => configHoeMenuIndex.Value;
  public static ConfigEntry<string> configCommandDefaultSize;
  public static float CommandDefaultSize => ConfigWrapper.TryParseFloat(configCommandDefaultSize);
#nullable enable

  private static void InitCommands(ConfigWrapper wrapper)
  {
    var section = "6. Commands";
    configCommandDefaultSize = wrapper.Bind(section, "Command default size", "10", "Default size for commands.");
    configCommandDefaultSize.SettingChanged += (s, e) =>
    {
      Scaling.Command.SetScaleX(CommandDefaultSize);
      Scaling.Command.SetScaleZ(CommandDefaultSize);
    };
    Scaling.Command.SetScaleX(CommandDefaultSize);
    Scaling.Command.SetScaleZ(CommandDefaultSize);
    Scaling.Command.SetScaleY(0f);

    configHammerMenuTab = wrapper.Bind(section, "Hammer menu tab", 0, "Index of the menu tab.");
    configHammerMenuIndex = wrapper.Bind(section, "Hammer menu index", 1, "Index on the menu.");
    configHoeMenuTab = wrapper.Bind(section, "Hoe menu tab", 0, "Index of the menu tab.");
    configHoeMenuIndex = wrapper.Bind(section, "Hoe menu index", 5, "Index on the menu.");
  }
}
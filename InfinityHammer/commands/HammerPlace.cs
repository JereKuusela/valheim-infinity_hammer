using UnityEngine;

namespace InfinityHammer {
  public class HammerPlaceCommand {
    public HammerPlaceCommand() {
      CommandWrapper.RegisterEmpty("hammer_place");
      new Terminal.ConsoleCommand("hammer_place", "Places the current object with a command.", delegate (Terminal.ConsoleEventArgs args) {
        if (!Player.m_localPlayer) return;
        Player.m_localPlayer.m_placePressedTime = Time.time;
        Player.m_localPlayer.m_lastToolUseTime = 0f;
        Player.m_localPlayer.UpdatePlacement(true, 0f);
      });
    }
  }
}

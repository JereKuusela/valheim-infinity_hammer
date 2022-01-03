using System.Globalization;

namespace InfinityHammer {
  public static class Helper {

    public static void RemoveZDO(ZDO zdo) {
      if (!zdo.IsOwner())
        zdo.SetOwner(ZDOMan.instance.GetMyID());
      if (ZNetScene.instance.m_instances.TryGetValue(zdo, out var view))
        ZNetScene.instance.Destroy(view.gameObject);
      else
        ZDOMan.instance.DestroyZDO(zdo);
    }

    public static void CopyData(ZDO from, ZDO to) {
      to.m_floats = from.m_floats;
      to.m_vec3 = from.m_vec3;
      to.m_quats = from.m_quats;
      to.m_ints = from.m_ints;
      to.m_longs = from.m_longs;
      to.m_strings = from.m_strings;
      to.m_byteArrays = from.m_byteArrays;
      to.IncreseDataRevision();
    }

    public static float ParseFloat(string value, float defaultValue = 0) {
      if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)) return result;
      return defaultValue;
    }

    public static void AddMessage(Terminal context, string message) {
      context.AddString(message);
      Player.m_localPlayer?.Message(MessageHud.MessageType.TopLeft, message);
    }
  }
}
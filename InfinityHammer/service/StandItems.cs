using System.Collections.Generic;
using Data;

namespace InfinityHammer;

internal static class StandItems
{
  // Blueprint extra info remains readable and accepts numeric hashes when a prefab is missing.
  internal static int Hash(string name) => string.IsNullOrEmpty(name) ? 0 : int.TryParse(name, out var hash) ? hash : name.GetStableHashCode();
  internal static string Name(int hash) => hash == 0 ? "" : ZNetScene.instance.GetPrefab(hash)?.name ?? hash.ToString(System.Globalization.CultureInfo.InvariantCulture);

  internal static bool TryGet(DataEntry data, Dictionary<string, string> pars, int key, out int hash)
  {
    if (data.TryGetHash(pars, key, out hash) || data.TryGetInt(pars, key, out hash)) return true;
    if (!data.TryGetString(pars, key, out var name)) return false;
    hash = Hash(name);
    data.Set(key, hash);
    data.Strings?.Remove(key);
    return true;
  }
}

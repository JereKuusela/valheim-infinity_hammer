using System.Text.Json.Serialization;
using System;
using BepInEx;

namespace Argo.Blueprint.Util;

/// <summary>
/// Best use the same values specified for [BepInPlugin(GUID, NAME, VERSION)] if your mod is based
/// on BepinEx. You can simply pass e.g. your derived class to the constructor  
/// </summary>
public struct ModIdentifier : IEquatable<ModIdentifier>, IComparable<ModIdentifier>
{
    [JsonInclude]                           public  string          GUID ;
    [JsonInclude] [JsonPropertyName("mod")] public          string          Name        = "";
    [JsonInclude]                           public          System.Version? Version     = null;
    [JsonInclude]                           public          string          Url         = "";
    [JsonInclude]                           public          string          Description = "";
    public ModIdentifier(BepInEx.PluginInfo pluginInfo, string url = "", string description = "")
        : this(pluginInfo.Metadata.GUID, pluginInfo.Metadata, url, description) { }
    public ModIdentifier(string guid, BepInPlugin pluginInfo, string url = "", string description = "") : this(guid) {
        Name        = pluginInfo.Name;
        Version     = pluginInfo.Version;
        Url         = url;
        Description = description;
    }
    public ModIdentifier(string guid, string name, System.Version version, string url = "", string description = "") : this(guid) {
        Name        = name;
        Version     = version;
        Url         = url;
        Description = description;
 
    }
    public ModIdentifier(string guid) {
        if (string.IsNullOrEmpty(guid))
            throw new ArgumentException("GUID may not be empty. The GUID usually refers to your mod name.\n" +
                "See BepInEx documentation for BepInPlugin for more info..");
        GUID        = guid;
 
    }
    public ModIdentifier(
        BepInEx.BaseUnityPlugin bepInExPlugin, string url = "", string description = "")
        : this(bepInExPlugin.Info.Metadata.GUID, bepInExPlugin.Info.Metadata, url, description) { }
    public bool Equals(ModIdentifier other) {
        var ret = string.Equals(GUID, other.GUID, StringComparison.OrdinalIgnoreCase) &&
            Version == other.Version;
        return ret;
    }

    public override bool Equals(object           other) { throw new NotImplementedException(); }
    public          int  CompareTo(ModIdentifier other) { throw new NotImplementedException(); }
}
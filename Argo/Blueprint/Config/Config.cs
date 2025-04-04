using System.Collections.Generic;
using Argo.Blueprint.Util;
using Argo.Util;
using Argo.Zdo;

namespace Argo.Blueprint;

public enum CategorySettings : byte
{
    Include      = 1,
    Exclude      = 2, // todo change to DoNotInclude or Ignore
    ForceExclude = 3
}

public static class CategorySettingsStrings
{
    public const string Include = "Include";
    public const string
        Exclude = "Exclude"; // todo change to DoNotInclude or Ignore
    public const string ForceExclude = "ForceExclude";
}

public class Config : ObjectPool<Config>
{
    static         Dictionary<string, Config> _config       = [];
    private static string                     currentConfig = "default";

    static Config() { }
    private   string           configname;
    protected BuilderRegister?     Register;
    protected HashRegister?          HashRegisterLookup;
    protected ExtraDataConfig? ExtraDataConfig;
    protected PrefabRegister?  Prefabs;
    protected SaveExtraData    SaveExtraData = SaveExtraData.NoSteamData;
    private Config(string name) : base(false) {
        configname = name;
        Register   = null;
        HashRegisterLookup = null;
    }
    protected Config(Config other) : base(false) {
        Register      = other.Register;
        HashRegisterLookup    = other.HashRegisterLookup;
        SaveExtraData = other.SaveExtraData;
    }
    private Config(
        BuilderRegister    register, HashRegister hashRegisterLookup,
        PrefabRegister prefabs,
        SaveExtraData  saveExtraData = SaveExtraData.NoSteamData) : base(false) {
        Register      = register;
        HashRegisterLookup    = hashRegisterLookup;
        SaveExtraData = saveExtraData;
        Prefabs       = prefabs;
    }
    protected static Config CreateCommmonInstance() {
        var Register   = BuilderRegister.GetDefault().Clone();
        var HashLookup = HashRegister.GetDefault().Clone();
        var prefabs    = PrefabRegister.GetDefault().Clone();

        return new Config(Register, HashLookup, prefabs);
    }

    protected static Config CreateDefaultInstance() {
        var Register      = BuilderRegister.Default;
        var Prefabs       = PrefabRegister.GetDefault();
        var HashLookup    = HashRegister.GetDefault();
        var SaveExtraData = ExtraDataConfig.FromConfig();
        return new Config(Register, HashLookup, Prefabs, SaveExtraData);
    }

    public static HashRegister GetHashLookup() {
        switch (currentConfig) {
            case "default": return HashRegister.GetDefault();
            case "common":  return HashRegister.GetCommon();
            default:        throw new System.NotImplementedException(); // todo
        }
    }
    public override Config Clone() {
        throw new System.NotImplementedException();
    }
    public override void AddToMod(ModIdentifier mod, params object[] pairs) {
        throw new System.NotImplementedException();
    }
}
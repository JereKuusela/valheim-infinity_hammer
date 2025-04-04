using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Argo.Blueprint;

namespace Argo.Zdo;

public enum SaveExtraData
{
    None         = 0,
    Filtered     = 1,
    All          = 2,
    NoPlayerData = 4,
    NoSteamData  = 5,
    Reduced      = 6,
    Custom = 7,
}


public static class SaveExtraDataConfig
{
    public static SaveExtraData Save = SaveExtraData.None;
    static SaveExtraDataConfig() {
        switch (Argonaut.SaveData) {
            case ("None"):         Save = SaveExtraData.None; break;
            case ("Filtered"):     Save = SaveExtraData.None; break;
            case ("All"):          Save = SaveExtraData.None; break;
            case ("NoPlayerData"): Save = SaveExtraData.None; break;
            case ("NoSteamData"):  Save = SaveExtraData.None; break;
            case ("Reduced"):      Save = SaveExtraData.None; break;
        }
    }
}

public class ExtraDataConfig
{
    public HashSet<int> Filtered;
    public HashSet<int> NoPlayerData;
    public HashSet<int> NoSteamData;
    public HashSet<int> Reduced;
    public static readonly string[]        AllowedValues    = GetAllowedValues();

    public static readonly ExtraDataConfig StandartInstance = new ExtraDataConfig();
    public static          ExtraDataConfig DefaultInstance  = new ExtraDataConfig();
    public ExtraDataConfig() {
        var info = ZDOInfo.GetInstance();
        Filtered    = new HashSet<int> { };
        NoSteamData = new HashSet<int> { ZDOVars.s_authorDisplayName, ZDOVars.s_author };
        NoPlayerData =
            new HashSet<int> { ZDOVars.s_creator, ZDOVars.s_crafterName, ZDOVars.s_crafterID }
               .Union(NoSteamData).ToHashSet();

        Reduced = new HashSet<int> { ZDOVars.s_support }.Union(NoPlayerData).ToHashSet();
    }
    public HashSet<int> get(SaveExtraData save) {
        switch (save) {
            case SaveExtraData.Reduced:
                return Reduced;
            case SaveExtraData.Filtered:
                return Filtered;
            case SaveExtraData.NoPlayerData:
                return NoPlayerData;
            case SaveExtraData.NoSteamData:
                return Reduced;
            default:
                return new HashSet<int>();
        }
    }
    public static SaveExtraData FromConfig() {
        SaveExtraData Save;
        switch (Argonaut.SaveData) {
            case ("None"):         Save = SaveExtraData.None; break;
            case ("Filtered"):     Save = SaveExtraData.Filtered; break;
            case ("All"):          Save = SaveExtraData.All; break;
            case ("NoPlayerData"): Save = SaveExtraData.NoPlayerData; break;
            case ("NoSteamData"):  Save = SaveExtraData.NoSteamData; break;
            case ("Reduced"):      Save = SaveExtraData.Reduced; break;
            case ("Custom"):       Save = SaveExtraData.Custom; break;
            default:             Save = SaveExtraData.NoSteamData; break;
        }
        return Save;
    }
    public static string[] GetAllowedValues() {
        FieldInfo[] fields = typeof(SaveExtraData).GetFields();
        string[] ret = new string[fields.Length];
        for (int i = 0; i < fields.Length; i++) {
            ret[i] = fields[i].Name;
        }
        return ret;
    }
}
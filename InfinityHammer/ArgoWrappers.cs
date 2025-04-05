using System;
using System.Collections.Generic;
using System.Linq;
using Argo.Blueprint;
using Data;
using UnityEngine;
using Argo.Blueprint;
using Argo.DataAnalysis;
using Argo.Zdo;
using UnityEngine.Assertions;
using Debug = System.Diagnostics.Debug;
using Object = System.Object;

namespace InfinityHammer;

using static Argo.Blueprint.BpjZVars;

public class ExtrDataInf : ExtraDataArgo
{
    public static readonly Dictionary<string, string> pars = new();
    public static Config m_config { get; set; } = Config.GetCurrentConfig();
    public ExtrDataInf() { }
    public ExtrDataInf(DataEntry? data) : this(data, Config.GetCurrentConfig()) { }
    public ExtrDataInf(DataEntry? data, Config mConfig) {
        m_config = mConfig;
        if (mConfig.SaveMode  != SaveExtraData.None) {
            var set = mConfig.Filter.Get();
            Func<int, bool> filter = (x) => {
                if (set.Contains(x)) return false;
                return true;
            };
            if (data != null) {
                s_floats = import<IFloatValue, float, float?>(data.Floats, filter,
                    x => x.Get(pars));
                s_ints  = import<IIntValue, int, int?>(data.Ints, filter, x => x.Get(pars));
                s_longs = import<ILongValue, long, long?>(data.Longs, filter, x => x.Get(pars));
                s_strings = import<IStringValue, string, string?>(data.Strings, filter,
                    x => x.Get(pars));
                s_vec3s = import<IVector3Value, Vector3, Vector3?>(data.Vecs, filter,
                    x => x.Get(pars));
                s_quats = import<IQuaternionValue, Quaternion, Quaternion?>(data.Quats, filter,
                    x => x.Get(pars));
                s_byteArrays = import<byte[], byte[], byte[]?>(data.ByteArrays, filter, x => x);
            }
        }
    }

    static Dictionary<int, U>? import<T, U, V>(
        IEnumerable<KeyValuePair<int, T>>? import,
        Func<int, bool>                    filter,
        Func<T, V>                         getter) {
        // var type = GetVType<T>();
        if (import != null) {
            var values = new Dictionary<int, U>();
            import.Select(x => x).Aggregate(
                values,
                (target, pair) => {
                    if (filter(pair.Key)) {
                        var val = getter(pair.Value);
                        if (val != null) {
                            target[pair.Key] = (U)(object)val;
                        }
                    }
                    return target;
                }
            );
            if (values.Count > 0) {
                return values;
            }
        }
        return null;
    }

    

    public override AExtraData Create() { return new ExtrDataInf(); }
}

public static class ExtrDataInfExt
{
    // todo cleanup, those methods arent needed in both classes
    public static readonly Dictionary<string, string> pars = new();

    public static Dictionary<int, U>? TodZdoVars<T, U>(this ExtraDataArgo? data) {
        if (data == null) return null;
        return data.s_floats?.ToDictionary(e => e.Key, e => (U)(object)e.Value);
    }
    public static DataEntry ToDataEntry(this ExtraDataArgo? data) {
        DataEntry datanew = new();
        datanew.Floats
            = data.s_floats?.ToDictionary(e => e.Key,
                e => (IFloatValue)new SimpleFloatValue(e.Value));
        datanew.Ints = data.s_ints?.ToDictionary(e => e.Key,
            e => (IIntValue)new SimpleIntValue(e.Value));
        datanew.Longs
            = data.s_longs?.ToDictionary(e => e.Key, e => (ILongValue)new SimpleLongValue(e.Value));
        datanew.Strings = data.s_strings?.ToDictionary(e => e.Key,
            e => (IStringValue)new SimpleStringValue(e.Value));
        datanew.Vecs = data.s_vec3s?.ToDictionary(e => e.Key,
            e => (IVector3Value)new SimpleVector3Value(e.Value));
        datanew.Quats = data.s_quats?.ToDictionary(e => e.Key,
            e => (IQuaternionValue)new SimpleQuaternionValue(e.Value));
        datanew.ByteArrays = data.s_byteArrays?.ToDictionary(e => e.Key, e => e.Value);

        return datanew;
    }
    public static DataEntry ToDataEntry(this ExtraDataValheim? data) {
        DataEntry datanew = new();
        datanew.Floats
            = data.s_floats?.ToDictionary(e => e.Key,
                e => (IFloatValue)new SimpleFloatValue(e.Value));
        datanew.Ints = data.s_ints?.ToDictionary(e => e.Key,
            e => (IIntValue)new SimpleIntValue(e.Value));
        datanew.Longs
            = data.s_longs?.ToDictionary(e => e.Key, e => (ILongValue)new SimpleLongValue(e.Value));
        datanew.Strings = data.s_strings?.ToDictionary(e => e.Key,
            e => (IStringValue)new SimpleStringValue(e.Value));
        datanew.Vecs = data.s_vec3s?.ToDictionary(e => e.Key,
            e => (IVector3Value)new SimpleVector3Value(e.Value));
        datanew.Quats = data.s_quats?.ToDictionary(e => e.Key,
            e => (IQuaternionValue)new SimpleQuaternionValue(e.Value));
        datanew.ByteArrays = data.s_byteArrays?.ToDictionary(e => e.Key, e => e.Value);

        return datanew;
    }

    public static ExtraDataArgo ToExtrDataArgo(this DataEntry data) =>
        ToExtrDataArgo(data, SaveExtraDataConfig.Save);
     public static ExtraDataArgo ToExtrDataArgo(this DataEntry data, SaveExtraData save) {
         var newdata = new ExtraDataArgo();
        if (save != SaveExtraData.None) {
            var set =  Config.GetCurrentConfig().Filter.Get(save);
            Func<int, bool> filter = (x) => {
                if (set.Contains(x)) return false;
                return true;
            };
            if (data != null) {
                newdata.s_floats = import<IFloatValue, float, float?>(data.Floats, filter,
                    x => x.Get(pars));
                newdata.s_ints  = import<IIntValue, int, int?>(data.Ints, filter, x => x.Get(pars));
                newdata.s_longs = import<ILongValue, long, long?>(data.Longs, filter, x => x.Get(pars));
                newdata.s_strings = import<IStringValue, string, string?>(data.Strings, filter,
                    x => x.Get(pars));
                newdata.s_vec3s = import<IVector3Value, Vector3, Vector3?>(data.Vecs, filter,
                    x => x.Get(pars));
                newdata.s_quats = import<IQuaternionValue, Quaternion, Quaternion?>(data.Quats, filter,
                    x => x.Get(pars));
                newdata.s_byteArrays = import<byte[], byte[], byte[]?>(data.ByteArrays, filter, x => x);
            }
        }
        return newdata;
    }

    static Dictionary<int, U>? import<T, U, V>(
        IEnumerable<KeyValuePair<int, T>>? import,
        Func<int, bool>                    filter,
        Func<T, V>                         getter) {
        // var type = GetVType<T>();
        if (import != null) {
            var values = new Dictionary<int, U>();
            import.Select(x => x).Aggregate(
                values,
                (target, pair) => {
                    if (filter(pair.Key)) {
                        var val = getter(pair.Value);
                        if (val != null) {
                            target[pair.Key] = (U)(object)val;
                        }
                    }
                    return target;
                }
            );
            if (values.Count > 0) {
                return values;
            }
        }
        return null;
    }
}

public class SelectedObjects : SelectionBase
{
    protected GameObject m_placementGhost;

    static readonly Dictionary<string, string> pars = new();
    public SelectedObjects(GameObject? placementGhost_, ObjectSelection selection) :
        base(Utility.GetChildren(placementGhost_),
            selection.Objects.Select((x) => (AExtraData)new ExtrDataInf(x.Data)).ToList(),
            placementGhost_?.transform.position ?? Vector3.zero, placementGhost_?.transform.rotation.eulerAngles ?? Vector3.zero) {
        m_placementGhost
            = placementGhost_ ?? throw new ArgumentNullException("No objects selected.");
        var piece = placementGhost_.GetComponent<Piece>();
        try {
            if (placementGhost_) {
                m_Name = piece
                    ? Localization.instance.Localize(piece.m_name)
                    : Utils.GetPrefabName(placementGhost_);
            } else {
                m_Name = "";
            }
        } catch (Exception e) {
            m_Name = "";
            System.Console.WriteLine("ArgoWrapper: Error in GetPrefabName" + e);
        }
    }

    public virtual GameObject PlacementGhost {
        get => m_placementGhost;
        set => m_placementGhost = value;
    }

    public override List<GameObject> GetSnapPoints() => Utility.GetSnapPoints(m_placementGhost);
}
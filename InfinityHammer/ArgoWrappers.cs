using System;
using System.Collections.Generic;
using System.Linq;
using Argo.blueprint;
using Data;
using UnityEngine;
using Argo.Blueprint;
using Argo.DataAnalysis;
using UnityEngine.Assertions;
using Debug = System.Diagnostics.Debug;
using Object = System.Object;

namespace InfinityHammer;

using static Argo.Blueprint.BpjZVars;

public class InfExtraData : AExtraData
{
    public override Dictionary<string, ZValue> import(Func<int, bool> filter) {
        throw new NotImplementedException();
    }
    protected override Dictionary<T, U> GetDic<T, U>(T value) {
        throw new NotImplementedException();
    }
    static U? Make<T, U>(T value) {
        Debug.Assert( value != null, nameof(value) + " != null" );
        if ((typeof(U) == typeof(IStringValue)) && (typeof(T) == typeof(string))) {
            IStringValue val = new SimpleStringValue( (string)(object)value );
            return (U)val;
        }
        if ((typeof(U) == typeof(IFloatValue)) && (typeof(T) == typeof(float))) {
            IFloatValue val = new SimpleFloatValue( (float)(object)value );
            return (U)val;
        }
        if ((typeof(U) == typeof(IIntValue)) && (typeof(T) == typeof(int))) {
            IIntValue val = new SimpleIntValue( (int)(object)value );
            return (U)val;
        }
        if ((typeof(U) == typeof(ILongValue)) && (typeof(T) == typeof(long))) {
            ILongValue val = new SimpleLongValue( (long)(object)value );
            return (U)val;
        }
        if ((typeof(U) == typeof(IVector3Value)) && (typeof(T) == typeof(Vector3))) {
            IVector3Value val = new SimpleVector3Value( (Vector3)(object)value );
            return (U)val;
        }
        if ((typeof(U) == typeof(IQuaternionValue)) && (typeof(T) == typeof(Quaternion))) {
            IQuaternionValue val = new SimpleQuaternionValue( (Quaternion)(object)value );
            return (U)val;
        }
        if ((typeof(U) == typeof(byte[])) && (typeof(T) == typeof(byte[]))) {
            return (U)(object)value;
        }
        throw new InvalidOperationException(
            $"Unsupported conversion from {typeof(T)} to {typeof(U)}" );
    }
    static readonly Dictionary<string, string> pars = new();
    static U? Get<T, U>(T value) {
        if ((typeof(T) == typeof(IStringValue)) && (typeof(U) == typeof(string))) {
            IStringValue val = (IStringValue)value;
            string val2 = val.Get( pars );
            return (U)(Object) val2;
        }
        if ((typeof(T) == typeof(IFloatValue)) && (typeof(U) == typeof(float))) {
            IFloatValue val  = (IFloatValue)value;
            float?      val2 = val.Get( pars );
            return (U)(Object) val2;
        }
        if ((typeof(T) == typeof(IIntValue)) && (typeof(U) == typeof(int))) {
            IIntValue val = (IIntValue)value;
            return (U)(object)val.Get( pars );
        }
        if ((typeof(T) == typeof(ILongValue)) && (typeof(U) == typeof(long))) {
            ILongValue val = (ILongValue)value;
            return (U)(object)val.Get( pars );
        }
        if ((typeof(T) == typeof(IVector3Value)) && (typeof(U) == typeof(Vector3))) {
            IVector3Value val = (IVector3Value)value;
            return (U)(object)val.Get( pars );
        }
        if ((typeof(T) == typeof(IQuaternionValue)) && (typeof(U) == typeof(Quaternion))) {
            IQuaternionValue val = (IQuaternionValue)value;
            return (U)(object)val.Get( pars );
        }
        if ((typeof(T) == typeof(byte[])) && (typeof(U) == typeof(byte[]))) {
            return (U)(object)value;
        }
        throw new InvalidOperationException(
            $"Unsupported conversion from {typeof(T)} to {typeof(U)}" );
    }
    static Dictionary<string, U> readvals<T, U>(
        IEnumerable<KeyValuePair<int, T>>? import,
        ZDOInfo                            info,
        Dictionary<string, U>              values) {
        var type = GetVType<T>();
        if (import != null) {
            import.Select( x => x ).Aggregate(
                values,
                (target, pair) => {
                    GetName( pair.Key, out var name, info );
                    U? val = Get<T, U>( pair.Value );
                    if (val != null) {
                        target[name] = val;
                    }
                    return target;
                }
            );
        }
        return values;
    }
    static Dictionary<int, U> export<T, U>(
        Dictionary<string, ZValue> import,
        Func<ZType, bool>          filter) {
        var                type   = GetVType<T>();
        Dictionary<int, U> export = new();
        if (import != null) {
            export = import.Select( x => x ).Aggregate(
                export,
                (target, pair) => {
                    if (filter( pair.Value.Type )) {
                        int hash = pair.Value.Name.GetStableHashCode();
                        U?  val  = Make<T, U>( (T)pair.Value.Value );
                        if (val != null) {
                            target[hash] = val;
                        }
                    }
                    return target;
                }
            );
        }
        return export;
    }
    public static DataEntry ToDataEntry(BpjZVars zvars) {
        var values = zvars.m_values;
        DataEntry data = new();
        data.Floats     = export<float, IFloatValue>( values, (x) => x == ZType.Float );
        data.Ints       = export<int, IIntValue>( values, (x) => x == ZType.Int );
        data.Longs      = export<long, ILongValue>( values, (x) => x == ZType.Long );
        data.Strings    = export<string, IStringValue>( values, (x) => x == ZType.String );
        data.Vecs       = export<Vector3, IVector3Value>( values, (x) => x == ZType.Vec3 );
        data.Quats      = export<Quaternion, IQuaternionValue>( values, (x) => x == ZType.Quat );
        data.ByteArrays = export<byte[], byte[]>( values, (x) => x == ZType.ByteArray );
        return data;
    }
    static Dictionary<string, ZValue> import<T, U>(
        IEnumerable<KeyValuePair<int, T>>? import,
        Dictionary<string, ZValue>         values,
        ZDOInfo                            info,
        Func<int, bool>                    filter) {
       // var type = GetVType<T>();
        if (import != null) {
            import.Select( x => x ).Aggregate(
                values,
                (target, pair) => {
                    if (filter( pair.Key )) {
                        bool unknown = !GetName( pair.Key, out var name, info );
                        var   val     = Get<T, U>( pair.Value );
                        if (val != null) {
                            target[name] = ZValue.Create<U>( name, val, unknown );
                        }
                    }
                    return target;
                }
            );
        }
        return values;
    }
    public static List<BpjZVars> ToZvars(
        List<SelectedObject> selectedObjects
    ) => ToZvars( selectedObjects, (x) => true );
    public static List<BpjZVars> ToZvars(
        List<SelectedObject> selectedObjects,
        Func<int, bool>      filter) {
        List<BpjZVars> ZVars = [];
        ZDOInfo        info  = ZDOInfo.Instance;

        foreach (var selected in selectedObjects) {
            DataEntry data = selected.Data;
            {
                Dictionary<string, string> pars   = new Dictionary<string, string>();
                Dictionary<string, ZValue> values = new Dictionary<string, ZValue>();
                values = import<IFloatValue, float>( data.Floats, values, info, filter );
                values = import<IIntValue, int>( data.Ints, values, info, filter );
                values = import<ILongValue, long>( data.Longs, values, info, filter );
                values = import<IStringValue, string>( data.Strings, values, info, filter );
                values = import<IVector3Value, Vector3>( data.Vecs, values, info, filter );
                values = import<IQuaternionValue, Quaternion>( data.Quats, values, info,
                    filter );
                values = import<byte[], byte[]>( data.ByteArrays, values, info, filter );
                ZVars.Add( new(values) );
            }
        }
        return ZVars;
    }
}

public class SelectedObjects : SelectionBase
{
    protected GameObject m_placementGhost;

    static readonly Dictionary<string, string> pars = new();
    public SelectedObjects(GameObject? placementGhost_, ObjectSelection selection) :
        base( Util.GetChildren( placementGhost_ ), InfExtraData.ToZvars( selection.Objects ),
            placementGhost_.transform.position, placementGhost_.transform.rotation.eulerAngles ) {
        m_placementGhost
            = placementGhost_ ?? throw new ArgumentNullException( "No objects selected." );
        var piece = placementGhost_.GetComponent<Piece>();
        try {
            if (placementGhost_) {
                m_Name = piece
                    ? Localization.instance.Localize( piece.m_name )
                    : Utils.GetPrefabName( placementGhost_ );
            } else {
                m_Name = "";
            }
        } catch (Exception e) {
            m_Name = "";
            System.Console.WriteLine( "ArgoWrapper: Error in GetPrefabName" + e );
        }
    }

    public virtual GameObject PlacementGhost {
        get => m_placementGhost;
        set => m_placementGhost = value;
    }

    public override List<GameObject> GetSnapPoints() => Util.GetSnapPoints( m_placementGhost );
}
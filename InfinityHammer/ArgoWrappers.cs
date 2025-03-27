using System;
using System.Collections.Generic;
using System.Linq;
using Argo.blueprint;
using Data;
using UnityEngine;
using Argo.Blueprint;
using Argo.DataAnalysis;

namespace InfinityHammer;

using static Argo.Blueprint.BpjZVars;

public class InfExtraData : AExtraData
{
    public override Dictionary<string, ZValue> readFiltered(Func<int, bool> filter) {
        throw new NotImplementedException();
    }
    protected override Dictionary<T, U> GetDic<T, U>(T value) {
        throw new NotImplementedException();
    }
    static readonly Dictionary<string, string> pars=new();
    static U? Get<T, U>(T value) {
        if ((typeof(T) == typeof(IStringValue)) && (typeof(U) == typeof(string))) {
            IStringValue val=(IStringValue)value;
            return (U)(object)val.Get( pars );
        }
        if ((typeof(T) == typeof(IFloatValue)) && (typeof(U) == typeof(float))) {
            IFloatValue val=(IFloatValue)value;
            return (U)(object)val.Get( pars );
        }
        if ((typeof(T) == typeof(IIntValue)) && (typeof(U) == typeof(int))) {
            IIntValue val=(IIntValue)value;
            return (U)(object)val.Get( pars );
        }
        if ((typeof(T) == typeof(ILongValue)) && (typeof(U) == typeof(long))) {
            ILongValue val=(ILongValue)value;
            return (U)(object)val.Get( pars );
        }
        if ((typeof(T) == typeof(IVector3Value)) && (typeof(U) == typeof(Vector3))) {
            IVector3Value val=(IVector3Value)value;
            return (U)(object)val.Get( pars );
        }
        if ((typeof(T) == typeof(IQuaternionValue)) && (typeof(U) == typeof(Quaternion))) {
            IQuaternionValue val=(IQuaternionValue)value;
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
        var type=GetVType<T>();
        if (import != null) {
            import.Select( x => x ).Aggregate(
                values,
                (target, pair) => {
                    GetName( pair.Key, out var name, info );
                    U? val=Get<T, U>( pair.Value );
                    if (val != null) {
                        target[name]=val;
                    }
                    return target;
                }
            );
        }
        return values;
    }
    static Dictionary<string, ZValue> readFiltered<T, U>(
        IEnumerable<KeyValuePair<int, T>>? import,
        Dictionary<string, ZValue>         values,
        ZDOInfo                            info,
        Func<int, bool>                    filter) {
        var type=GetVType<T>();
        if (import != null) {
            import.Select( x => x ).Aggregate(
                values,
                (target, pair) => {
                    if (filter( pair.Key )) {
                        GetName( pair.Key, out var name, info );
                        U? val=Get<T, U>( pair.Value );
                        if (val != null) {
                            target[name]=ZValue.Create( name, val );
                        }
                    }
                    return target;
                }
            );
        }
        return values;
    }
    static List<BpjZVars> ToZvars(
        List<SelectedObject> selectedObjects,
        Func<int, bool>      filter) {
        List<BpjZVars> ZVars= [];
        ZDOInfo        info =ZDOInfo.Instance;

        foreach (var selected in selectedObjects) {
            DataEntry data=selected.Data;
            {
                Dictionary<string, string> pars  =new Dictionary<string, string>();
                Dictionary<string, ZValue> values=new Dictionary<string, ZValue>();
                values=readFiltered<IFloatValue, float>( data.Floats, values, info, filter );
                values=readFiltered<IIntValue, int>( data.Ints, values, info, filter );
                values=readFiltered<ILongValue, long>( data.Longs, values, info, filter );
                values=readFiltered<IStringValue, string>( data.Strings, values, info, filter );
                values=readFiltered<IVector3Value, Vector3>( data.Vecs, values, info, filter );
                values=readFiltered<IQuaternionValue, Quaternion>( data.Quats, values, info,
                    filter );
                values=readFiltered<byte[], byte[]>( data.ByteArrays, values, info, filter );
                ZVars.Add( new(values) );
            }
        }
        return ZVars;
    }
}

public class SelectedObjects : SelectionBase
{
    protected GameObject m_placementGhost;
    protected Vector3    m_Rotation;
    protected string     m_Name;

    static readonly Dictionary<string, string> pars=new();
    static U? Get<T, U>(T value) {
        if ((typeof(T) == typeof(IStringValue)) && (typeof(U) == typeof(string))) {
            IStringValue val=(IStringValue)value;
            return (U)(object)val.Get( pars );
        }
        if ((typeof(T) == typeof(IFloatValue)) && (typeof(U) == typeof(float))) {
            IFloatValue val=(IFloatValue)value;
            return (U)(object)val.Get( pars );
        }
        if ((typeof(T) == typeof(IIntValue)) && (typeof(U) == typeof(int))) {
            IIntValue val=(IIntValue)value;
            return (U)(object)val.Get( pars );
        }
        if ((typeof(T) == typeof(ILongValue)) && (typeof(U) == typeof(long))) {
            ILongValue val=(ILongValue)value;
            return (U)(object)val.Get( pars );
        }
        if ((typeof(T) == typeof(IVector3Value)) && (typeof(U) == typeof(Vector3))) {
            IVector3Value val=(IVector3Value)value;
            return (U)(object)val.Get( pars );
        }
        if ((typeof(T) == typeof(IQuaternionValue)) && (typeof(U) == typeof(Quaternion))) {
            IQuaternionValue val=(IQuaternionValue)value;
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
        var type=GetVType<T>();
        if (import != null) {
            import.Select( x => x ).Aggregate(
                values,
                (target, pair) => {
                    GetName( pair.Key, out var name, info );
                    U? val=Get<T, U>( pair.Value );
                    if (val != null) {
                        target[name]=val;
                    }
                    return target;
                }
            );
        }
        return values;
    }
    static Dictionary<string, ZValue> readFiltered<T, U>(
        IEnumerable<KeyValuePair<int, T>>? import,
        ZDOInfo                            info,
        Dictionary<string, ZValue>         values,
        Func<int, bool>                    filter) {
        var type=GetVType<T>();
        if (import != null) {
            import.Select( x => x ).Aggregate(
                values,
                (target, pair) => {
                    GetName( pair.Key, out var name, info );
                    U? val=Get<T, U>( pair.Value );
                    if ((val != null) && filter( pair.Key )) {
                        target[name]=ZValue.Create( name, val );
                    }
                    return target;
                }
            );
        }
        return values;
    }
    static Dictionary<string, ZValue> readStrings(
        Dictionary<int, IStringValue>? values, Dictionary<string, ZValue> target,
        Dictionary<string, string>     pars,   ZDOInfo                    info) {
        if (values != null) {
            values.Select( x => x ).Aggregate(
                target,
                (target, pair) => {
                    GetName( pair.Key, out var name, info );
                    string? val=pair.Value.Get( pars );
                    if (val != null) { target[name]=ZValue.Create( name, (string)val ); }
                    return target;
                }
            );
        }
        return target;
    }
    static Dictionary<string, ZValue> readFloats(
        Dictionary<int, IFloatValue>? values, Dictionary<string, ZValue> target,
        Dictionary<string, string>    pars,   ZDOInfo                    info) {
        if (values != null) {
            values.Select( x => x ).Aggregate(
                target,
                (target, pair) => {
                    GetName( pair.Key, out var name, info );
                    float? val=pair.Value.Get( pars );
                    if (val != null) { target[name]=ZValue.Create( name, (float)val ); }
                    return target;
                }
            );
        }
        return target;
    }
    static Dictionary<string, ZValue> readLongs(
        Dictionary<int, ILongValue>? values, Dictionary<string, ZValue> target,
        Dictionary<string, string>   pars,   ZDOInfo                    info) {
        if (values != null) {
            values.Select( x => x ).Aggregate(
                target,
                (target, pair) => {
                    GetName( pair.Key, out var name, info );
                    long? val=pair.Value.Get( pars );
                    if (val != null) { target[name]=ZValue.Create( name, (long)val ); }
                    return target;
                }
            );
        }
        return target;
    }
    static Dictionary<string, ZValue> readInts(
        Dictionary<int, IIntValue>? values, Dictionary<string, ZValue> target,
        Dictionary<string, string>  pars,   ZDOInfo                    info) {
        if (values != null) {
            values.Select( x => x ).Aggregate(
                target,
                (target, pair) => {
                    GetName( pair.Key, out var name, info );
                    int? val=pair.Value.Get( pars );
                    if (val != null) { target[name]=ZValue.Create( name, (int)val ); }
                    return target;
                }
            );
        }
        return target;
    }
    static Dictionary<string, ZValue> readQuats(
        Dictionary<int, IQuaternionValue>? values, Dictionary<string, ZValue> target,
        Dictionary<string, string>         pars,   ZDOInfo                    info) {
        if (values != null) {
            values.Select( x => x ).Aggregate(
                target,
                (target, pair) => {
                    GetName( pair.Key, out var name, info );
                    Quaternion? val=pair.Value.Get( pars );
                    if (val != null) { target[name]=ZValue.Create( name, (Quaternion)val ); }
                    return target;
                }
            );
        }
        return target;
    }
    static Dictionary<string, ZValue> readVecs(
        Dictionary<int, IVector3Value>? values, Dictionary<string, ZValue> target,
        Dictionary<string, string>      pars,   ZDOInfo                    info) {
        if (values != null) {
            values.Select( x => x ).Aggregate(
                target,
                (target, pair) => {
                    GetName( pair.Key, out var name, info );
                    Vector3? val=pair.Value.Get( pars );
                    if (val != null) { target[name]=ZValue.Create( name, (Vector3)val ); }
                    return target;
                }
            );
        }
        return target;
    }
    static Dictionary<string, ZValue> readByteArrays(
        Dictionary<int, byte[]>?   values, Dictionary<string, ZValue> target,
        Dictionary<string, string> pars,   ZDOInfo                    info) {
        if (values != null) {
            values.Select( x => x ).Aggregate(
                target,
                (target, pair) => {
                    GetName( pair.Key, out var name, info );
                    if (pair.Value != null) { target[name]=ZValue.Create( name, pair.Value ); }
                    return target;
                }
            );
        }
        return target;
    }

    static List<BpjZVars> ToZvars(List<SelectedObject> selectedObjects) {
        List<BpjZVars> ZVars= [];
        ZDOInfo        info =ZDOInfo.Instance;

        foreach (var selected in selectedObjects) {
            DataEntry data=selected.Data;
            {
                Dictionary<string, string> pars  =new Dictionary<string, string>();
                Dictionary<string, ZValue> values=new Dictionary<string, ZValue>();
                values=readFloats( data.Floats, values, pars, info );
                values=readInts( data.Ints, values, pars, info );
                values=readLongs( data.Longs, values, pars, info );
                values=readStrings( data.Strings, values, pars, info );
                values=readVecs( data.Vecs, values, pars, info );
                values=readQuats( data.Quats, values, pars, info );
                values=readByteArrays( data.ByteArrays, values, pars, info );
                ZVars.Add( new(values) );
            }
        }
        return ZVars;
    }
    public SelectedObjects(GameObject? placementGhost_, ObjectSelection selection) :
        base( Util.GetChildren( placementGhost_ ), ToZvars( selection.Objects ) ) {
        m_placementGhost
            =placementGhost_ ?? throw new ArgumentNullException( "No objects selected." );
        m_Rotation=placementGhost_.transform.rotation.eulerAngles;
        var piece=placementGhost_.GetComponent<Piece>();
        try {
            if (placementGhost_) {
                m_Name=piece
                    ? Localization.instance.Localize( piece.m_name )
                    : Utils.GetPrefabName( placementGhost_ );
            } else {
                m_Name="";
            }
        } catch (Exception e) {
            m_Name="";
            System.Console.WriteLine( "ArgoWrapper: Error in GetPrefabName" + e );
        }
    }
    public override Vector3 Rotation { get;           set; }
    public override string  Name     { get => m_Name; set => m_Name=value; }
    public virtual GameObject PlacementGhost {
        get => m_placementGhost;
        set => m_placementGhost=value;
    }

    public override List<GameObject> GetSnapPoints() => Util.GetSnapPoints( m_placementGhost );
}
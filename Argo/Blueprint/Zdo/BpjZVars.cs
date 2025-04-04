using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Argo.Blueprint.Utility;
using Argo.DataAnalysis;
using Argo.Zdo;
using UnityEngine;

namespace Argo.Blueprint;

using static Argo.Blueprint.BpjZVars;

public struct BpjZVars
{
    

    /*
    public Dictionary<string, ZValue> m_values = [];

    // todo when saveing save with prefix f for float and so on
    public bool TryGetValue<T>(int hash, out T? value) {
        GetName( hash, out var name );
        return TryGetValue( name, out value );
    }
    public bool TryGetValue<T>(string name, out T? value) {
        if (m_values.TryGetValue( name, out var zvalue ))
            if (zvalue.Type == GetVType<T>()) {
                value = (T)zvalue.Value;
                return true;
            }
        value = default;
        return false;
    }

    public void AddValue(ZValue value) { m_values[value.Name] = value; }
    public static void AddValue(Dictionary<string, ZValue> target, ZValue value) {
        target[value.Name] = value;
    }
    public BpjZVars() { }
    public BpjZVars(Dictionary<string, ZValue> values) { m_values = values; }

    [JsonIgnore] public int Count { get => m_values.Count; }

    public bool Pop(string name, out ZValue zvalue) {
        if (m_values.TryGetValue( name, out zvalue )) {
            m_values.Remove( name );
            return true;
        }
        return false;
    }

   


    private Dictionary<int, string> TryAddOnly<T>(
        ZDOType                   type, BinarySearchDictionary<int, T>? data,
        Dictionary<int, string> rest) {
        if (data != null) {
            foreach (var pair in rest) {
                if (data.TryGetValue( pair.Key, out T value )) {
                    rest.Remove( pair.Key );
                    m_values[pair.Value] = ZValue.Create( pair.Key, value );
                }
            }
        }
        return rest;
    }
    private List<int> TryAddOnly<T>(
        ZDOType     type, BinarySearchDictionary<int, T>? data,
        List<int> rest) {
        if (data != null) {
            foreach (var hash in rest) {
                if (data.TryGetValue( hash, out T value )) {
                    rest.Remove( hash );
                    AddValue( ZValue.Create( hash, value ) );
                }
            }
        }
        return rest;
    }
    private List<string> TryAddOnly<T>(
        ZDOType        type, BinarySearchDictionary<int, T>? data,
        List<string> rest) {
        if (data != null) {
            foreach (var name in rest) {
                if (data.TryGetValue( name.GetStableHashCode(), out T value )) {
                    AddValue( ZValue.Create( name, value, false ) );
                }
            }
        }
        return rest;
    }
    private Dictionary<int, string> TryAddExcept<T>(
        ZDOType                   type, BinarySearchDictionary<int, T>? data,
        Dictionary<int, string> rest) {
        if (data != null) {
            foreach (KeyValuePair<int, T> pair_ in data) {
                if (rest.ContainsKey( pair_.Key ))
                    rest.Remove( pair_.Key );
                else {
                    AddValue( ZValue.Create( pair_.Key, pair_.Value ) );
                }
            }
        }
        return rest;
    }
    private List<string> TryAddExcept<T>(
        ZDOType        type, BinarySearchDictionary<int, T>? data, List<string> rest) {
        if (data != null) {
            ZDOInfo info = ZDOInfo.GetInstance();
            foreach (KeyValuePair<int, T> pair in data) {
                bool unknown = !GetName( pair.Key, out var name, info );
                if (rest.Contains( name ))
                    rest.Remove( name );
                else { AddValue( ZValue.Create( name, pair.Value, unknown ) ); }
                return rest;
            }
        }
        return rest;
    }
    private List<int> TryAddExcept<T>(
        ZDOType     type, BinarySearchDictionary<int, T>? data, List<int> rest) {
        if (data != null) {
            ZDOInfo info      = ZDOInfo.GetInstance();
            var     this_vals = this.m_values;
            foreach (KeyValuePair<int, T> pair_ in data) {
                if (rest.Contains( pair_.Key ))
                    rest.Remove( pair_.Key );
                else {
                    AddValue( ZValue.Create( pair_.Key, pair_.Value ) );
                }
            }
        }
        return rest;
    }
    /*public List<int> AddExcept<T>(
        VType type, BinarySearchDictionary<int, T>? data, List<string> rest) {
        return AddExcept(type, data, // @formatter:off
            rest.Select(x => x.GetStableHashCode()).ToList());}// @formatter:on#1#

    /*public Dictionary<int, string> AddFilter<T>(
        VType type, BinarySearchDictionary<int, T>? data, List<string> rest) {
        return AddFilter(type, data,
            rest.Select(x => x).Aggregate(new Dictionary<int, string>(), // @formatter:off
                (ret, x) => {ret.Add(x.GetStableHashCode(), x);return ret;}));}// @formatter:on
                #1#

    private List<T> TryAddOnlyHelper<T, U>(
        ZDOType                           type,
        BinarySearchDictionary<int, U>? data,
        List<T>                         rest) {
        return typeof(T) switch {
            Type t when t == typeof(int)
                => TryAddOnly( type, data, rest as List<int> ) as List<T>,
            Type t when t == typeof(string)
                => TryAddOnly( type, data, rest as List<string> ) as List<T>,
            _ => throw new ArgumentException(
                $"Unsupported data type: {typeof(T)}, You can only add ints or strings as keys" )
        } ?? [];
    }

    private List<T> TryAddExceptHelper<T, U>(
        ZDOType                           type,
        BinarySearchDictionary<int, U>? data,
        List<T>                         rest) {
        return typeof(T) switch {
            Type t when t == typeof(int)
                => TryAddOnly( type, data, rest as List<int> ) as List<T>,
            Type t when t == typeof(string)
                => TryAddOnly( type, data, rest as List<string> ) as List<T>,
            _ => throw new ArgumentException(
                $"Unsupported data type: {typeof(T)}, You can only add ints or strings as keys" )
        } ?? [];
    }

    public List<T> TryAddOnly<T>(ZDO zdo, params T[] name_arr) {
        if (name_arr.Length <= 0) return [];
        List<T> rest = new List<T>();

        rest = TryAddOnlyHelper( ZDOType.I, ZDOExtraData.s_ints[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddOnlyHelper( ZDOType.F, ZDOExtraData.s_floats[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddOnlyHelper( ZDOType.L, ZDOExtraData.s_longs[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddOnlyHelper<T, string>( ZDOType.S, ZDOExtraData.s_strings[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddOnlyHelper( ZDOType.V, ZDOExtraData.s_vec3[zdo.m_uid], rest );
        if (rest.Count > 0) return [];
        rest = TryAddOnlyHelper( ZDOType.Q, ZDOExtraData.s_quats[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddOnlyHelper<T, byte[]>( ZDOType.B, ZDOExtraData.s_byteArrays[zdo.m_uid], rest );
        return rest;
    }

    public List<T> TryAddExcept<T>(ZDO zdo, params T[] name_arr) {
        if (name_arr.Length <= 0) return [];
        List<T> rest = new List<T>();
        rest = TryAddExceptHelper<T, int>( ZDOType.I, ZDOExtraData.s_ints[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddExceptHelper<T, float>( ZDOType.F, ZDOExtraData.s_floats[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddExceptHelper<T, long>( ZDOType.L, ZDOExtraData.s_longs[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddExceptHelper<T, string>( ZDOType.S, ZDOExtraData.s_strings[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddExceptHelper<T, Vector3>( ZDOType.V, ZDOExtraData.s_vec3[zdo.m_uid], rest );
        if (rest.Count > 0) return [];
        rest = TryAddExceptHelper<T, Quaternion>( ZDOType.Q, ZDOExtraData.s_quats[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddExceptHelper<T, byte[]>( ZDOType.B, ZDOExtraData.s_byteArrays[zdo.m_uid], rest );
        return (rest as List<T>);
    }

    public List<int> TryAddOnly(ZDOType type, BinarySearchDictionary<int, int> data, List<int> rest) {
        if (data != null) {
            ZDOInfo info      = ZDOInfo.GetInstance();
            var     this_vals = this.m_values;
            foreach (var pair_ in data) {
                if (rest.Contains( pair_.Key ))
                    rest.Remove( pair_.Key );
                else {
                    AddValue( ZValue.Create( pair_.Key, pair_.Value ) );
                }
            }
        }
        return rest;
    }
    public void AddPair<T>(string key, T value) {
        if (typeof(T) == typeof(ZDO)) {
            throw new ArgumentException( "For adding from a ZDO, use TryAddFromZdo instead" );
        }
        m_values.Add( key, ZValue.Create( key, value, true ) );
    }
    public void AddPair<T>(int key, T value) {
        if (typeof(T) == typeof(ZDO)) {
            throw new ArgumentException( "For adding from a ZDO, use TryAddFromZdo instead" );
        }
        AddValue( ZValue.Create( key, value ) );
    }

    public List<int> TryAddOnly(ZDO zdo, params int[] hashes)
        => TryAddOnly<int>( zdo, hashes );

    public List<string> TryAddOnly(ZDO zdo, params string[] names)
        => TryAddOnly<string>( zdo, names );

    public List<int> TryAddOnly<T>(ZDO zdo, params int[] hashes)
        => TryAddOnly<T>( GetVType<T>(), GetDict<T>( zdo ), hashes.ToList() );

    public List<string> TryAddOnly<T>(ZDO zdo, params string[] names)
        => TryAddOnly<T>( GetVType<T>(), GetDict<T>( zdo ), names.ToList() );

    static Dictionary<string, ZValue> ReadFromZdo(ZDO zdo) {
        ZDOInfo                    info   = ZDOInfo.GetInstance();
        Dictionary<string, ZValue> values = new Dictionary<string, ZValue>();
        values = readvals( ZDOExtraData.s_floats[zdo.m_uid], info, values );
        values = readvals( ZDOExtraData.s_vec3[zdo.m_uid], info, values );
        values = readvals( ZDOExtraData.s_quats[zdo.m_uid], info, values );
        values = readvals( ZDOExtraData.s_ints[zdo.m_uid], info, values );
        values = readvals( ZDOExtraData.s_longs[zdo.m_uid], info, values );
        values = readvals( ZDOExtraData.s_strings[zdo.m_uid], info, values );
        values = readvals( ZDOExtraData.s_byteArrays[zdo.m_uid], info, values );
        return values;
    }
    public static BpjZVars MakeFromZdo(ZDO zdo) {
        var values = ReadFromZdo( zdo );
        if (values.Count > 0) { return new BpjZVars( values ); }
        return new BpjZVars();
    }
    public static bool GetName(int hash, out string name, ZDOInfo info) {
        bool ret = info.HashToName.TryGetValue( hash, out name );
        if (!ret) { name = hash.ToString(); }
        return ret;
    }
    public static bool GetName(int hash, out string name) {
        ZDOInfo info = ZDOInfo.GetInstance();
        return GetName( hash, out name, info );
    }

    public bool TryAddFromZdo<T>(ZDO zdo, int hash) {
        if (GetDict<T>( zdo )?.TryGetValue( hash, out T value ) ?? false) {
            AddValue( ZValue.Create( hash, value ) );

            return true;
        }
        return false;
    }

    public bool TryAddFromZdo<T>(ZDO zdo, string name) {
        int hash = name.GetStableHashCode();
        if (GetDict<T>( zdo )?.TryGetValue( hash, out T value ) ?? false) {
            AddValue( ZValue.Create( name, value, true ) );
            return true;
        }
        return false;
    }

    static Dictionary<string, ZValue> readvals<T>(
        IEnumerable<KeyValuePair<int, T>>? import,
        ZDOInfo                            info,
        Dictionary<string, ZValue>         values) {
        var type = GetVType<T>();
        if (import != null) {
            import.Select( x => x ).Aggregate(
                values,
                (target, pair) => {
                    AddValue( target, ZValue.Create( pair.Key, pair.Value ) );
                    return target;
                }
            );
        }
        return values;
    }
    public static ZDOType GetVType<T>() {
        return typeof(T) switch {
            Type t when t == typeof(int) => ZDOType.I,
            Type t when t == typeof(float) => ZDOType.F,
            Type t when t == typeof(Quaternion) => ZDOType.Q,
            Type t when t == typeof(Vector3) => ZDOType.V,
            Type t when t == typeof(long) => ZDOType.L,
            Type t when t == typeof(string) => ZDOType.S,
            Type t when t == typeof(byte[]) => ZDOType.B,
            _ => throw new ArgumentException( $"Unsupported data type: {typeof(T)}" ),
        };
    }

    static BinarySearchDictionary<int, T>? GetDict<T>(ZDO zdo) {
        return typeof(T) switch {
            Type t when t == typeof(int) =>
                ZDOExtraData.s_ints[zdo.m_uid] as BinarySearchDictionary<int, T>,
            Type t when t == typeof(float) =>
                ZDOExtraData.s_floats[zdo.m_uid] as BinarySearchDictionary<int, T>,
            Type t when t == typeof(Quaternion) =>
                ZDOExtraData.s_quats[zdo.m_uid] as BinarySearchDictionary<int, T>,
            Type t when t == typeof(Vector3) =>
                ZDOExtraData.s_vec3[zdo.m_uid] as BinarySearchDictionary<int, T>,
            Type t when t == typeof(long) =>
                ZDOExtraData.s_longs[zdo.m_uid] as BinarySearchDictionary<int, T>,
            Type t when t == typeof(string) =>
                ZDOExtraData.s_strings[zdo.m_uid] as BinarySearchDictionary<int, T>,
            Type t when t == typeof(byte[]) => ZDOExtraData.s_byteArrays[zdo.m_uid] as
                BinarySearchDictionary<int, T>,
            _ => throw new ArgumentException( $"Unsupported data type: {typeof(T)}" )
        } ?? [];
    }*/
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Argo.blueprint.Util;
using Argo.DataAnalysis;
using UnityEngine;

namespace Argo.Blueprint;
using static Argo.Blueprint.BpjZVars;
public class ZdoConverter : JsonConverter<ZValue>
{
    static readonly Vec3JsonConverter vec3JsonConverter = new Vec3JsonConverter();
    static readonly QuatJsonConverter quatJsonConverter = new QuatJsonConverter();
    public override void Write(
        Utf8JsonWriter        writer, ZValue zvar,
        JsonSerializerOptions options) {
        try {
            var unknown  = zvar.UnknownHash;
            if (unknown == true) {
                writer.WritePropertyName( "u" + (char)zvar.Type + zvar.Name );
            } else {
                writer.WritePropertyName( (char)zvar.Type + zvar.Name );
            }
            switch (zvar.Type) {
                case ZType.Float: writer.WriteNumberValue( (float)(zvar.Value) ); break;
                case ZType.Vec3:
                    vec3JsonConverter.Write ( writer, (Vector3)(zvar.Value), options );
                    break;
                case ZType.Quat:
                    quatJsonConverter.Write( writer, (Quaternion)(zvar.Value), options );
                    break;
                case ZType.Int:    writer.WriteNumberValue( (int)(zvar.Value) ); break;
                case ZType.Long:   writer.WriteNumberValue( (long)(zvar.Value) ); break;
                case ZType.String: writer.WriteStringValue( (string)(zvar.Value) ); break;
                case ZType.ByteArray:
                    writer.WriteStringValue( (string)(zvar.Value) ); break;
                default: throw new JsonException( "BpjZVars.ZValue has Unknown type" );
            }
        } catch (Exception e) {
            System.Console.WriteLine( "Error in Json Serializer BpjZVars.ZValue " + e );
            throw e;
        }
    }

    public override BpjZVars.ZValue Read(
        ref Utf8JsonReader    reader, Type type,
        JsonSerializerOptions options) {
        try {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException( "Expected start of of object." );

            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();
            string name    = reader.GetString() ?? string.Empty;
            char   prefix  = name[0];
            bool   unknown;
            if (prefix == 'u') {
                prefix = name[1];
                name   = name.Substring( 2 );
                unknown  = true;
            } else {
                name    = name.Substring( 1 );
                unknown = false;
            }
            reader.Read();
            switch (prefix) {
                case (char)ZType.Float:
                    return new ZValue( name, reader.GetSingle(), unknown );
                case (char)ZType.Vec3:
                    return new ZValue( name,
                        vec3JsonConverter.Read(ref reader, typeof(Vector3),  options ), unknown );
                case (char)ZType.Quat:
                    return new ZValue( name,
                        quatJsonConverter.Read(ref reader, typeof(Quaternion),  options ), unknown );
                case (char)ZType.Int:
                    return new ZValue( name, reader.GetInt32(), unknown );
                case (char)ZType.Long:
                    return new ZValue( name, reader.GetInt64(), unknown );
                case (char)ZType.String:
                    return new ZValue( name,
                        reader.GetString() ?? "", unknown );
                case (char)ZType.ByteArray:
                    return new ZValue( name,
                        reader.GetString() ?? "", unknown );
                default:
                    throw new JsonException( "Unknown type" );
            }
        } catch (Exception e) {
            System.Console.WriteLine( "Error in Json Serializer BpjZVars.ZValue " + e );
            throw;
        }
    }
}

public struct BpjZVars
{
    
    public enum ZType : byte
    {
        Float       = (byte)'f',
        Vec3        = (byte)'v',
        Quat        = (byte)'q',
        Int         = (byte)'i',
        Long        = (byte)'l',
        String      = (byte)'s',
        ByteArray   = (byte)'b',
    
        F = (byte)'f',
        V = (byte)'v',
        Q = (byte)'q',
        I = (byte)'i',
        L = (byte)'l',
        S = (byte)'s',
        B = (byte)'b',
    }

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

    public struct ZValue
    {
        [JsonIgnore]         ZType  m_type;
        [JsonIgnore]         string m_name;
        [JsonIgnore]         object m_value;
        [JsonIgnore]         bool   m_hashUnknown = false;
        [JsonInclude] public ZType  Type  { get => m_type; set => m_type = value; }
        [JsonInclude] public string Name  { get => m_name; set => m_name = value; }
        [JsonInclude] public object Value { get => m_value; }
        [JsonInclude] public bool UnknownHash { get => m_hashUnknown; }
        public               void   SetValue(int        value_) => m_value = value_;
        public               void   SetValue(float      value_) => m_value = value_;
        public               void   SetValue(Quaternion value_) => m_value = value_;
        public               void   SetValue(Vector3    value_) => m_value = value_;
        public               void   SetValue(long       value_) => m_value = value_;
        public               void   SetValue(byte[]     value_) => m_value = value_;
        public               void   SetValue(string     value_) => m_value = value_;

        private ZValue(ZType type_, string name_, object value_, bool unknown = false) {
            m_type  = type_;
            m_name  = name_;
            m_value = value_;
            m_hashUnknown = unknown;
        }
        public static ZValue Create<T>(string name_, T value, bool unknown = false) {
            if (value == null) throw new InvalidOperationException();
            if (typeof(T) == typeof(int))
                return new ZValue( name_, (int)(object)value, unknown );
            if (typeof(T) == typeof(float))
                return new ZValue( name_, (float)(object)value, unknown );
            if (typeof(T) == typeof(Quaternion))
                return new ZValue( name_, (Quaternion)(object)value, unknown );
            if (typeof(T) == typeof(Vector3))
                return new ZValue( name_, (Vector3)(object)value, unknown );
            if (typeof(T) == typeof(long))
                return new ZValue( name_, (long)(object)value, unknown );
            if (typeof(T) == typeof(string))
                return new ZValue( name_, (string)(object)value, unknown );
            if (typeof(T) == typeof(byte[]))
                return new ZValue( name_, (byte[])(object)value, unknown );
            throw new ArgumentException( $"Unsupported data type1: {typeof(T)}" );
        }
        public static ZValue Create<T>(int hash, T value_) {
            var unknown  = !GetName( hash, out string name_ );
            if (value_ == null) throw new InvalidOperationException();
            if (typeof(T) == typeof(int))
                return new ZValue( name_, (int)(object)value_, unknown );
            if (typeof(T) == typeof(float))
                return new ZValue( name_, (float)(object)value_, unknown );
            if (typeof(T) == typeof(Quaternion))
                return new ZValue( name_, (Quaternion)(object)value_, unknown );
            if (typeof(T) == typeof(Vector3))
                return new ZValue( name_, (Vector3)(object)value_, unknown );
            if (typeof(T) == typeof(long))
                return new ZValue( name_, (long)(object)value_, unknown );
            if (typeof(T) == typeof(string))
                return new ZValue( name_, (string)(object)value_, unknown );
            if (typeof(T) == typeof(byte[]))
                return new ZValue( name_, (byte[])(object)value_, unknown );
            throw new ArgumentException( $"Unsupported data type1: {typeof(T)}" );
        }
        static T CheckValue<T>(T value) {
            if (value     == null) throw new InvalidOperationException();
            if (typeof(T) == typeof(int)) { return (T)value; }
            if (typeof(T) == typeof(float)) { return (T)value; }
            if (typeof(T) == typeof(Quaternion)) { return (T)value; }
            if (typeof(T) == typeof(Vector3)) { return (T)value; }
            if (typeof(T) == typeof(long)) { return (T)value; }
            if (typeof(T) == typeof(byte[])) { return (T)value; }
            if (typeof(T) == typeof(string)) { return (T)value; }

            throw new ArgumentException( $"Unsupported data type2: {typeof(T)}" );
        }
        public ZValue(string name_, int value_, bool unknown = false) :
            this( ZType.I  ,name_, value_, unknown ) { }
        public ZValue(string name_, long value_, bool unknown = false) :
            this( ZType.L, name_, value_, unknown ) { }
        public ZValue(string name_, float value_, bool unknown = false) :
            this( ZType.F, name_, value_, unknown ) { }
        public ZValue(string name_, string value_, bool unknown = false) :
            this( ZType.S, name_, value_, unknown ) { }
        public ZValue(string name_, Vector3 value_, bool unknown = false) :
            this( ZType.V  ,name_, value_, unknown ) { }
        public ZValue(string name_, Quaternion value_, bool unknown = false) :
            this( ZType.Q  ,name_, value_, unknown ) { }
        public ZValue(string name_, byte[] value_, bool unknown = false) {
            m_type = ZType.ByteArray;
            m_name = name_;
            m_hashUnknown = unknown;
            ZPackage pkg = new ZPackage();
            pkg.Write( value_ );
            m_value = pkg.GetBase64();
        }
    }

    public Dictionary<string, ZValue> m_values = [];

    public void AddValue(ZValue value) { m_values[value.Name] = value; }
    public static void AddValue(Dictionary<string, ZValue> target, ZValue value) {
        target[value.Name] = value;
    }
    public BpjZVars() { }
    public BpjZVars(Dictionary<string, ZValue> values) { m_values = values; }

    [JsonIgnore] public int Count { get => m_values.Count; }

    private Dictionary<int, string> TryAddOnly<T>(
        ZType                   type, BinarySearchDictionary<int, T>? data,
        Dictionary<int, string> rest) {
        if (data != null) {
            foreach (var pair in rest) {
                if (data.TryGetValue( pair.Key, out T value )) {
                    rest.Remove( pair.Key );
                    m_values[pair.Value] = ZValue.Create( pair.Value, value );
                }
            }
        }
        return rest;
    }
    private List<int> TryAddOnly<T>(
        ZType     type, BinarySearchDictionary<int, T>? data,
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
        ZType        type, BinarySearchDictionary<int, T>? data,
        List<string> rest) {
        if (data != null) {
            foreach (var name in rest) {
                if (data.TryGetValue( name.GetStableHashCode(), out T value )) {
                    AddValue( ZValue.Create( name, value ) );
                }
            }
        }
        return rest;
    }
    private Dictionary<int, string> TryAddExcept<T>(
        ZType                   type, BinarySearchDictionary<int, T>? data,
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
        ZType        type, BinarySearchDictionary<int, T>? data,
        List<string> rest) {
        if (data != null) {
            ZDOInfo info      = ZDOInfo.Instance;
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
        ZType     type, BinarySearchDictionary<int, T>? data,
        List<int> rest) {
        if (data != null) {
            ZDOInfo info      = ZDOInfo.Instance;
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
            rest.Select(x => x.GetStableHashCode()).ToList());}// @formatter:on*/

    /*public Dictionary<int, string> AddFilter<T>(
        VType type, BinarySearchDictionary<int, T>? data, List<string> rest) {
        return AddFilter(type, data,
            rest.Select(x => x).Aggregate(new Dictionary<int, string>(), // @formatter:off
                (ret, x) => {ret.Add(x.GetStableHashCode(), x);return ret;}));}// @formatter:on
                */

    private List<T> TryAddOnlyHelper<T, U>(
        ZType                           type,
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
        ZType                           type,
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

        rest = TryAddOnlyHelper( ZType.I, ZDOExtraData.s_ints[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddOnlyHelper( ZType.F, ZDOExtraData.s_floats[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddOnlyHelper( ZType.L, ZDOExtraData.s_longs[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddOnlyHelper<T, string>( ZType.S, ZDOExtraData.s_strings[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddOnlyHelper( ZType.V, ZDOExtraData.s_vec3[zdo.m_uid], rest );
        if (rest.Count > 0) return [];
        rest = TryAddOnlyHelper( ZType.Q, ZDOExtraData.s_quats[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddOnlyHelper<T, byte[]>( ZType.B, ZDOExtraData.s_byteArrays[zdo.m_uid], rest );
        return rest;
    }

    public List<T> TryAddExcept<T>(ZDO zdo, params T[] name_arr) {
        if (name_arr.Length <= 0) return [];
        List<T> rest = new List<T>();
        rest = TryAddExceptHelper<T, int>( ZType.I, ZDOExtraData.s_ints[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddExceptHelper<T, float>( ZType.F, ZDOExtraData.s_floats[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddExceptHelper<T, long>( ZType.L, ZDOExtraData.s_longs[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddExceptHelper<T, string>( ZType.S, ZDOExtraData.s_strings[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddExceptHelper<T, Vector3>( ZType.V, ZDOExtraData.s_vec3[zdo.m_uid], rest );
        if (rest.Count > 0) return [];
        rest = TryAddExceptHelper<T, Quaternion>( ZType.Q, ZDOExtraData.s_quats[zdo.m_uid], rest );
        if (rest.Count <= 0) return [];
        rest = TryAddExceptHelper<T, byte[]>( ZType.B, ZDOExtraData.s_byteArrays[zdo.m_uid], rest );
        return (rest as List<T>);
    }

    public List<int> TryAddOnly(ZType type, BinarySearchDictionary<int, int> data, List<int> rest) {
        if (data != null) {
            ZDOInfo info      = ZDOInfo.Instance;
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
    public void AddPair<T>(T value, string name) {
        if (typeof(T) == typeof(ZDO)) {
            throw new ArgumentException( "For adding from a ZDO, use TryAddFromZdo instead" );
        }
        m_values.Add( name, ZValue.Create( name, value ) );
    }
    public void AddPair<T>(T value, int hash) {
        if (typeof(T) == typeof(ZDO)) {
            throw new ArgumentException( "For adding from a ZDO, use TryAddFromZdo instead" );
        }
        AddValue( ZValue.Create( hash, value ) );
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
        ZDOInfo                    info   = ZDOInfo.Instance;
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
        ZDOInfo info = ZDOInfo.Instance;
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
            AddValue( ZValue.Create( name, value ) );
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
    public static ZType GetVType<T>() {
        return typeof(T) switch {
            Type t when t == typeof(int) => ZType.I,
            Type t when t == typeof(float) => ZType.F,
            Type t when t == typeof(Quaternion) => ZType.Q,
            Type t when t == typeof(Vector3) => ZType.V,
            Type t when t == typeof(long) => ZType.L,
            Type t when t == typeof(string) => ZType.S,
            Type t when t == typeof(byte[]) => ZType.B,
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
    }
}
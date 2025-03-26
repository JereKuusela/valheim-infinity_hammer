using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Argo.blueprint.Util;
using Argo.DataAnalysis;
using UnityEngine;
using UnityEngine.Serialization;

namespace Argo.Blueprint;

using DOdata = ZDOExtraData;
using static BpoZDOVars;
using static BpoZDOVars.ZType;

[Flags]
public enum BPOFlags : uint
{
    // todo flags setzen nach Piece -> PieceCategory, ComfortGroup, m_cultivatedGroundOnly
    // todo     harvest -> m_harvest;m_harvestRadius;  m_harvestRadiusMaxLevel;
    // todo     requrement -> m_craftingStation??
    // todo     "piece_nonsolid"
    // todo     Component<Pickable>
    // todo    is m_targetNonPlayerBuilt != BuildPlayer? (may rename build Player Playerbuildable) 
    //	 for pieces buildable by the player with the hammer
    BuildPlayer = 1u << 31,
    //	 for all build piece even spawn only like the dverger probs
    BuildPiece = 1 << 30,
    Placeable  = 1 << 29, //	for pieces like food placed with the serving tray
    //	 for the option to exclude compfort pieces from static pieces (which do nothing)
    Compfort    = 1 << 28,
    LightSource = 1 << 27, //	 for pieces like armor stands and item stands	

    Hoverable    = 1 << 25, // todo add those tags, kinda forgott
    TextReceiver = 1 << 24, //		
    Interactable = 1 << 23, //		
//	 for containers like the chest and things that need	have fuel
    ObjectHolder    = 1 << 21,
    ContainerPiece  = 1 << 20, //		
    CraftingStation = 1 << 19, //		
    Fuel            = 1 << 18, //	 for pieces with fuel like the furnace	
    Pickable        = 1 << 17, //	 e.g for food placed with the serving tray or crops	
//	 for player grown pieces which need cultivated ground (vegtables basically)
    Cultivated            = 1 << 15,
    DestroyableTerrain    = 1 << 14, //	 Objects like trees and rocks 	
    Fractured             = 1 << 13, //	 for partially broken stones etc	
    HoverableResourceNode = 1 << 12, //	 	
    NonStatic
        = 1 << 11, //	 pieces with some sort of funktion but not direktly interactable	 like ladders and whisplights
    Creature = 1 << 10, //	 for all creatures	
    Tameable = 1 << 9, //	 for all creatures that can be tamed
    Vehicle  = 1 << 8, //	 for all vehicles
//	 for player grown pieces which need cultivated ground (vegtables basically)
    Animated = 1 << 6,
    //	 for stuff like WaterInteractable  PieceMarker ... which are pretty rare      PieceMarker  WaterInteractable  DooDadControler  Projectile: condensed into Special Interface
    SpecialInterface = 1 << 5,
    //	 for interfaces Hoverable	 IHasHoverMenu & IHasHoverMenuExtended
    Indestructible = 1 << 4,
    // piece categories from Valheim Piece.cs PieceCategory.
    // Uses the last 4 bits but leave the last 8 open for future additions
    CustomNotVanilla = 1 << 0,
}

public class BpoConverter : JsonConverter<BpjObject>
{
    public override void Write(
        Utf8JsonWriter        writer, BpjObject Bpo,
        JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WritePropertyName(Bpo.m_prefab);
        JsonSerializer.Serialize(writer, Bpo.m_data, options);
        if (Bpo.m_properties.Count != 0) {
            var options1 = new JsonSerializerOptions(options);
            options1.Converters.Add(new ZdoConverter());
            writer.WritePropertyName("ZDOVars");
            JsonSerializer.Serialize(writer, Bpo.m_properties.m_values.Values, options1);
        }
        writer.WriteEndObject();
        writer.WriteRawValue("\n", true);
    }

    public override BpjObject Read(
        ref Utf8JsonReader    reader, Type type,
        JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of of object.");

        reader.Read();
        string prefab = reader.GetString();
        BpjObject.TData data
            = JsonSerializer.Deserialize<BpjObject.TData>(ref reader, options);
        BpjObject bpo = new BpjObject(prefab, data);
        reader.Read();
        if (reader.TokenType == JsonTokenType.EndObject)
            return bpo; 

        if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();

        string temp = reader.GetString() ?? "";
        if (string.Equals(temp, "ZDOVars",
                StringComparison.InvariantCultureIgnoreCase)) {

            reader.Read();
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of Object.");
            reader.Read();
            
            var options1 = new JsonSerializerOptions(options);
            options1.Converters.Add(new ZdoConverter());
            
            BpoZDOVars values = new BpoZDOVars();
            while (reader.TokenType != JsonTokenType.EndObject) {
              
                ZValue v = JsonSerializer.Deserialize<ZValue>(ref reader, options1);
                values.m_values.Add(v.name, v);
                reader.Read();
            }
            bpo.ZVars = values;
        }
        if (reader.TokenType != JsonTokenType.EndObject)
            throw new JsonException("Expected end of Object.");
        return bpo;
    }
}

public class ZdoConverter : JsonConverter<BpoZDOVars.ZValue>
{
    public override void Write(
        Utf8JsonWriter        writer, BpoZDOVars.ZValue value,
        JsonSerializerOptions options) {
        writer.WritePropertyName((char)value.type + value.name);
        switch (value.type) {
            case Float: writer.WriteNumberValue((float)value.value); break;
            case Vec3:
                JsonSerializer.Serialize(writer, (Vector3)value.value, options);
                break;
            case Quat:
                JsonSerializer.Serialize(writer, (Quaternion)value.value, options);
                break;
            case Int:       writer.WriteNumberValue((int)value.value); break;
            case Long:      writer.WriteNumberValue((long)value.value); break;
            case String:    writer.WriteStringValue((string)value.value); break;
            case ByteArray: writer.WriteStringValue((string)value.value); break;
            default:                         throw new JsonException("Unknown type");
        }
    }

    public override BpoZDOVars.ZValue Read(
        ref Utf8JsonReader    reader, Type type,
        JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of of object.");

        if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();
        string name   = reader.GetString();
        char   prefix = name[0];
        name = name.Substring(1);
        reader.Read();
        switch (prefix) {
            case (char)Float: return new ZValue(Float, name, reader.GetSingle());
            case (char)Vec3:
                return new ZValue(Vec3, name,
                    JsonSerializer.Deserialize<Vector3>(ref reader, options));
            case (char)Quat:
                return new ZValue(Quat, name,
                    JsonSerializer.Deserialize<Quaternion>(ref reader, options));
            case (char)Int:
                return new ZValue(Int, name, reader.GetInt32());
            case (char)Long:
                return new ZValue(Long, name, reader.GetInt64());
            case (char)String:
                return new ZValue(String, name, reader.GetString() ?? "");
            case (char)ByteArray:
                return new ZValue(ByteArray, name, reader.GetString() ?? "");
            default:
                throw new JsonException("Unknown type");
        }
    }
}

public struct BpoZDOVars
{
    public enum ZType : byte
    {
        Float     = (byte)'f',
        Vec3      = (byte)'v',
        Quat      = (byte)'q',
        Int       = (byte)'i',
        Long      = (byte)'l',
        String    = (byte)'s',
        ByteArray = (byte)'b',
        Unknown   = (byte)'u',
        F         = (byte)'f',
        V         = (byte)'v',
        Q         = (byte)'q',
        I         = (byte)'i',
        L         = (byte)'l',
        S         = (byte)'s',
        B         = (byte)'b',
    }

    public struct ZValue
    {
        public ZType  type;
        public string name;
        public object value;
        public ZValue(ZType type_, string name_, object value_) {
            type  = type_;
            name  = name_;
            value = value_;
        }
        public ZValue(string name_, int value_) :
            this(ZType.I, name_, value_) { }
        public ZValue(string name_, long value_) :
            this(ZType.L, name_, value_) { }
        public ZValue(string name_, float value_) :
            this(ZType.F, name_, value_) { }
        public ZValue(string name_, string value_) :
            this(ZType.S, name_, value_) { }
        public ZValue(string name_, Vector3 value_) : this(ZType.V, name_,
            value_) { }
        public ZValue(string name_, Quaternion value_) : this(ZType.Q, name_,
            value_) { }
        public ZValue(string name_, byte[] value_) {
            type = ZType.ByteArray;
            name = name_;
            ZPackage pkg = new ZPackage();
            pkg.Write(value_);
            pkg.GetBase64();
            value = pkg.GetBase64();
        }
    }

    // todo when saveing save with prefix f for float and so on
    [JsonInclude] public Dictionary<string, ZValue> m_values = [];
    public BpoZDOVars() { }
    public BpoZDOVars(Dictionary<string, ZValue> values) { m_values = values; }

    public int Count { get => m_values.Count; }

    internal Dictionary<int, string> TryAddOnly<T>(
        ZType                   type, BinarySearchDictionary<int, T>? data,
        Dictionary<int, string> rest) {
        if (data != null) {
            foreach (var pair in rest) {
                if (data.TryGetValue(pair.Key, out T value)) {
                    rest.Remove(pair.Key);
                    m_values[pair.Value] = new ZValue(type, pair.Value, value);
                }
            }
        }
        return rest;
    }
    internal List<int> TryAddOnly<T>(
        ZType     type, BinarySearchDictionary<int, T>? data,
        List<int> rest) {
        if (data != null) {
            foreach (var hash in rest) {
                if (data.TryGetValue(hash, out T value)) {
                    rest.Remove(hash);
                    GetNameOrDefault(hash, out string name);
                    m_values[name] = new ZValue(type, name, value);
                }
            }
        }
        return rest;
    }
    internal List<string> TryAddOnly<T>(
        ZType        type, BinarySearchDictionary<int, T>? data,
        List<string> rest) {
        if (data != null) {
            foreach (var name in rest) {
                if (data.TryGetValue(name.GetStableHashCode(), out T value)) {
                    rest.Remove(name);
                    m_values[name] = new ZValue(type, name, value);
                }
            }
        }
        return rest;
    }
    public Dictionary<int, string> TryAddExcept<T>(
        ZType                   type, BinarySearchDictionary<int, T>? data,
        Dictionary<int, string> rest) {
        if (data != null) {
            ZDOInfo info      = ZDOInfo.Instance;
            var     this_vals = this.m_values;

            foreach (KeyValuePair<int, T> pair_ in data) {
                if (rest.ContainsKey(pair_.Key))
                    rest.Remove(pair_.Key);
                else {
                    GetNameOrDefault(pair_.Key, out var name_, info);
                    this_vals[name_] = new ZValue(type, name_, pair_.Value);
                }
            }
        }
        return rest;
    }
    public List<string> TryAddExcept<T>(
        ZType        type, BinarySearchDictionary<int, T>? data,
        List<string> rest) {
        if (data != null) {
            ZDOInfo info      = ZDOInfo.Instance;
            var     this_vals = this.m_values;
            foreach (KeyValuePair<int, T> pair in data) {
                GetNameOrDefault(pair.Key, out var name, info);
                if (rest.Contains(name))
                    rest.Remove(name);
                else { this_vals[name] = new ZValue(type, name, pair.Value); }
                return rest;
            }
        }
        return rest;
    }
    public List<int> TryAddExcept<T>(
        ZType     type, BinarySearchDictionary<int, T>? data,
        List<int> rest) {
        if (data != null) {
            ZDOInfo info      = ZDOInfo.Instance;
            var     this_vals = this.m_values;
            foreach (KeyValuePair<int, T> pair_ in data) {
                if (rest.Contains(pair_.Key))
                    rest.Remove(pair_.Key);
                else {
                    GetNameOrDefault(pair_.Key, out var name_, info);
                    this_vals[name_] = new ZValue(type, name_, pair_.Value);
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
                => TryAddOnly(type, data, rest as List<int>) as List<T>,
            Type t when t == typeof(string)
                => TryAddOnly(type, data, rest as List<string>) as List<T>,
            _ => throw new ArgumentException(
                $"Unsupported data type: {typeof(T)}, You can only add ints or strings as keys")
        } ?? [];
    }

    private List<T> TryAddExceptHelper<T, U>(
        ZType                           type,
        BinarySearchDictionary<int, U>? data,
        List<T>                         rest) {
        return typeof(T) switch {
            Type t when t == typeof(int)
                => TryAddOnly(type, data, rest as List<int>) as List<T>,
            Type t when t == typeof(string)
                => TryAddOnly(type, data, rest as List<string>) as List<T>,
            _ => throw new ArgumentException(
                $"Unsupported data type: {typeof(T)}, You can only add ints or strings as keys")
        } ?? [];
    }

    public List<T> TryAddOnly<T>(ZDO zdo, params T[] name_arr) {
        if (name_arr.Length <= 0) return [];
        List<T> rest = new List<T>();

        rest = TryAddOnlyHelper<T, int>(ZType.I, ZDOExtraData.s_ints[zdo.m_uid], rest);
        if (rest.Count <= 0) return [];
        rest = TryAddOnlyHelper<T, float>(ZType.F, ZDOExtraData.s_floats[zdo.m_uid], rest);
        if (rest.Count <= 0) return [];
        rest = TryAddOnlyHelper<T, long>(ZType.L, ZDOExtraData.s_longs[zdo.m_uid], rest);
        if (rest.Count <= 0) return [];
        rest = TryAddOnlyHelper<T, string>(ZType.S, ZDOExtraData.s_strings[zdo.m_uid], rest);
        if (rest.Count <= 0) return [];
        rest = TryAddOnlyHelper<T, Vector3>(ZType.V, ZDOExtraData.s_vec3[zdo.m_uid], rest);
        if (rest.Count > 0) return [];
        rest = TryAddOnlyHelper<T, Quaternion>(ZType.Q, ZDOExtraData.s_quats[zdo.m_uid], rest);
        if (rest.Count <= 0) return [];
        rest = TryAddOnlyHelper<T, byte[]>(ZType.B, ZDOExtraData.s_byteArrays[zdo.m_uid], rest);
        return (rest as List<T>);
    }

    public List<T> TryAddExcept<T>(ZDO zdo, params T[] name_arr) {
        if (name_arr.Length <= 0) return [];
        List<T> rest = new List<T>();
        rest = TryAddExceptHelper<T, int>(ZType.I, ZDOExtraData.s_ints[zdo.m_uid], rest);
        if (rest.Count <= 0) return [];
        rest = TryAddExceptHelper<T, float>(ZType.F, ZDOExtraData.s_floats[zdo.m_uid], rest);
        if (rest.Count <= 0) return [];
        rest = TryAddExceptHelper<T, long>(ZType.L, ZDOExtraData.s_longs[zdo.m_uid], rest);
        if (rest.Count <= 0) return [];
        rest = TryAddExceptHelper<T, string>(ZType.S, ZDOExtraData.s_strings[zdo.m_uid], rest);
        if (rest.Count <= 0) return [];
        rest = TryAddExceptHelper<T, Vector3>(ZType.V, ZDOExtraData.s_vec3[zdo.m_uid], rest);
        if (rest.Count > 0) return [];
        rest = TryAddExceptHelper<T, Quaternion>(ZType.Q, ZDOExtraData.s_quats[zdo.m_uid], rest);
        if (rest.Count <= 0) return [];
        rest = TryAddExceptHelper<T, byte[]>(ZType.B, ZDOExtraData.s_byteArrays[zdo.m_uid], rest);
        return (rest as List<T>);
    }

    public List<int> TryAddOnly(ZType type, BinarySearchDictionary<int, int> data, List<int> rest) {
        if (data != null) {
            ZDOInfo info      = ZDOInfo.Instance;
            var     this_vals = this.m_values;
            foreach (var pair_ in data) {
                if (rest.Contains(pair_.Key))
                    rest.Remove(pair_.Key);
                else {
                    GetNameOrDefault(pair_.Key, out var name_, info);
                    this_vals[name_] = new ZValue(type, name_, pair_.Value);
                }
            }
        }
        return rest;
    }
    public void AddPair<T>(T value, string name) {
        if (typeof(T) == typeof(ZDO)) {
            throw new ArgumentException("For adding from a ZDO, use TryAddFromZdo instead");
        }
        m_values.Add(name, new ZValue(GetVType<T>(), name, value));
    }
    public void AddPair<T>(T value, int hash) {
        if (typeof(T) == typeof(ZDO)) {
            throw new ArgumentException("For adding from a ZDO, use TryAddFromZdo instead");
        }
        GetNameOrDefault(hash, out var name);
        m_values.Add(name, new ZValue(GetVType<T>(), name, value));
    }

    public List<int> TryAddOnly(ZDO zdo, params int[] hashes)
        => TryAddOnly<int>(zdo, hashes);

    public List<string> TryAddOnly(ZDO zdo, params string[] names)
        => TryAddOnly<string>(zdo, names);

    public List<int> TryAddOnly<T>(ZDO zdo, params int[] hashes)
        => TryAddOnly<T>(GetVType<T>(), GetDict<T>(zdo), hashes.ToList());

    public List<string> TryAddOnly<T>(ZDO zdo, params string[] names)
        => TryAddOnly<T>(GetVType<T>(), GetDict<T>(zdo), names.ToList());

    static Dictionary<string, ZValue> ReadFromZdo(ZDO zdo) {
        ZDOInfo                    info   = ZDOInfo.Instance;
        Dictionary<string, ZValue> values = new Dictionary<string, ZValue>();
        values = readvalsnew(ZDOExtraData.s_floats[zdo.m_uid], info, values);
        values = readvalsnew(ZDOExtraData.s_vec3[zdo.m_uid], info, values);
        values = readvalsnew(ZDOExtraData.s_quats[zdo.m_uid], info, values);
        values = readvalsnew(ZDOExtraData.s_ints[zdo.m_uid], info, values);
        values = readvalsnew(ZDOExtraData.s_longs[zdo.m_uid], info, values);
        values = readvalsnew(ZDOExtraData.s_strings[zdo.m_uid], info, values);
        values = readvalsnew(ZDOExtraData.s_byteArrays[zdo.m_uid], info, values);
        return values;
    }
    public static BpoZDOVars? MakeFromZdo(ZDO zdo) {
        var values = ReadFromZdo(zdo);
        if (values.Count > 0) { return new BpoZDOVars(values); }
        return null;
    }
    public static bool GetNameOrDefault(int hash, out string name, ZDOInfo info) {
        bool ret = info.HashToName.TryGetValue(hash, out name);
        if (!ret) { name = "Unknown_" + hash; }
        return ret;
    }
    public static bool GetNameOrDefault(int hash, out string name) {
        ZDOInfo info = ZDOInfo.Instance;
        return GetNameOrDefault(hash, out name, info);
    }

    public bool TryAddFromZdo<T>(ZDO zdo, int hash) {
        GetNameOrDefault(hash, out var name);
        if (GetDict<T>(zdo)?.TryGetValue(hash, out T value) ?? false) {
            m_values[name] = new ZValue(GetVType<T>(), name, value);
            return true;
        }
        return false;
    }

    public bool TryAddFromZdo<T>(ZDO zdo, string name) {
        int hash = name.GetStableHashCode();
        if (GetDict<T>(zdo)?.TryGetValue(hash, out T value) ?? false) {
            m_values[name] = new ZValue(GetVType<T>(), name, value);
            return true;
        }
        return false;
    }

    /*
    public void InitFromJson(string json) {
        string[] parts = json.Trim().Split(new[] { ':' }, 2);
        if ((parts.Length > 1) && (parts[0].Contains("ZDOVars"))){
            var matches =
                Regex.Matches(parts[1],
                    @"\{(.*?)\}"); // without partensis
            foreach (Match match in matches)
            {
                parts = match.Value.Split(new[] { ',' }, 2);
                var pattern = "[^a-zA-Z0-9]+"; // remove all non alphanumeric characters
                var result  = Regex.Replace(parts[0], pattern, "").ToLowerInvariant();
                switch (result)
                {
                    case ("floats"):
                    case ("vecs"):
                    case ("quats"):
                    case ("ints"):
                    case ("longs"):
                    case ("strings"):
                    case ("bytearrays"):
                }


                IndexOf
                String.IndexOf(match.Value.Split());
            }

            ZDOInfo info = ZDOInfo.Instance;
            ZDOID   id   = zdo.m_uid;
            floats = readvals(ZDOExtraData.s_floats[zdo.m_uid], info, x => x);
            vecs = readvals(ZDOExtraData.s_vec3[zdo.m_uid], info, x => {
                return new float[3] { x.x, x.y, x.z };
            }
        });

        quats = readvals(ZDOExtraData.s_quats[zdo.m_uid], info, x => {
            return new float[] { x.x, x.y, x.z, x.w };
        });

        ints    = readvals(ZDOExtraData.s_ints[zdo.m_uid], info, x => x);
        longs   = readvals(ZDOExtraData.s_longs[zdo.m_uid], info, x => x);
        strings = readvals(ZDOExtraData.s_strings[zdo.m_uid], info, x => x);
        bytearrays = readvals(ZDOExtraData.s_byteArrays[zdo.m_uid], info, x => {
            ZPackage pkg = new ZPackage();
            pkg.Write(x);
            return pkg.GetBase64();
        });
    }*/

    static Dictionary<string, ZValue> readvalsnew<T>(
        BinarySearchDictionary<int, T>? import,
        ZDOInfo                         info,
        Dictionary<string, ZValue>      values) {
        var type = GetVType<T>();
        if (import != null) {
            import.Select(x => x).Aggregate(
                values,
                (target, pair) => {
                    GetNameOrDefault(pair.Key, out var name, info);
                    T val = pair.Value;
                    target[name] = new ZValue(type, name, val);
                    return target;
                }
            );
        }
        return values;
    }
    static ZType GetVType<T>() {
        return typeof(T) switch {
            Type t when t == typeof(int) => ZType.I,
            Type t when t == typeof(float) => ZType.F,
            Type t when t == typeof(Quaternion) => ZType.Q,
            Type t when t == typeof(Vector3) => ZType.V,
            Type t when t == typeof(long) => ZType.L,
            Type t when t == typeof(string) => ZType.S,
            Type t when t == typeof(byte[]) => ZType.B,
            _ => throw new ArgumentException($"Unsupported data type: {typeof(T)}"),
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
            _ => throw new ArgumentException($"Unsupported data type: {typeof(T)}")
        } ?? [];
    }
}

public interface IBlueprintObject
{
    string     Prefab { get; set; }
    Vector3    Pos    { get; set; }
    Quaternion Rot    { get; set; }
    // string Data { get; set; }
    Vector3 Scale  { get; set; }
    float   Chance { get; set; }
    //  string ExtraInfo { get; set; }
}

[Serializable]
public class BpjObject : IBlueprintObject
{
    [JsonIgnore] // the prefab will saved as key
    public string m_prefab = "";
    [JsonPropertyName("Data")]    public TData      m_data;
    [JsonPropertyName("ZDOVars")] public BpoZDOVars m_properties;

    public BpoZDOVars ZVars { get => m_properties; set => m_properties = value; }
    internal BpjObject() {
        m_prefab = "";
        m_data = new TData(0, Vector3.zero, Quaternion.identity, Vector3.one,
            0f);
    }
    public BpjObject(string prefab, TData data, BpoZDOVars? properties = null) {
        this.m_prefab     = prefab;
        this.m_data       = data;
        this.m_properties = properties ?? new BpoZDOVars();
    }
    public BpjObject(
        string  prefab, Vector3 pos, Quaternion rot,
        Vector3 scale,
        float   chance) {
        m_prefab     = prefab;
        this.m_data  = new TData(0, pos, rot, scale, chance);
        m_properties = new BpoZDOVars();
    }

    public BpjObject(
        string  prefab, Vector3 pos, Quaternion rot,
        Vector3 scale,
        string  info,
        string  data, float chance) {
        m_prefab     = prefab;
        this.m_data  = new TData(0, pos, rot, scale, chance);
        m_properties = new BpoZDOVars();
    }
    public BpjObject(
        string   mPrefab,   GameObject? obj, bool saveData,
        BPOFlags flags = 0, BpoZDOVars? vars = null) {
        this.m_prefab = mPrefab;
        this.m_data = new TData(flags,
            obj.transform.localPosition, obj.transform.localRotation,
            obj.transform.localScale, 1f);
        this.m_properties = vars ?? new BpoZDOVars();
    }

    [Serializable]
    public struct TData
    {
        public BPOFlags flags;
        public float[]  p;
        public float[]  r;
        public float[]  s;
        public float    odds;

        public TData(
            BPOFlags flags, Vector3 pos, Quaternion rot, Vector3 scale,
            float    chance) {
            var norm = rot.normalized;
            this.flags = flags;
            this.p     = new[] { pos.x, pos.y, pos.z };
            this.r     = new[] { norm.x, norm.y, norm.z, norm.w };
            this.s     = new[] { scale.x, scale.y, scale.z };
            this.odds  = chance;
        }
    }

    static internal string EscapeString(string s) {
        string pattern = @"[\\\""\b\f\n\r\t]";
        return Regex.Replace(s, pattern, match => match.Value switch {
            "\\" => @"\\", "\"" => @"\""", "\b" => @"\b", "\f" => @"\f",
            "\n" => @"\n",
            "\r" => @"\r", "\t" => @"\t", _ => match.Value
        });
    }
    static internal string UnescapeString(string input) {
        string pattern = @"\\[\\\""bfnrt]";
        return Regex.Replace(input, pattern, match => match.Value switch {
            @"\\" => "\\", @"\""" => "\"", @"\b" => "\b", @"\f" => "\f",
            @"\n" => "\n",
            @"\r" => "\r", @"\t" => "\t", _ => match.Value
        });
    }

    public string ToJson() {
        var options = new JsonSerializerOptions {
            WriteIndented = false
        };
        options.Converters.Add(new BpoConverter());
        options.Converters.Add(new Vec3JsonConverter());
        options.Converters.Add(new QuatJsonConverter());

        return JsonSerializer.Serialize(this, options);
        /*var prefab = this.m_prefab;
        var data   = JsonUtility.ToJson(this.m_data);
        if (m_properties.Count == 0)
        {
            return $"{{\"{prefab}\":{data}}}";
        } else
        {
            var extended = string.Join(",",
                this.m_properties.Select(x => x.toJson()));
            return $"{{\"{prefab}\":{data},\"ext\":{extended}}}";
        }*/
    }
    public static BlueprintJson.BpjLine Split(string line) {
        string[] parts = line.Split(new[] { ':' }, 2);
        string[] parts2 = parts[1].Split(new[] { "\"ext\":" },
            StringSplitOptions.None);
        var    prefab = parts[0].Trim('{', '"');
        string data_;

        if (parts2.Length == 1) {
            // contains no additional data 
            data_ = parts2[0].TrimEnd('}', ',') + "}";
            return new BlueprintJson.BpjLine(prefab, data_, []);
        } else if (parts2.Length > 1) // todo move to import loop
        {
            data_ = parts2[0].TrimEnd(',');
            //var matches = Regex.Matches(parts2[1], @"\{.*?\}");// with partensis 
            var matches =
                Regex.Matches(parts2[1],
                    @"\{(.*?)\}"); // without partensis 
            string[] ext_ = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++) { ext_[i] = matches[i].Value; }
            return new BlueprintJson.BpjLine(prefab, data_, ext_);
        }

        throw new ArgumentException("incorrect format: " + line);
    }

// todo remove 
    public static BpjObject? FromJson(BlueprintJson.BpjLine line) {
        var prefab = line.prefab;
        if (line.ext.Length == 0) {
            // contains no additional data 
            return new BpjObject(prefab,
                JsonUtility.FromJson<TData>(line.data));
        } // todo move to import loop

        var obj = new BpjObject(prefab,
            JsonUtility.FromJson<TData>(line.data));
        foreach (var match in line.ext) {
            // todo does nothing atm
            System.Console.WriteLine(match);
        }
        return obj;
    }
    public static BpjObject? FromJson(string line) {
        string[] parts = line.Split(new[] { ':' }, 2);
        string[] parts2 = parts[1].Split(new[] { "\"ext\":" },
            StringSplitOptions.None);

        try {
            var prefab = parts[0].Trim('{', '"');
            if (parts2.Length == 1) {
                // contains no additional data 
                return new BpjObject(prefab,
                    JsonUtility.FromJson<TData>(parts2[0].TrimEnd('}', ',') +
                        "}"));
            } else if (parts2.Length > 1) // todo move to import loop
            {
                var obj = new BpjObject(prefab,
                    JsonUtility.FromJson<TData>(parts2[0].TrimEnd(',')));
                //var matches = Regex.Matches(parts2[1], @"\{.*?\}");// with partensis 
                var matches =
                    Regex.Matches(parts2[1],
                        @"\{(.*?)\}"); // without partensis 

                foreach (Match match in matches) { System.Console.WriteLine(match.Value); }

                return obj;
            }

            throw new ArgumentException("Parts2.length" +
                parts2.Length.ToString());
        } catch (Exception e) {
            System.Console.WriteLine(e);
            System.Console.WriteLine(parts[0].TrimStart('{'));
        }

        return new BpjObject();
    }
    public virtual string Prefab { get => m_prefab; set => m_prefab = value; }
    public virtual Vector3 Pos {
        get => new Vector3(m_data.p[0], m_data.p[1], m_data.p[2]);
        set => m_data.p = [value.x, value.y, value.z];
    }
    public virtual Quaternion Rot {
        get => new Quaternion(m_data.r[0], m_data.r[1], m_data.r[2],
            m_data.r[3]);
        set => m_data.r = [value.x, value.y, value.z, value.w];
    }
// todo write conversion function for other blueprint format    
    public virtual string Data { get => ""; set { } }
    public virtual Vector3 Scale {
        get => new Vector3(m_data.s[0], m_data.s[1], m_data.s[2]);
        set => m_data.s = [value.x, value.y, value.z];
    }
// todo write conversion function for other blueprint format    
    public virtual float    Chance    { get => m_data.odds;  set => m_data.odds = value; }
    public virtual string   ExtraInfo { get => "";           set { } }
    public virtual BPOFlags Flags     { get => m_data.flags; set => m_data.flags = value; }
}
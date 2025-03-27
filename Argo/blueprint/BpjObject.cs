using System;
using System.Data;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Argo.blueprint.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Argo.Blueprint;

using DOdata = ZDOExtraData;
using static BpjZVars;

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
    IsVanilla = 1 << 0, // todo add for every vanilla piece
}

public class BpjDataConverter : JsonConverter<BpjObject.TData>
{
    static readonly Vec3JsonConverter vec3JsonConverter = new Vec3JsonConverter();
    static readonly QuatJsonConverter quatJsonConverter = new QuatJsonConverter();
    public override void Write(
        Utf8JsonWriter        writer, BpjObject.TData data,
        JsonSerializerOptions options) {
        try {
            writer.WriteStartObject();
            writer.WritePropertyName( "flags" );
            writer.WriteNumberValue( (uint)(data.flags) );
            /*writer.WritePropertyName("p");
            JsonSerializer.Serialize(writer, (Vector3)(data.p), options);*/
            writer.WritePropertyName( "p" );
            vec3JsonConverter.Write( writer, data.p, options );
            writer.WritePropertyName( "r" );
            quatJsonConverter.Write( writer, data.r, options );
            writer.WritePropertyName( "s" );
            vec3JsonConverter.Write( writer, data.s, options );

            /*
            writer.WriteStartArray();
            writer.WriteNumberValue(data.p.x);
            writer.WriteNumberValue(data.p.y);
            writer.WriteNumberValue(data.p.z);
            writer.WriteEndArray();
            writer.WritePropertyName("r");
            writer.WriteStartArray();
            writer.WriteNumberValue(data.r.x);
            writer.WriteNumberValue(data.r.y);
            writer.WriteNumberValue(data.r.z);
            writer.WriteNumberValue(data.r.w);
            writer.WriteEndArray();
            writer.WritePropertyName("s");
            writer.WriteStartArray();
            writer.WriteNumberValue(data.s.x);
            writer.WriteNumberValue(data.s.y);
            writer.WriteNumberValue(data.s.z);
            writer.WriteEndArray();*/
            /*writer.WritePropertyName("s");
            JsonSerializer.Serialize(writer, (Vector3)(data.s), options);
            */
            writer.WritePropertyName( "odds" );
            writer.WriteNumberValue( data.odds );
            writer.WriteEndObject();
        } catch (Exception e) {
            System.Console.WriteLine( "Error in Json Serializer Bpjobject " + e );
            throw e;
        }
    }
    public override BpjObject.TData Read(
        ref Utf8JsonReader    reader, Type type,
        JsonSerializerOptions options) {
        reader.Read();
        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();
        reader.Read();
        BPOFlags   flags  = 0;
        Vector3    p      = Vector3.zero;
        Quaternion r      = Quaternion.identity;
        Vector3    s      = Vector3.zero;
        float      odds   = 0f;
        while (reader.TokenType != JsonTokenType.EndObject) {
            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();
            string     p_name = reader.GetString() ?? "";
            switch (p_name) {
                case "flags":
                    reader.Read();
                    flags = (BPOFlags)reader.GetUInt32();
                    break;
                case "p":
                    p= vec3JsonConverter.Read( ref reader, typeof(Vector3), options );
                    break;
                case "r":
                    r = quatJsonConverter.Read( ref reader, typeof(Quaternion), options );
                    break;
                case "s":
                    s = vec3JsonConverter.Read( ref reader, typeof(Vector3), options );
                    break;
                case "odds":
                    reader.Read();
                    odds = reader.GetSingle();
                    break;
            }
            reader.Read();
        }
       return new BpjObject.TData( flags, p, r, s, odds );
    }
}

public class BpjObjectConverter : JsonConverter<BpjObject>
{
    private readonly static BpjDataConverter TDataConverter = new BpjDataConverter();
    private readonly static ZdoConverter     zdoConverter   = new ZdoConverter();
    public override void Write(
        Utf8JsonWriter        writer, BpjObject? Bpo,
        JsonSerializerOptions options) {
        if (Bpo == null) { return; }
        try {
            writer.WriteStartObject();
            writer.WritePropertyName( Bpo.m_prefab );
            /*writer.WriteStartObject();
            writer.WritePropertyName("flags");
            writer.WriteNumberValue((uint)(Bpo.m_data.flags));
            /*writer.WritePropertyName("p");
            JsonSerializer.Serialize(writer, (Vector3)(data.p), options);#1#
            writer.WritePropertyName("p");
            writer.WriteStartArray();
            writer.WriteNumberValue(Bpo.m_data.p.x);
            writer.WriteNumberValue(Bpo.m_data.p.y);
            writer.WriteNumberValue(Bpo.m_data.p.z);
            writer.WriteEndArray();
            writer.WritePropertyName("r");
            writer.WriteStartArray();
            writer.WriteNumberValue(Bpo.m_data.r.x);
            writer.WriteNumberValue(Bpo.m_data.r.y);
            writer.WriteNumberValue(Bpo.m_data.r.z);
            writer.WriteNumberValue(Bpo.m_data.r.w);
            writer.WriteEndArray();
            writer.WritePropertyName("s");
            writer.WriteStartArray();
            writer.WriteNumberValue(Bpo.m_data.s.x);
            writer.WriteNumberValue(Bpo.m_data.s.y);
            writer.WriteNumberValue(Bpo.m_data.s.z);
            writer.WriteEndArray();
            /*writer.WritePropertyName("s");
            JsonSerializer.Serialize(writer, (Bpo.m_data.s), options);
            #1#

            writer.WritePropertyName("odds");
            writer.WriteNumberValue(Bpo.m_data.odds);
            writer.WriteEndObject();*/

            /*writer.WritePropertyName("flags");
            writer.WriteNumberValue((uint)(Bpo.Flags));
            writer.WritePropertyName("p");
            JsonSerializer.Serialize(writer, Bpo.Pos, options);
            writer.WritePropertyName("r");
            JsonSerializer.Serialize(writer, Bpo.Rot, options);
            writer.WritePropertyName("s");
            JsonSerializer.Serialize(writer, Bpo.Scale, options);
            writer.WritePropertyName("odds");
            writer.WriteNumberValue(Bpo.Chance);*/
            TDataConverter.Write( writer, Bpo.m_data, options );
            //JsonSerializer.Serialize<BpjObject.TData>(writer, Bpo.m_data, options);
            if (Bpo.m_properties.Count != 0) {
                writer.WritePropertyName( "ZDOVars" );
                writer.WriteStartObject();
                foreach (var pair in Bpo.m_properties.m_values) {
                    zdoConverter.Write( writer, pair.Value, options );
                }
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
            writer.WriteRawValue( "\n", true );
        } catch (Exception e) {
            System.Console.WriteLine( "Error in Json Serializer Bpjobject " + e );
            throw e;
        }
    }

    public override BpjObject Read(
        ref Utf8JsonReader    reader, Type type,
        JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException( "Expected start of of object." );

        try {
            reader.Read();
            string prefab = reader.GetString();
            BpjObject.TData data
                = JsonSerializer.Deserialize<BpjObject.TData>( ref reader, options );
            BpjObject bpo = new BpjObject( prefab, data );
            reader.Read();
            if (reader.TokenType == JsonTokenType.EndObject)
                return bpo;

            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();

            string temp = reader.GetString() ?? "";
            if (string.Equals( temp, "ZDOVars",
                    StringComparison.InvariantCultureIgnoreCase )) {
                reader.Read();
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException( "Expected start of Object." );
                reader.Read();

                BpjZVars values = new BpjZVars();
                while (reader.TokenType != JsonTokenType.EndObject) {
                    ZValue v = zdoConverter.Read( ref reader, typeof(ZValue), options );
                    // ZValue v = JsonSerializer.Deserialize<ZValue>(ref reader, options);
                    values.m_values.Add( v.Name, v );
                    reader.Read();
                }
                bpo.ZVars = values;
            }
            if (reader.TokenType != JsonTokenType.EndObject)
                throw new JsonException( "Expected end of Object." );
            return bpo;
        } catch (Exception e) {
            System.Console.WriteLine( "Error in Json Serializer BpjObject" + e );
            throw e;
        }
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
    [JsonPropertyName( "Data" )]    public TData    m_data;
    [JsonPropertyName( "ZDOVars" )] public BpjZVars m_properties;

    [JsonIgnore] public BpjZVars ZVars { get => m_properties; set => m_properties = value; }
    internal BpjObject() {
        m_prefab = "";
        m_data = new TData( 0, Vector3.zero, Quaternion.identity, Vector3.one,
            0f );
    }
    public BpjObject(string prefab, TData data, BpjZVars? properties = null) {
        this.m_prefab     = prefab;
        this.m_data       = data;
        this.m_properties = properties ?? new BpjZVars();
    }
    public BpjObject(
        string  prefab, Vector3 pos, Quaternion rot,
        Vector3 scale,
        float   chance) {
        m_prefab     = prefab;
        this.m_data  = new TData( 0, pos, rot, scale, chance );
        m_properties = new BpjZVars();
    }

    public BpjObject(
        string  prefab, Vector3 pos, Quaternion rot,
        Vector3 scale,
        string  info,
        string  data, float chance) {
        m_prefab     = prefab;
        this.m_data  = new TData( 0, pos, rot, scale, chance );
        m_properties = new BpjZVars();
    }
    public BpjObject(
        string   mPrefab,   GameObject? obj, bool saveData,
        BPOFlags flags = 0, BpjZVars?   vars = null) {
        this.m_prefab = mPrefab;
        this.m_data = new TData( flags,
            obj.transform.localPosition, obj.transform.localRotation,
            obj.transform.localScale, 1f );
        this.m_properties = vars ?? new BpjZVars();
    }

    public class TData
    {
        [JsonInclude] public BPOFlags   flags;
        [JsonInclude] public Vector3    p;
        [JsonInclude] public Quaternion r;
        [JsonInclude] public Vector3    s;
        [JsonInclude] public float      odds;

        public TData(
            BPOFlags flags, Vector3 pos, Quaternion rot, Vector3 scale,
            float    chance) {
            var norm = rot.normalized;
            this.flags = flags;
            this.p     = pos;
            this.r     = norm;
            this.s     = scale;
            this.odds  = chance;
        }
    }

    static internal string EscapeString(string s) {
        string pattern = @"[\\\""\b\f\n\r\t]";
        return Regex.Replace( s, pattern, match => match.Value switch {
            "\\" => @"\\", "\"" => @"\""", "\b" => @"\b", "\f" => @"\f",
            "\n" => @"\n",
            "\r" => @"\r", "\t" => @"\t", _ => match.Value
        } );
    }
    static internal string UnescapeString(string input) {
        string pattern = @"\\[\\\""bfnrt]";
        return Regex.Replace( input, pattern, match => match.Value switch {
            @"\\" => "\\", @"\""" => "\"", @"\b" => "\b", @"\f" => "\f",
            @"\n" => "\n",
            @"\r" => "\r", @"\t" => "\t", _ => match.Value
        } );
    }

    public string ToJson() {
        var options = new JsonSerializerOptions {
            WriteIndented = false
        };
        /*
        options.Converters.Add(new BpjObjectConverter());
        options.Converters.Add(new Vec3JsonConverter());
        options.Converters.Add(new QuatJsonConverter());
        */

        return JsonSerializer.Serialize( this, options );
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
        string[] parts = line.Split( new[] { ':' }, 2 );
        string[] parts2 = parts[1].Split( new[] { "\"ext\":" },
            StringSplitOptions.None );
        var    prefab = parts[0].Trim( '{', '"' );
        string data_;

        if (parts2.Length == 1) {
            // contains no additional data 
            data_ = parts2[0].TrimEnd( '}', ',' ) + "}";
            return new BlueprintJson.BpjLine( prefab, data_, [] );
        } else if (parts2.Length > 1) // todo move to import loop
        {
            data_ = parts2[0].TrimEnd( ',' );
            //var matches = Regex.Matches(parts2[1], @"\{.*?\}");// with partensis 
            var matches =
                Regex.Matches( parts2[1],
                    @"\{(.*?)\}" ); // without partensis 
            string[] ext_ = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++) { ext_[i] = matches[i].Value; }
            return new BlueprintJson.BpjLine( prefab, data_, ext_ );
        }

        throw new ArgumentException( "incorrect format: " + line );
    }

// todo remove 
    public static BpjObject? FromJson(BlueprintJson.BpjLine line) {
        var prefab = line.prefab;
        if (line.ext.Length == 0) {
            // contains no additional data 
            return new BpjObject( prefab,
                JsonUtility.FromJson<TData>( line.data ) );
        } // todo move to import loop

        var obj = new BpjObject( prefab,
            JsonUtility.FromJson<TData>( line.data ) );
        foreach (var match in line.ext) {
            // todo does nothing atm
            System.Console.WriteLine( match );
        }
        return obj;
    }
    public static BpjObject? FromJson(string line) {
        string[] parts = line.Split( new[] { ':' }, 2 );
        string[] parts2 = parts[1].Split( new[] { "\"ext\":" },
            StringSplitOptions.None );

        try {
            var prefab = parts[0].Trim( '{', '"' );
            if (parts2.Length == 1) {
                // contains no additional data 
                return new BpjObject( prefab,
                    JsonUtility.FromJson<TData>( parts2[0].TrimEnd( '}', ',' ) +
                        "}" ) );
            } else if (parts2.Length > 1) // todo move to import loop
            {
                var obj = new BpjObject( prefab,
                    JsonUtility.FromJson<TData>( parts2[0].TrimEnd( ',' ) ) );
                //var matches = Regex.Matches(parts2[1], @"\{.*?\}");// with partensis 
                var matches =
                    Regex.Matches( parts2[1],
                        @"\{(.*?)\}" ); // without partensis 

                foreach (Match match in matches) { System.Console.WriteLine( match.Value ); }

                return obj;
            }

            throw new ArgumentException( "Parts2.length" +
                parts2.Length.ToString() );
        } catch (Exception e) {
            System.Console.WriteLine( e );
            System.Console.WriteLine( parts[0].TrimStart( '{' ) );
        }

        return new BpjObject();
    }
    [JsonIgnore] public virtual string     Prefab { get => m_prefab; set => m_prefab = value; }
    [JsonIgnore] public virtual Vector3    Pos    { get => m_data.p; set => m_data.p = value; }
    [JsonIgnore] public virtual Quaternion Rot    { get => m_data.r; set => m_data.r = value; }
// todo write conversion function for other blueprint format    
    [JsonIgnore] public virtual string  Data  { get => "";       set { } }
    [JsonIgnore] public virtual Vector3 Scale { get => m_data.s; set => m_data.s = value; }
// todo write conversion function for other blueprint format    
    [JsonIgnore] public virtual float    Chance { get => m_data.odds; set => m_data.odds = value; }
    [JsonIgnore] public virtual string   ExtraInfo { get => ""; set { } }
    [JsonIgnore] public virtual BPOFlags Flags { get => m_data.flags; set => m_data.flags = value; }
}
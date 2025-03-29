using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Argo.blueprint.Util;
using UnityEngine;

namespace Argo.Blueprint;

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
    BuildPiece = 1u << 31,
    // BuildPiecePlayer = 1u << 30, //	 for pieces buildable by the player with the hammer
    BuildPlayer = 1u << 30,
    //	 for all build piece even spawn only like the dverger probs
    Placeable = 1 << 29, //	for pieces like food placed with the serving tray
    //	 for the option to exclude compfort pieces from static pieces (which do nothing)
    Compfort    = 1 << 28,
    LightSource = 1 << 27, //	 for pieces like armor stands and item stands	

    Hoverable    = 1 << 25, // todo add those tags, kinda forgott
    TextReceiver = 1 << 24, //		
    Interactable = 1 << 23, //		
//	 for containers like the chest and things that need	have fuel
// todo, can maybe combined with containerpiece & pickable
    ObjectHolder    = 1 << 21,
    ContainerPiece  = 1 << 20, //		
    CraftingStation = 1 << 19, //		
    // todo, can maybe combined with Craftig station and Containerpiece
    Fuel            = 1 << 18, //	 for pieces with fuel like the furnace	
    // todo maybe combine with placeable, if its a buildpieceplayer that means ists like
    // placed food, if only buildpiece its like the pickable dverger lanterns and if its neither
    // its like muhsrooms, rasperries and wild crops 
    // wild mushrooms seem to have ZNetView, StaticPhysics and Pickable
    // placed meads (same for dear mead) have: ZNetView, ZSyncTransform, ItemDrop, Piece, WearNTear, MaterialManNofier
    Pickable        = 1 << 17, //	 e.g for food placed with the serving tray or crops	
//	 for player grown pieces which need cultivated ground (vegtables basically)
    Cultivated            = 1 << 15,
    DestroyableTerrain    = 1 << 14, //	 Objects like trees and rocks 	
    Fractured             = 1 << 13, //	 for partially broken stones etc	
    HoverableResourceNode = 1 << 12, //	 	
    NonStatic
        = 1 << 11, //	 pieces with some sort of funktion but not direktly interactable	 like ladders and whisplights
    Creature = 1 << 10, //	 for all creatures	
    // can maybe combined with TextReceiver and creature
    Tameable = 1 << 9, //	 for all creatures that can be tamed
    Vehicle  = 1 << 8, //	 for all vehicles
//	 for player grown pieces which need cultivated ground (vegtables basically)
    Animated = 1 << 6,
    //	 for stuff like WaterInteractable  PieceMarker ... which are pretty rare      PieceMarker  WaterInteractable  DooDadControler  Projectile: condensed into Special Interface
    SpecialInterface = 1 << 5,
    //	 for interfaces Hoverable	 IHasHoverMenu & IHasHoverMenuExtended
    Indestructible = 1 << 4,
    // todo  
    NoCollider = 1 << 3, 
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
            writer.WritePropertyName( "p" );
            vec3JsonConverter.Write( writer, data.pos, options );
            writer.WritePropertyName( "r" );
            quatJsonConverter.Write( writer, data.rot, options );
            writer.WritePropertyName( "s" );
            vec3JsonConverter.Write( writer, data.scale, options );
            writer.WritePropertyName( "odds" );
            writer.WriteNumberValue( data.chance );
            writer.WriteEndObject();
        } catch (Exception e) {
            System.Console.WriteLine( "Error in Json Serializer Bpjobject " + e );
            throw;
        }
    }
    public override BpjObject.TData Read(
        ref Utf8JsonReader    reader, Type type,
        JsonSerializerOptions options) {
        reader.Read();
        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();
        reader.Read();
        BPOFlags   flags = 0;
        Vector3    p     = Vector3.zero;
        Quaternion r     = Quaternion.identity;
        Vector3    s     = Vector3.zero;
        float      odds  = 0f;
        while (reader.TokenType != JsonTokenType.EndObject) {
            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();
            string p_name = reader.GetString() ?? "";
            switch (p_name) {
                case "flags":
                    reader.Read();
                    flags = (BPOFlags)reader.GetUInt32();
                    break;
                case "p":
                    p = vec3JsonConverter.Read( ref reader, typeof(Vector3), options );
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
                default:
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
        try {
            reader.Read();
            if (reader.TokenType == JsonTokenType.StartObject) {
                reader.Read();
            }
            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException( "BlueprintObject3: Expected PropertyName." );
            }
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
                    throw new JsonException( "BlueprintObject2: Expected start of Object." );
                

                BpjZVars values = new BpjZVars();
                do {
                  
                    ZValue v    = zdoConverter.Read( ref reader, typeof(ZValue), options );
                    // ZValue v = JsonSerializer.Deserialize<ZValue>(ref reader, options);
                    values.m_values.Add( v.Name, v );
                    reader.Read();
                } while (reader.TokenType != JsonTokenType.EndObject);
                reader.Read();
                bpo.ZVars = values;
            }
            if (reader.TokenType != JsonTokenType.EndObject)
                throw new JsonException( "Expected end of Object." );
            reader.Read();
            return bpo;
        } catch (Exception e) {
            System.Console.WriteLine( "Error in Json Serializer BpjObject" + e );
            System.Console.WriteLine( "TokenType:" + reader.TokenType.ToString() );
            System.Console.WriteLine( "Bytes" + reader.BytesConsumed );
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
        this.m_prefab     = prefab;
        this.m_data       = new TData( 0, pos, rot, scale, chance );
        this.m_properties = new BpjZVars();
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

    public struct TData()
    {
        [JsonInclude] [JsonPropertyName( "flags" )] public BPOFlags   flags;
        [JsonInclude] [JsonPropertyName( "p" )]     public Vector3    pos;
        [JsonInclude] [JsonPropertyName( "r" )]     public Quaternion rot;
        [JsonInclude] [JsonPropertyName( "s" )]     public Vector3    scale;
        [JsonInclude] [JsonPropertyName( "odds" )]  public float      chance;

        public TData(
            BPOFlags flags, Vector3 p, Quaternion r, Vector3 s,
            float    chance) : this() {
            //var norm = rot.normalized;
            this.flags  = flags;
            this.pos    = p;
            this.rot    = r;
            this.scale  = s;
            this.chance = chance;
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
    /*public static BlueprintJson.BpjLine Split(string line) {
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
    }*/

// todo remove 
    /*public static BpjObject? FromJson(BlueprintJson.BpjLine line) {
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
    }*/
    /*public static BpjObject? FromJson(string line) {
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
    }*/
    [JsonIgnore] public virtual string     Prefab { get => m_prefab;   set => m_prefab = value; }
    [JsonIgnore] public virtual Vector3    Pos    { get => m_data.pos; set => m_data.pos = value; }
    [JsonIgnore] public virtual Quaternion Rot    { get => m_data.rot; set => m_data.rot = value; }
// todo write conversion function for other blueprint format    
    [JsonIgnore] public virtual string  Data  { get => "";           set { } }
    [JsonIgnore] public virtual Vector3 Scale { get => m_data.scale; set => m_data.scale = value; }
// todo write conversion function for other blueprint format    
    [JsonIgnore] public virtual float Chance { get => m_data.chance; set => m_data.chance = value; }
    [JsonIgnore] public virtual string ExtraInfo { get => ""; set { } }
    [JsonIgnore] public virtual BPOFlags Flags { get => m_data.flags; set => m_data.flags = value; }
}
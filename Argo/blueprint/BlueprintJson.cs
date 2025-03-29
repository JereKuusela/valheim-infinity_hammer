using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Argo.blueprint.Util;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Argo.Blueprint;

public struct BlueprintMod
{
    [JsonInclude] public string Name        = "";
    [JsonInclude] public string Version     = "";
    [JsonInclude] public string Url         = "";
    [JsonInclude] public string Description = "";
    public BlueprintMod() { }
}

public struct BlueprintHeader()
{
    [JsonInclude] public string  Name        = "";
    [JsonInclude] public string  Creator     = "";
    [JsonInclude] public string  Description = "";
    [JsonInclude] public string  CenterPiece = "";
    [JsonInclude] public Vector3 Coordinates = Vector3.zero;
    [JsonInclude] public Vector3 Rotation    = Vector3.zero;
    [JsonInclude] public string  Category    = "InfinityHammer.Json";
    [JsonInclude] public float   Radius      = 0f;
    [JsonInclude] public string  Version     = "0.1.0";
    [JsonInclude]
    public List<BlueprintMod> Mods = new List<BlueprintMod>
        { new BlueprintMod { Name = "InfinityHammer", Version = "1.64.0" } };
    [JsonInclude]  public bool          Sorted     = true;
    [JsonInclude] public List<Vector3> SnapPoints = [];
    public BlueprintHeader(string centerPiece) : this() { }
};

public class BlueprintConverter : JsonConverter<BlueprintJson>
{
    private static          BpHeaderConverter     HeaderConverter = new BpHeaderConverter();
    private static          BpjObjectConverter    ObjectConverter = new BpjObjectConverter();
    private static readonly JsonSerializerOptions options;
    static BlueprintConverter() {
        options = new JsonSerializerOptions {
            WriteIndented = false
        };
        //     IntPtrCheck.CheckDeepForIntPtr(this.GetType());
        options.Converters.Add( new BpjObjectConverter() );
        options.Converters.Add( new Vec3JsonConverter() );
        options.Converters.Add( new QuatJsonConverter() );
        options.Converters.Add( new ZdoConverter() );
        // For performance, use the existing converter.
    }
    public override void Write(
        Utf8JsonWriter        writer, BlueprintJson? bp,
        JsonSerializerOptions options) {
        if (bp == null) { return; }
        try {
            writer.WriteStartObject();
            writer.WritePropertyName( "header" );
            writer.WriteRawValue( "\n", true );
            HeaderConverter.Write( writer, bp.header, options );
            if (bp.objects.Count > 0) {
                writer.WritePropertyName( "objects" );
                writer.WriteStartArray();
                writer.WriteRawValue( "\n", true );
                foreach (var bpo in bp.objects) {
                    ObjectConverter.Write( writer, bpo, options );
                }
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        } catch (Exception e) {
            System.Console.WriteLine( "Error in Json Serializer Bpjobject " + e );
            throw;
        }
    }

    public override BlueprintJson Read(
        ref Utf8JsonReader    reader, Type type,
        JsonSerializerOptions options) {
        /*
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException( "Expected start of of object." );
            */

        try {
//            reader.Read();
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException( "Blueprint0: Expected start of Object." );
            var objects = new List<BpjObject?>();
            var header  = new BlueprintHeader();

            while (reader.Read()) {
                if (reader.TokenType == JsonTokenType.EndObject) {
                    break;
                }
                string property = reader.GetString() ?? string.Empty;
                switch (property) {
                    case "header":

                        header = HeaderConverter.Read( ref reader, typeof(BlueprintHeader),
                            options );
                        break;
                    case "objects":
                        reader.Read();
                        if (reader.TokenType != JsonTokenType.StartArray)
                            throw new JsonException( "Blueprint0: Expected start of Array." );
                        while (reader.TokenType != JsonTokenType.EndArray) {
                            objects.Add( ObjectConverter.Read( ref reader, typeof(BpjObject),
                                options ) );
                        }
                        break;
                    default:
                        break;
                }
            }
            return new BlueprintJson( header, objects );
        } catch (Exception e) {
            System.Console.WriteLine( "Error in Json Serializer BpjObject" + e );
            System.Console.WriteLine( "TokenType:" + reader.TokenType.ToString() );
            System.Console.WriteLine( "Bytes" + reader.BytesConsumed );
            throw e;
        }
    }
}

public interface IBlueprint
{
    string        Name        { get; set; }
    string        Description { get; set; }
    string        Creator     { get; set; }
    Vector3       Coordinates { get; set; }
    Vector3       Rotation    { get; set; }
    string        CenterPiece { get; set; }
    List<Vector3> SnapPoints  { get; set; }
    float         Radius      { get; set; }
    Vector3       Center(string centerPiece);
}

public class BlueprintJson : IBlueprint
{
    [JsonIgnore]  public Player?            player;
    [JsonIgnore]  public string             snapPiece; // todo add branche for snappiece
    [JsonIgnore]  public bool               saveData;
    [JsonInclude] public BlueprintHeader    header;
    [JsonInclude] public List<BpjObject?>   objects;
    [JsonIgnore]  public BpjObjectFactory   factory;
    [JsonIgnore]  public List<BPListObject> GameObjects;
    [JsonIgnore]  public SelectionBase?     selection;
    [JsonIgnore]  public List<BpjLine>      m_lines;

    public BpjObjectFactory Factory { get => factory; internal set => factory = value; }
    /// <summary>
    /// since the Factory is a MonoBehaviour it needs an game object attached to
    /// so unity can keep its coroutines running
    /// </summary>
    [JsonIgnore]
    public GameObject? facWrapper = null;
    [JsonIgnore] internal readonly BpoRegister bpoRegister;

    internal BlueprintJson(
        string playerName, string name_,        Vector3 placement_pos, Vector3? rot,
        string SnapPiece_, string CenterPiece_, bool    saveData_) {
        this.snapPiece = SnapPiece_;
        this.header = new BlueprintHeader {
            Creator     = playerName,
            CenterPiece = CenterPiece_,
            Name        = name_,
            Rotation    = rot ?? Vector3.zero,
        };
        this.objects  = [];
        this.saveData = saveData_;
        this.m_lines  = [];
    }
    public BlueprintJson(
        string            playerName, string name_, Vector3 placement_pos, Vector3? rot = null,
        string            SnapPiece_ = "", string CenterPiece_ = "", bool saveData_ = true,
        BpjObjectFactory? factory_   = null, BpoRegister? register = null)
        : this( playerName, name_, placement_pos, rot, SnapPiece_,
            CenterPiece_, saveData_ ) {
        if (factory_ == null) {
            (facWrapper, this.factory) = BpjObjectFactory.MakeInstance( this );
        } else {
            factory    = factory_;
            facWrapper = factory.Parent;
        }
        this.bpoRegister = register ?? BpoRegister.GetDefault();

        this.selection   = null;
        this.GameObjects = [];
        if (header.Sorted == false) {
            m_lines.Sort( (a, b) =>
                String.Compare( a.prefab, b.prefab,
                    StringComparison.OrdinalIgnoreCase ) );
        }
    }
    public BlueprintJson(BlueprintHeader blueprintHeader, List<BpjObject?> objects_) {
        this.header           = blueprintHeader;
        snapPiece             = "";
        saveData              = true;
        (facWrapper, factory) = BpjObjectFactory.MakeInstance( this );
        GameObjects           = [];
        selection             = null;
        m_lines               = [];
        this.bpoRegister      = BpoRegister.GetDefault();

        this.objects = objects_;
    }
    public BlueprintJson(
        string            playerName,        SelectionBase selection_,        Vector3 placement_pos,
        string            SnapPiece_ = "",   string        CenterPiece_ = "", bool saveData_ = true,
        BpjObjectFactory? factory_   = null, BpoRegister?  register     = null)
        : this( playerName, selection_.Name, placement_pos, selection_.Rotation,
            SnapPiece_, CenterPiece_, saveData_, factory_, register ) {
        m_lines = [];

        selection   = selection_;
        GameObjects = [];
        if (selection.Objects.Count == selection_.ZVars.Count) {
            for (int i = 0; i < selection_.Objects.Count; i++) {
                var obj = selection_.Objects[i];
                if (obj) {
                    var prefab = Util.GetPrefabName( obj );
                    if ((prefab != "")) {
                        var bpo = new BPListObject( prefab, obj, selection_.ZVars[i] );
                        GameObjects.Add( bpo );
                    }
                }
            }
        } else {
            for (int i = 0; i < selection_.Objects.Count; i++) {
                var obj = selection_.Objects[i];
                if (obj) {
                    var prefab = Util.GetPrefabName( obj );
                    if (prefab != "") {
                        BPListObject bpo = new BPListObject( prefab, obj );
                        GameObjects.Add( bpo );
                    }
                }
            }
        }
        if (GameObjects.Count == 0)
            throw new Exception( "BlueprintJson no objects" );
        GameObjects.Sort( (a, b) =>
            String.Compare( a.prefab, b.prefab,
                StringComparison.OrdinalIgnoreCase ) );
    }
    public static BlueprintJson ReadFromFile(
        string       path,
        bool         saveData_ = true, BpjObjectFactory factory = null,
        BpoRegister? register  = null) {
        var file = new StreamReader( path, false );
        var options = new JsonSerializerOptions {
            WriteIndented = false
        };
        options.Converters.Add( new BpjObjectConverter() );
        options.Converters.Add( new Vec3JsonConverter() );
        options.Converters.Add( new QuatJsonConverter() );
        options.Converters.Add( new ZdoConverter() );
        options.Converters.Add( new BlueprintConverter() );

        var str = file.ReadToEnd();
        //  string pattern = @"(?<!\\)\\n";
        var str2 = Regex.Replace( str, @"(?<!\\)\\n", "" );
        file.Close();
        file.Dispose();
        var bp   = JsonSerializer.Deserialize<BlueprintJson>( str, options );
        return bp;
    }

    public struct BpjLine
    {
        public string   prefab;
        public string   data;
        public string[] ext;
        public BpjLine(string prefab, string data, string[] ext) {
            this.prefab = prefab;
            this.data   = data;
            this.ext    = ext;
        }
    }

    public SortedSet<string> IngoredPieces { get => bpoRegister.IgnoredPrefabs; }
    public virtual string Name {
        // todo maybe reinclude header
        get => header.Name;
        set => header.Name = value.Replace( "\\n", "\n" );
    }
    public virtual string Description {
        get => header.Description;
        set => header.Description = value;
    }
    public virtual string Creator { get => header.Creator; set => header.Creator = value; }
    public virtual Vector3 Coordinates {
        get => header.Coordinates;
        set => header.Coordinates = value;
    }
    public virtual Vector3 Rotation { get => header.Rotation; set => header.Rotation = value; }
    public virtual string CenterPiece {
        get => header.CenterPiece;
        set => header.CenterPiece = value;
    }
    public virtual List<BpjObject?> Objects { get => objects; set => objects = value; }
    public virtual List<Vector3> SnapPoints {
        get => header.SnapPoints;
        set => header.SnapPoints = value;
    }
    public virtual float         Radius { get => header.Radius; set => header.Radius = value; }
    public virtual List<BpjLine> Lines  { get => m_lines;       protected set => m_lines = value; }

    internal Vector3 GetPlacementPos(BlueprintJson blueprint) {
        return blueprint.player.m_placementGhost.transform.position;
    }
    public void BuildFromSelectionSingle() {
        if (factory) {
            Factory.BuildFromSelectionCoroutine();
        } else {
            System.Console.WriteLine( "BuildFromSelection: Factory is null" );
        }
    }

    public void BuildFromSelection() {
        // todo add selection here
        if (factory) {
            Factory.BuildFromSelectionCoroutine();
        } else {
            System.Console.WriteLine( "BuildFromSelection: Factory is null" );
        }
    }
    public void WriteToFile(string path) {
        //  todo move to blueprint
        var file = new StreamWriter( path, append: false );
        //    foreach (var line in this.GetJsonHeader()) { file.WriteLine(line); }
        var options = new JsonSerializerOptions {
            WriteIndented = false
        };
        //     IntPtrCheck.CheckDeepForIntPtr(this.GetType());
        options.Converters.Add( new BpjObjectConverter() );
        options.Converters.Add( new Vec3JsonConverter() );
        options.Converters.Add( new QuatJsonConverter() );
        options.Converters.Add( new ZdoConverter() );
        options.Converters.Add( new BlueprintConverter() );

        try {
            string result = JsonSerializer.Serialize( (BlueprintJson)this, options );
            // when writing a newline with WriteRawValue it treats it as json value and
            // adds a comma at the begin of the line, we just remove it here.
            result = result.Replace( "\n,", "\n" );
            // remove trailing comma
            result = result.Replace( ",\n]", "\n]" );
            file.Write( result );
            
            /*foreach (var bpo in this.Objects) {
                string result=JsonSerializer.Serialize( bpo, options );
                file.Write( result );
            }*/
            // file.WriteLine(this.GetJsonObjects());
            file.Close();
            file.Dispose();
        } catch (Exception e) {
            file.Close();
            file.Dispose();
            System.Console.WriteLine( "Error in Json Serializer " + e );
        }
        
    }
    public void AddExportListener(UnityAction action) { Factory.AddExportListener( action ); }

    public Vector3 Center(string centerPiece) // todo move to factoryloop
    {
        if (centerPiece != "")
            CenterPiece = centerPiece;
        Bounds     bounds = new();
        var        y      = float.MaxValue;
        Quaternion rot    = Quaternion.identity;
        foreach (var obj in Objects) {
            y = Mathf.Min( y, obj.Pos.y );
            bounds.Encapsulate( obj.Pos );
        }

        Vector3 center = new(bounds.center.x, y, bounds.center.z);
        foreach (var obj in Objects) {
            if (obj.Prefab == CenterPiece) {
                center = obj.Pos;
                rot    = Quaternion.Inverse( obj.Rot );
                break;
            }
        }

        Radius = Utils.LengthXZ( bounds.extents );
        foreach (var obj in Objects)
            obj.Pos -= center;
        SnapPoints = SnapPoints.Select( p => p - center ).ToList();
        if (rot != Quaternion.identity) {
            foreach (var obj in Objects) {
                obj.Pos = rot * obj.Pos;
                obj.Rot = rot * obj.Rot;
            }

            SnapPoints = SnapPoints.Select( p => rot * p ).ToList();
        }

        return center;
    }

    public void Add(BpjObject? obj) { this.Objects.Add( obj ); }

    /*
    public string GetJsonObjects() {
        List<string> strings = [];
        foreach (BpjObject? obj in this.Objects) { strings.Add( obj.ToJson() ); }

        return string.Join( ",\n", strings );
    }

    public string[] GetJsonHeader() {
        return [
            $"{{\"Header\":",
            $"{JsonUtility.ToJson( this.header )},",
            $"\"Objects\": [",
        ];
    }
    public string GetJsonSnappoints() {
        /*if (Configuration.SimplerBlueprints)
        {
            return
            [
                $"#Center:{this.CenterPiece}",
                $"#Pieces",
                .. this.Objects.OrderBy(o => o.Prefab)
                    .Select(GetJsonObject),
            ];
        }#1#

        var strings = string.Join( ",\n",
            this.SnapPoints.Select( point => JsonUtility.ToJson( point ) ) );

        return $"],\n\"SnapPoints\": [{strings}";
    }
    public string GetJsonFooter() { return $"]}}"; }*/
}
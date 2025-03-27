using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.Json;
using System.Text.Json.Serialization;
using Argo.blueprint.Util;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Argo.Blueprint;

[Serializable]
public struct BlueprintHeader()
{
    public string        Name       ="";
    public string        Creator    ="";
    public string        Description="";
    public string        CenterPiece="";
    public Vector3       Coordinates=default;
    public Vector3       Rotation   =default;
    public string        Category   ="InfinityHammer.Json";
    public float         Radius     =0f;
    public string        Version    ="0.1.0";
    public string        ModName    ="InfinityHammer";
    public string        ModVersion ="0.1.0";
    public bool          Sorted     =true;
    public List<Vector3> SnapPoints = [];
    public BlueprintHeader(string centerPiece) : this() { }
};

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
    [JsonIgnore]  public Player             player;
    [JsonIgnore]  public string             snapPiece;
    [JsonIgnore]  public bool               saveData;
    [JsonInclude] public BlueprintHeader    header       =new BlueprintHeader();
    [JsonInclude] public List<BpjObject?>   objects      = [];
    [JsonIgnore]  public HashSet<string>    ingoredPieces= [];
    [JsonIgnore]  public BpjObjectFactory   factory;
    [JsonIgnore]  public List<BPListObject> GameObjects= [];
    [JsonIgnore]  public SelectionBase?     selection;
    [JsonIgnore]  public List<BpjLine>      m_lines;

    public BpjObjectFactory Factory { get => factory; internal set => factory=value; }
    /// <summary>
    /// since the Factory is a MonoBehaviour it needs an game object attached to
    /// so unity can keep its coroutines running
    /// </summary>
    [JsonIgnore]
    public GameObject? facWrapper=null;
    [JsonIgnore] internal readonly BpoRegister bpoRegister;
    public BlueprintJson(
        Player       player_, string name_, string[] lines_,
        BpoRegister? register=null) {
        player                             =player_;
        snapPiece                          ="";
        saveData                           =true;
        var (importedHeader, importedLines)=ReadLines( lines_ );
        header                             =importedHeader ?? new BlueprintHeader();
        bpoRegister                        =register       ?? BpoRegister.GetDefault();
        var (FacWrapper, fac_)             =BpjObjectFactory.MakeInstance( this );
        factory                            =fac_;
        facWrapper                         =FacWrapper;
        header.Name                        =name_;
        selection                          =null;
        GameObjects                        =[];
        m_lines                            =importedLines;
        if (header.Sorted == false) {
            m_lines.Sort( (a, b) =>
                String.Compare( a.prefab, b.prefab,
                    StringComparison.OrdinalIgnoreCase ) );
        }
    }
    public BlueprintJson(
        Player       player_, SelectionBase selection_,
        string       CenterPiece_,
        string       SnapPiece_, bool saveData_=true,
        BpoRegister? register=null) {
        player                =player_;
        snapPiece             =SnapPiece_;
        saveData              =saveData_;
        m_lines               =[];
        header.CenterPiece    =CenterPiece_;
        bpoRegister           =register ?? BpoRegister.GetDefault();
        var (FacWrapper, fac_)=BpjObjectFactory.MakeInstance( this );
        factory               =fac_;
        facWrapper            =FacWrapper;
        header.Name           =selection_.Name;
        header.Creator        =player.GetPlayerName();
        header.Rotation       =selection_.Rotation;

        selection  =selection_;
        GameObjects=[];
        if (selection.Objects.Count == selection_.ZVars.Count) {
            for (int i=0; i < selection_.Objects.Count; i++) {
                var obj=selection_.Objects[i];
                if (obj) {
                    var prefab=Util.GetPrefabName( obj );
                    if ((prefab != "")) {
                        var bpo=new BPListObject( prefab, obj, selection_.ZVars[i] );
                        GameObjects.Add( bpo );
                    }
                }
            }
        } else {
            for (int i=0; i < selection_.Objects.Count; i++) {
                var obj=selection_.Objects[i];
                if (obj) {
                    var prefab=Util.GetPrefabName( obj );
                    if (prefab != "") {
                        BPListObject bpo=new BPListObject( prefab, obj );
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

    public struct BpjLine
    {
        public string   prefab;
        public string   data;
        public string[] ext;
        public BpjLine(string prefab, string data, string[] ext) {
            this.prefab=prefab;
            this.data  =data;
            this.ext   =ext;
        }
    }

    public HashSet<string> IngoredPieces {
        get => ingoredPieces;
        set => ingoredPieces=
            value ?? throw new ArgumentNullException( nameof(value) );
    }
    public virtual string Name {
        get => header.Name;
        set => header.Name=value.Replace( "\\n", "\n" );
    }
    public virtual string Description {
        get => header.Description;
        set => header.Description=value;
    }
    public virtual string Creator { get => header.Creator; set => header.Creator=value; }
    public virtual Vector3 Coordinates {
        get => header.Coordinates;
        set => header.Coordinates=value;
    }
    public virtual Vector3 Rotation { get => header.Rotation; set => header.Rotation=value; }
    public virtual string CenterPiece {
        get => header.CenterPiece;
        set => header.CenterPiece=value;
    }
    public virtual List<BpjObject?> Objects { get => objects; set => objects=value; }
    public virtual List<Vector3> SnapPoints {
        get => header.SnapPoints;
        set => header.SnapPoints=value;
    }
    public virtual float         Radius { get => header.Radius; set => header.Radius=value; }
    public virtual List<BpjLine> Lines { get => m_lines; protected set => m_lines=value; }
    public         void          BuildBluePrintSingle() { Factory.BuildBluePrintCoroutine(); }

    public void BuildBluePrintCoroutine() { Factory.BuildBluePrintCoroutine(); }
    public void WriteToFile(string path) {
        //  todo move to blueprint
        var file=new StreamWriter( path, append: false );
        //    foreach (var line in this.GetJsonHeader()) { file.WriteLine(line); }
        var options=new JsonSerializerOptions {
            WriteIndented=false
        };
        //     IntPtrCheck.CheckDeepForIntPtr(this.GetType());
        options.Converters.Add( new BpjObjectConverter() );
        options.Converters.Add( new Vec3JsonConverter() );
        options.Converters.Add( new QuatJsonConverter() );
        options.Converters.Add( new ZdoConverter() );

        try {
            foreach (var bpo in this.Objects) {
                string result=JsonSerializer.Serialize( bpo, options );
                file.Write( result );
            }
            // file.WriteLine(this.GetJsonObjects());
        } catch (Exception e) {
            System.Console.WriteLine( "Error in Json Serializer " + e );
        }
        file.Close();
        file.Dispose();
    }
    public void AddExportListener(UnityAction action) { Factory.AddExportListener( action ); }

    private static (BlueprintHeader? header, List<BpjLine> lines) ReadLines(
        string[] rows) {
        var              piece         =false;
        List<BpjLine>    importedLines = [];
        BlueprintHeader? importedHeader=null;
        for (int i=0; i < rows.Length; i++) {
            if (rows[i].StartsWith( "]",
                    StringComparison.OrdinalIgnoreCase ) &&
                piece) { piece=false; } else if (piece) {
                var str=rows[i].TrimEnd( ',' ); // remove comma at end if any
                try { importedLines.Add( BpjObject.Split( rows[i] ) ); } catch (Exception e) {
                    System.Console.WriteLine( e );
                    System.Console.WriteLine( "error while parsing: piece: " +
                        str                                                  + "" );
                }
            } else if (rows[i].StartsWith( "{\"Header\":",
                           StringComparison.OrdinalIgnoreCase )) {
                try {
                    i++;
                    importedHeader=
                        JsonUtility.FromJson<BlueprintHeader>(
                            rows[i].TrimEnd( ',' ) );
                } catch (Exception e) {
                    System.Console.WriteLine( e );
                    System.Console.WriteLine( "error while parsing: header: " +
                        rows[i]                                               + "" );
                }
            } else if (rows[i].StartsWith( "\"Objects\":",
                           StringComparison.OrdinalIgnoreCase )) { piece=true; }
        }

        return (importedHeader, importedLines);
    }
    public Vector3 Center(string centerPiece) // todo move to factoryloop
    {
        if (centerPiece != "")
            CenterPiece=centerPiece;
        Bounds     bounds=new();
        var        y     =float.MaxValue;
        Quaternion rot   =Quaternion.identity;
        foreach (var obj in Objects) {
            y=Mathf.Min( y, obj.Pos.y );
            bounds.Encapsulate( obj.Pos );
        }

        Vector3 center=new(bounds.center.x, y, bounds.center.z);
        foreach (var obj in Objects) {
            if (obj.Prefab == CenterPiece) {
                center=obj.Pos;
                rot   =Quaternion.Inverse( obj.Rot );
                break;
            }
        }

        Radius=Utils.LengthXZ( bounds.extents );
        foreach (var obj in Objects)
            obj.Pos-=center;
        SnapPoints=SnapPoints.Select( p => p - center ).ToList();
        if (rot != Quaternion.identity) {
            foreach (var obj in Objects) {
                obj.Pos=rot * obj.Pos;
                obj.Rot=rot * obj.Rot;
            }

            SnapPoints=SnapPoints.Select( p => rot * p ).ToList();
        }

        return center;
    }

    public void Add(BpjObject? obj) { this.Objects.Add( obj ); }

    public string GetJsonObjects() {
        List<string> strings= [];
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
        }*/

        var strings=string.Join( ",\n",
            this.SnapPoints.Select( point => JsonUtility.ToJson( point ) ) );

        return $"],\n\"SnapPoints\": [{strings}";
    }
    public string GetJsonFooter() { return $"]}}"; }
}
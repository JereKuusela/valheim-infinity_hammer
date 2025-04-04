using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Argo.Blueprint.Json;
using Argo.Blueprint.Util;
using Argo.DataAnalysis;
using Argo.Zdo;
using UnityEngine;

namespace Argo.Blueprint;

using static BpjZVars;

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

public abstract class BpoComponent { }


public class RequiredModsComponent : BpoComponent
{
    public List<ModIdentifier> mods = new();
    // featurs might be scaling of pieces etc
    public List<string> features = new();
}


public class BpoReferenceComponent : BpoComponent
{
    // todo split
    // primary means this is the object like a blueprint every other object is refering to
    // its for example used for nested blueprints. if its primary it has a parents component
    // since it can have multiple parents (everything thats refering it)
    // if its not a primary it has at least one child which should be the primary
    // todo references should have a possibilty to include or exclude certain parts of the blueprint
    // todo, references should get a unique name 
    public bool                           isPrimary      = true;
    public bool                           isExternal     = false;
    public bool                           isCyclic       = false; // todo test on stack
    // if this object ir refering other objects they are stored here
    // an object can have multiple targets if it has variants
    public List<WeakReference<BpjObject>> Targets = new(); 
    // if this object is referred by other objects they are stored here
    // an object may have Targets and Refferers at the same time for example if an frequently
    // refered blueprint or prefab has multiple variants, its just easier to leave it up to
    // that target to manage the variants
    public List<WeakReference<BpjObject>> Refferers = new();  
    // todo add variants here or in different kind of structure?
    }
public class BpoVariantComponent : BpoComponent
{
    // todo remove? should be possible to easiely be handled by the Reference component?
    //      on the other side it might be usefull for tags
    public List<string> varianttags = new();
}
public class BpoChildrenComponent : BpoComponent
{
    public List<BpjObject> children = new();
}

// mainly used for reference components
public class BpoParentsComponent : BpoComponent
{
    public List<string>                   references     = new();
    public List<WeakReference<BpjObject>> obj_references = new();
}
public abstract class BpoComponentDic : BpoComponent
{
    public AExtraData m_values;
    public AExtraData Values { get => m_values; set => m_values = value; }

    public void Add<T>(int hash, T value, HashRegister info) {
        bool unknown = !info.GetNameOrToString(hash, out var name);
        this.m_values.TryAdd(name, value, unknown);
    }
}





[Serializable]
public class BpjObject : IBlueprintObject
{
    [JsonIgnore] // the prefab will saved as key
    public string m_prefab = "";
    [JsonPropertyName("Data")]    public TData                  m_baseData;
    [JsonPropertyName("AddData")] public List<BpoComponent> m_pieceData;

    [JsonPropertyName("ZDOVars")] public AExtraData m_extraData;

    public struct TData()
    {
        [JsonInclude] [JsonPropertyName("flags")] public BPOFlags   flags;
        [JsonInclude] [JsonPropertyName("p")]     public Vector3    pos;
        [JsonInclude] [JsonPropertyName("r")]     public Quaternion rot;
        [JsonInclude] [JsonPropertyName("s")]     public Vector3    scale;
        [JsonInclude] [JsonPropertyName("odds")]  public float      chance;

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

    [JsonIgnore] public AExtraData ZVars { get => m_extraData; set => m_extraData = value; }
    internal BpjObject() {
        m_prefab = "";
        m_baseData = new TData(0, Vector3.zero, Quaternion.identity, Vector3.one,
            0f);
    }
    public BpjObject(string prefab, TData baseData, AExtraData? properties = null) {
        this.m_prefab    = prefab;
        this.m_baseData  = baseData;
        this.m_extraData = properties ?? (AExtraData)new ExtraDataArgo();
    }
    public BpjObject(
        string  prefab, Vector3 pos, Quaternion rot,
        Vector3 scale,
        float   chance) {
        this.m_prefab    = prefab;
        this.m_baseData  = new TData(0, pos, rot, scale, chance);
        this.m_extraData = new ExtraDataArgo();
    }

    public BpjObject(
        string  prefab, Vector3 pos, Quaternion rot,
        Vector3 scale,
        string  info,
        string  data, float chance) {
        m_prefab        = prefab;
        this.m_baseData = new TData(0, pos, rot, scale, chance);
        m_extraData     = new ExtraDataArgo();
    }
    public BpjObject(
        string   mPrefab,   GameObject? obj, SaveExtraData saveData,
        BPOFlags flags = 0, AExtraData? vars = null) {
        this.m_prefab = mPrefab;
        this.m_baseData = new TData(flags,
            obj.transform.localPosition, obj.transform.localRotation,
            obj.transform.localScale, 1f);
        this.m_extraData = vars ?? new ExtraDataArgo();
    }

    public void Add(BpoComponent component) { this.m_pieceData.Add(component); }

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

        return JsonSerializer.Serialize(this, options);
    }

    [JsonIgnore] public virtual string Prefab { get => m_prefab; set => m_prefab = value; }
    [JsonIgnore]
    public virtual Vector3 Pos { get => m_baseData.pos; set => m_baseData.pos = value; }
    [JsonIgnore]
    public virtual Quaternion Rot { get => m_baseData.rot; set => m_baseData.rot = value; }
// todo write conversion function for other blueprint format    
    [JsonIgnore] public virtual string Data { get => ""; set { } }
    [JsonIgnore]
    public virtual Vector3 Scale { get => m_baseData.scale; set => m_baseData.scale = value; }
// todo write conversion function for other blueprint format    
    [JsonIgnore]
    public virtual float Chance { get => m_baseData.chance;   set => m_baseData.chance = value; }
    [JsonIgnore] public virtual string ExtraInfo { get => ""; set { } }
    [JsonIgnore]
    public virtual BPOFlags Flags { get => m_baseData.flags; set => m_baseData.flags = value; }
}
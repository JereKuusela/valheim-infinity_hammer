using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
//using System.Text.Json;
using UnityEngine;
using Data;

namespace InfinityHammer;
enum BlueprintObjectFlags
{
    Interactable = 1 << 31,
    Hoverable = 1 << 30,
    HoverMenu = 1 << 29,
    HoverMenuExtened = 1 << 28,
    TextReceiver = 1 << 27,
    Destructible = 1 << 26,
    PieceMarker = 1 << 25,
    WarterInteractable = 1 << 24,
    IDooDadControler = 1 << 23,
    Projectile = 1 << 22,
    
    
    
    Misc = 0,
    Crafting = 1,
    BuildingWorkbench = 2,
    BuildingStonecutter = 3,
    Furniture = 4,
    Feasts = 5,
    Food = 6,
    Meads = 7,
}

[Serializable]
public class BlueprintObjectJson(
    string name,
    Vector3 pos,
    Quaternion rot,
    Vector3 scale,
    string info,
    string data,
    float chance) : IBlueprintObject
{
    public int flags = 0;
    public string prefab = name;
    public Vector3 pos = pos;
    public Quaternion rot = rot.normalized;
    public string data = data;
    public Vector3 scale = scale;
    public float chance = chance;
    public string extraInfo = info;

    public virtual string Prefab
    {
        get => prefab;
        set => prefab = value ?? throw new ArgumentNullException(nameof(value));
    }

    public virtual Vector3 Pos
    {
        get => pos;
        set => pos = value;
    }

    public virtual Quaternion Rot
    {
        get => rot;
        set => rot = value;
    }

    public virtual string Data
    {
        get => data;
        set => data = value ?? throw new ArgumentNullException(nameof(value));
    }

    public virtual Vector3 Scale
    {
        get => scale;
        set => scale = value;
    }

    public virtual float Chance
    {
        get => chance;
        set => chance = value;
    }

    public virtual string ExtraInfo
    {
        get => extraInfo;
        set => extraInfo =
            value ?? throw new ArgumentNullException(nameof(value));
    }
}
[Serializable]
public class BlueprintHeader
{
    public string Name = "";
    public string Creator = "";
    public string Description = "";
    public string CenterPiece = "";
    public Vector3 Coordinates;
    public Vector3 Rotation;
    public List<Vector3> SnapPoints = [];
    public string Category = "InfinityHammer";
    public float Radius = 0f;
};
[Serializable]
public class BlueprintJson : IBlueprint
{
    public BlueprintHeader header;
    public List<BlueprintObjectJson> Objects = [];

   public BlueprintJson()
    {
        header = new BlueprintHeader();
    }

    public virtual string Name
    {
        get => header.Name;
        set => header.Name =
            value ?? throw new ArgumentNullException(nameof(value));
    }

    public virtual string Description
    {
        get => header.Description;
        set => header.Description =
            value ?? throw new ArgumentNullException(nameof(value));
    }

    public virtual string Creator
    {
        get => header.Creator;
        set => header.Creator =
            value ?? throw new ArgumentNullException(nameof(value));
    }

    public virtual Vector3 Coordinates
    {
        get => header.Coordinates;
        set => header.Coordinates = value;
    }

    public virtual Vector3 Rotation
    {
        get => header.Rotation;
        set => header.Rotation = value;
    }

    public virtual string CenterPiece
    {
        get => header.CenterPiece;
        set => header.CenterPiece =
            value ?? throw new ArgumentNullException(nameof(value));
    }

    /*public virtual List<IBlueprintObject> Objects
    {
        get => objects;
        set => objects =
            value ?? throw new ArgumentNullException(nameof(value));
    }*/

    public virtual List<Vector3> SnapPoints
    {
        get => header.SnapPoints;
        set => header.SnapPoints =
            value ?? throw new ArgumentNullException(nameof(value));
    }

    public virtual float Radius
    {
        get => header.Radius;
        set =>  header.Radius = value;
    }


  
    public Vector3 Center(string centerPiece)
    {
        if (centerPiece != "")
            CenterPiece = centerPiece;
        Bounds bounds = new();
        var y = float.MaxValue;
        Quaternion rot = Quaternion.identity;
        foreach (var obj in Objects)
        {
            y = Mathf.Min(y, obj.Pos.y);
            bounds.Encapsulate(obj.Pos);
        }

        Vector3 center = new(bounds.center.x, y, bounds.center.z);
        foreach (var obj in Objects)
        {
            if (obj.Prefab == CenterPiece)
            {
                center = obj.Pos;
                rot = Quaternion.Inverse(obj.Rot);
                break;
            }
        }

        Radius = Utils.LengthXZ(bounds.extents);
        foreach (var obj in Objects)
            obj.Pos -= center;
        SnapPoints = SnapPoints.Select(p => p - center).ToList();
        if (rot != Quaternion.identity)
        {
            foreach (var obj in Objects)
            {
                obj.Pos = rot * obj.Pos;
                obj.Rot = rot * obj.Rot;
            }

            SnapPoints = SnapPoints.Select(p => rot * p).ToList();
        }

        return center;
    }

    private static string GetExtraInfo(GameObject obj, DataEntry data)
    {
        Dictionary<string, string> pars = [];
        var info = "";
        if (data.TryGetString(pars, ZDOVars.s_text, out var text))
            info = text;
        if (data.TryGetString(pars, ZDOVars.s_tamedName, out var name))
            info = name;
        if (data.TryGetString(pars, ZDOVars.s_tag, out var tag))
            info = tag;
        if (data.TryGetString(pars, ZDOVars.s_item, out var item))
        {
            var variant = data.TryGetInt(pars, ZDOVars.s_variant, out var v)
                ? v
                : 0;
            if (variant != 0)
                info = $"{item}:{variant}";
            else
                info = $"{item}";
        }

        if (obj.TryGetComponent<ArmorStand>(out var armorStand))
        {
            info = $"{armorStand.m_pose}:";
            info += $"{armorStand.m_slots.Count}:";
            var slots = armorStand.m_slots.Select(slot =>
                $"{slot.m_visualName}:{slot.m_visualVariant}");
            info += string.Join(":", slots);
        }

        return info;
    }

    public void AddSingleObject(
        Dictionary<string, string> pars, GameObject obj, bool saveData)
    {
        var name = Utils.GetPrefabName(obj);
        var save = saveData ||
                   Configuration.SavedObjectData.Contains(
                       name.ToLowerInvariant());
        var data = save ? Selection.Get().GetData() : null;
        var info = data == null ? "" : GetExtraInfo(obj, data);
        this.Objects.Add(new BlueprintObjectJson(name, Vector3.zero,
            Quaternion.identity, obj.transform.localScale, info,
            data?.GetBase64(pars) ?? "", 1f));
    }

    public void AddObject(
        Dictionary<string, string> pars, GameObject obj, bool saveData,
        int index = 0)
    {
        var name = Utils.GetPrefabName(obj);
        var save = saveData ||
                   Configuration.SavedObjectData.Contains(
                       name.ToLowerInvariant());
        var data = save ? Selection.Get().GetData(index) : null;
        var info = data == null ? "" : GetExtraInfo(obj, data);
        this.Objects.Add(new BlueprintObjectJson(name,
            obj.transform.localPosition, obj.transform.localRotation,
            obj.transform.localScale, info, data?.GetBase64(pars) ?? "", 1f));
    }

    private static string GetJsonObject(IBlueprintObject obj)
    {
        if (obj is BlueprintObjectJson derived)
        {
            return GetJsonObject(derived);
        }
        else
        {
            return "";
            // throw new InvalidCastException                ($"Downcasting von Typ {obj.GetType()} zu {typeof(BlueprintObjectJson)} fehlgeschlagen.");
        }
    }

    private static string GetJsonObject(BlueprintObjectJson obj)
    {
        try
        {
            //return JsonSerializer.Serialize(obj);
            return JsonUtility.ToJson(obj) + ",";
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e);
            return "";
        }
    }

    private static string GetPlanBuildSnapPoint(Vector3 pos)
    {
        var x = HammerHelper.Format(pos.x);
        var y = HammerHelper.Format(pos.y);
        var z = HammerHelper.Format(pos.z);
        return $"{x};{y};{z}";
    }

    public string[] GetPlanBuildFile()
    {
        if (Configuration.SimplerBlueprints)
            return
            [
                $"#Center:{this.CenterPiece}",
                $"#Pieces",
                .. this.Objects.OrderBy(o => o.Prefab)
                    .Select(GetJsonObject),
            ];
        return
        [
            $"{{\"Header\":",
            $"{JsonUtility.ToJson(this.header)},",
            $"\"Objects\": [", 
            string.Join(",\n", this.Objects.Select(JsonUtility.ToJson)),
            $"]}}"
        ];
    }
}
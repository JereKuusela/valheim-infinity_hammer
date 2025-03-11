using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text.Json;
using UnityEngine;
using Data;

namespace InfinityHammer;

public class BlueprintObjectJson : BlueprintObject
{
    public BlueprintObjectJson(string name, Vector3 pos, Quaternion rot,
        Vector3 scale,
        string info, string data, float chance) : base(name, pos, rot, scale,
        info, data, chance) { }
}

public class BlueprintJson : Blueprint
{
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
    private static string GetPlanBuildObject(BlueprintObject obj)
    {
        if (obj is BlueprintObjectJson derived)
        {
            return GetPlanBuildObject(derived);
        }
        else
        {
            throw new InvalidCastException
                ($"Downcasting von Typ {obj.GetType()} zu {typeof(BlueprintObjectJson)} fehlgeschlagen.");
        }
    }

    private static string GetPlanBuildObject(BlueprintObjectJson obj)
    {
        var name = obj.Prefab;
        var posX = HammerHelper.Format(obj.Pos.x);
        var posY = HammerHelper.Format(obj.Pos.y);
        var posZ = HammerHelper.Format(obj.Pos.z);
        var rotX = HammerHelper.Format(obj.Rot.x);
        var rotY = HammerHelper.Format(obj.Rot.y);
        var rotZ = HammerHelper.Format(obj.Rot.z);
        var rotW = HammerHelper.Format(obj.Rot.w);
        var scaleX = HammerHelper.Format(obj.Scale.x);
        var scaleY = HammerHelper.Format(obj.Scale.y);
        var scaleZ = HammerHelper.Format(obj.Scale.z);
        var info = obj.ExtraInfo;
        var data = obj.Data;
        return
            $"{name};;{posX};{posY};{posZ};{rotX};{rotY};{rotZ};{rotW};{info};{scaleX};{scaleY};{scaleZ};{data}";
    }
    private static string GetJsonObject(BlueprintObject obj)
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
            return "";
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
                    .Select(GetPlanBuildObject),
            ];
        return
        [
            $"#Name:{this.Name}",
            $"#Creator:{this.Creator}",
            $"#Description:{this.Description}",
            $"#Category:InfinityHammer",
            $"#Center:{this.CenterPiece}",
            $"#Coordinates:{HammerHelper.PrintXZY(this.Coordinates)}",
            $"#Rotation:{HammerHelper.PrintYXZ(this.Rotation)}",
            $"#SnapPoints",
            .. this.SnapPoints.Select(GetPlanBuildSnapPoint),
            $"#Pieces",
            .. this.Objects.OrderBy(o => o.Prefab).Select(GetPlanBuildObject),
        ];
    }
}
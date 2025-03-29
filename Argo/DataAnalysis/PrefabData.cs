using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Argo.Blueprint;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Argo.DataAnalysis;

using static ComponentData;

[Serializable]
public class PrefabData
{
    static PrefabData() { }

    [Serializable]
    public enum ZDOVarType
    {
        TUnknown    = 0,
        Tfloat      = 1,
        TVector3    = 2,
        TQuaternion = 3,
        Tint        = 4,
        Tlong       = 5,
        Tstring     = 6,
        TbyteArray  = 7,
    }

    public Dictionary<int, ComponentData.ZdoVarFlag> AllKnownZdoVarTypes
        = Data.ZdoVarTypes_default;
    public Dictionary<ZdoVarFlag, SortedSet<int>> AllKnownZdoVarHashs
        = Data.ZdoVarHashs_default;
    public Dictionary<string, Data> PrefabDataZDO = [];
    public Dictionary<string, List<string>> PrefabDataNoZDO = [];
    public ComponentData m_ComponentData = new ComponentData();

    [Serializable]
    public struct ZdoPair
    {
        [JsonInclude] public string name;
        [JsonInclude] public int    hash;
    }

    [Serializable]
    public struct ZdoVar
    {
        [JsonInclude] public List<ZdoPair> floats      = [];
        [JsonInclude] public List<ZdoPair> Vector3s    = [];
        [JsonInclude] public List<ZdoPair> Quaternions = [];
        [JsonInclude] public List<ZdoPair> ints        = [];
        [JsonInclude] public List<ZdoPair> longs       = [];
        [JsonInclude] public List<ZdoPair> strings     = [];
        [JsonInclude] public List<ZdoPair> byteArrays  = [];
        public ZdoVar() { }
    }

    [Serializable]
    public struct Data
    {
        private static Data? instance;
        public string Prefab;
        public List<string> Components;
        public Dictionary<int, ComponentData.ZdoVarFlag> KnownZdoVarTypes;
        public Dictionary<ZdoVarFlag, SortedSet<int>> KnownZdoVarHashs;
        public static readonly Dictionary<int, ComponentData.ZdoVarFlag>
            ZdoVarTypes_default;
        public static readonly Dictionary<ZdoVarFlag, SortedSet<int>>
            ZdoVarHashs_default;
        static Data() {
            ZdoVarTypes_default
                = ZDOInfo.Instance.Hashes.ToDictionary(key => key,
                    key => ComponentData.ZdoVarFlag.TUnknown);
            ZdoVarHashs_default = ZdoVarFlag.GetValues(typeof(ZdoVarFlag))
                                            .Cast<ZdoVarFlag>()
                                            .ToDictionary(key => key,
                                                 key => new SortedSet<int>());
            ;
        }
        Data Instance() {
            if (instance == null)
            {
                instance = new();
            }
            return (Data)instance;
        }
        public Data() {
            Prefab           = "";
            Components       = new List<string>();
            KnownZdoVarTypes = ZdoVarTypes_default;
            KnownZdoVarHashs = ZdoVarHashs_default;
        }
        public string ToJson() {
            try
            {
                List<string> component_list = [];
                foreach (var str1 in Components)
                {
                    component_list.Add($"\"{str1}\"");
                }
                string components
                    = $"\"Components\":[{string.Join(",", component_list)}]";
                List<string> zdo_str_list = [];
                foreach (var pair in KnownZdoVarTypes)
                {
                    if (pair.Value == ZdoVarFlag.TUnknown) continue;
                    int hash = pair.Key;

                    var name = ZDOInfo.GetName(hash);

                    /*string result
                        = $"{{\"{name}\":{JsonUtility.ToJson(pair.Value)}}}";*/
                    string result
                        = $"{{\"{name}\":{JsonSerializer.Serialize(pair.Value)}}}";
                    zdo_str_list.Add(result);
                }
                string zdo_vars
                    = $"\"ZDOVars\":[{String.Join(",", zdo_str_list)}]";
                string str = $"{{{components},{zdo_vars}}}";
                return str;
            } catch (Exception e)
            {
                System.Console.WriteLine("data ToJson" + e);
            }
            return "";
        }
    }

    public ZdoVar GetZdoVars(ZDO zdo) {
        ZdoVar vars = new();
        try
        {
            var id = zdo.m_uid;

            try
            {
                vars.floats = ZDOExtraData.s_floats[zdo.m_uid] == null
                    ? GetZdoVars(ZDOExtraData.s_floats[zdo.m_uid],
                        ZdoVarFlag.Tfloat)
                    : new List<ZdoPair>();
            } catch (Exception e)
            {
                System.Console.WriteLine("GetZdoVars1.2" + e);
                vars.floats = new List<ZdoPair>();
            }
            try
            {
                vars.Vector3s = GetZdoVars(
                    ZDOExtraData.s_vec3[zdo.m_uid] ?? null,
                    ZdoVarFlag.TVector3);
            } catch (Exception e)
            {
                System.Console.WriteLine("GetZdoVars1.3" + e);
                vars.Vector3s = new List<ZdoPair>();
            }
            try
            {
                vars.Quaternions = GetZdoVars(
                    ZDOExtraData.s_quats[zdo.m_uid] ?? null,
                    ZdoVarFlag.TQuaternion);
            } catch (Exception e)
            {
                System.Console.WriteLine("GetZdoVars11.4" + e);
                vars.Quaternions = new List<ZdoPair>();
            }
            try
            {
                vars.ints = GetZdoVars(ZDOExtraData.s_ints[zdo.m_uid] ?? null,
                    ZdoVarFlag.Tint);
            } catch (Exception e)
            {
                System.Console.WriteLine("GetZdoVars1.5" + e);
                vars.ints = new List<ZdoPair>();
            }
            try
            {
                vars.longs = GetZdoVars(ZDOExtraData.s_longs[zdo.m_uid] ?? null,
                    ZdoVarFlag.Tlong);
            } catch (Exception e)
            {
                System.Console.WriteLine("GetZdoVars1.6" + e);
                vars.longs = new List<ZdoPair>();
            }
            try
            {
                vars.strings = GetZdoVars(
                    ZDOExtraData.s_floats[zdo.m_uid] ?? null,
                    ZdoVarFlag.Tstring);
            } catch (Exception e)
            {
                System.Console.WriteLine("GetZdoVars1.7" + e);
                vars.strings = new List<ZdoPair>();
            }
            try
            {
                vars.byteArrays = GetZdoVars(
                    ZDOExtraData.s_byteArrays[zdo.m_uid],
                    ZdoVarFlag.TbyteArray);
            } catch (Exception e)
            {
                System.Console.WriteLine("GetZdoVars1.8" + e);
                vars.byteArrays = new List<ZdoPair>();
            }
        } catch (Exception)
        {
            System.Console.WriteLine("GetZdoVars1.1 ZDO not valid \n");
        }
        return new ZdoVar();
        // return vars;
    }

    public List<ZdoPair> GetZdoVars<T>(BinarySearchDictionary<int, T>? dic,
                                       ComponentData.ZdoVarFlag        type) {
        List<ZdoPair> list = [];
        try
        {
            if (dic == null) return list;
            foreach (var kv in dic)
            {
                list.Add(
                    new ZdoPair
                        { name = ZDOInfo.GetName(kv.Key), hash = kv.Key });
            }
        } catch (Exception e)
        {
            System.Console.WriteLine("GetZdoVars2" + e);
        }
        return list;
    }

    public List<ZdoPair> GetZdoVarsStr<T>(BinarySearchDictionary<int, T>? dic,
                                          Data data,
                                          ComponentData.ZdoVarFlag type) {
        List<ZdoPair> list = [];
        try
        {
            if (dic == null) return list;
            foreach (var key in dic.Keys)
            {
                if (AllKnownZdoVarTypes.ContainsKey(key))
                {
                    AllKnownZdoVarTypes[key] |= type;
                }
                if (data.KnownZdoVarTypes.ContainsKey(key))
                {
                    data.KnownZdoVarTypes[key] |= type;
                }

                data.KnownZdoVarHashs[type].Add(key);
                AllKnownZdoVarHashs[type].Add(key);
            }
        } catch (Exception e)
        {
            System.Console.WriteLine("GetZdoVars2" + e);
        }
        return list;
    }

    private static ZdoVarFlag
        GetZdoType<T>(BinarySearchDictionary<int, T> zdos, Data data) {
        try
        {
            switch (typeof(T))
            {
                case { } t when t == typeof(float):
                    return ZdoVarFlag.Tfloat;
                case { } t when t == typeof(Vector3):
                    return ZdoVarFlag.TVector3;
                case { } t when t == typeof(Quaternion):
                    return ZdoVarFlag.TQuaternion;
                case { } t when t == typeof(int):
                    return ZdoVarFlag.Tint;
                case { } t when t == typeof(long):
                    return ZdoVarFlag.Tlong;
                case { } t when t == typeof(byte[]):
                    return ZdoVarFlag.TbyteArray;
                case { } t when t == typeof(string):
                    return ZdoVarFlag.Tstring;
                default:
                    return ZdoVarFlag.TUnknown;
            }
        } catch (Exception e)
        {
            System.Console.WriteLine("GetZdoType" + e);
        }
        return ZdoVarFlag.TUnknown;
    }
    public void VarsToJson(List<string> lines) {
        try
        {
            List<string> known_zdo_types = [];
            List<string> known_zdo_hashs = [];
            lines.Add("{\"KnownZDOVars\": [");
            // todo clear this mess up and use some lamdas or so instead of the loops
            foreach (var pair in AllKnownZdoVarTypes)
            {
                if (pair.Value == ZdoVarFlag.TUnknown) continue;

                lines.Add($"{{\"{ZDOInfo.GetName(pair.Key)}\":");
                lines.Add($"{pair.Value}");
                lines.Add($"}},");
            }
            lines.Add($"}},");
            lines.Add("{\"KnownZDOVarsByType\": [");
            foreach (var pair in AllKnownZdoVarHashs)
            {
                if (pair.Value.Count == 0) continue;
                List<string> hashs = [];
                foreach (var type in pair.Value)
                {
                    hashs.Add($"\"{ZDOInfo.GetName(type)}\"");
                }
                lines.Add($"{{\"{pair.Key.ToString()}\":");
                lines.Add($"[\"{string.Join(",", hashs)}\"]");
                lines.Add($"}},");
            }
            lines.Add($"}}");
        } catch (Exception e)
        {
            System.Console.WriteLine("VarsToJson" + e);
        }
    }

    public List<string> ToJson() {
        try
        {
            List<string> lines = [];
            lines.Add("{\"WithZdo\": ");
            // todo clear this mess up and use some lamdas or so instead of the loops
            foreach (var pair in PrefabDataZDO)
            {
                lines.Add($"{{{pair.Key}:{pair.Value.ToJson()}}},");
            }
            lines[lines.Count - 1]
                = lines[lines.Count - 1]
                   .TrimEnd(','); // remove last comma [0..^1]
            lines.Add(" }");
            lines.Add("{\"NoZdo\": ");
            foreach (var pair in PrefabDataNoZDO)
            {
                List<string> list = [];
                lines.Add($"{{\"{pair.Key}\":");
                foreach (var str in pair.Value)
                {
                    list.Add($"\"{str}\"");
                }
                list[list.Count - 1]
                    = list[list.Count - 1]
                       .TrimEnd(','); // remove last comma [0..^1]
                lines.Add(
                    $"{{\"{pair.Key}\":[{String.Join(",", list)}]}},");
            }
            lines[lines.Count - 1]
                = lines[lines.Count - 1]
                   .TrimEnd(','); // remove last comma [0..^1]
            VarsToJson(lines);
            lines.Add(" }");
            return lines;
        } catch (Exception e)
        {
            System.Console.WriteLine("ToJson" + e);
        }
        return new List<string>();
    }
    /*public void GetDataOld() {
        var prefabs = ZNetScene.s_instance.m_namedPrefabs;
        try
        {
            foreach (var pair in prefabs)
            {
                var zdo_key = pair.Key;
                var obj     = pair.Value;
                if (obj.TryGetComponent<ZNetView>(out var _))
                {
                    var go = Util.CreateDummy(zdo_key);
                    if (obj.TryGetComponent<ZNetView>(out var znetView))
                    {
                        ZDO  zdo  = znetView.GetZDO();
                        var data = GetZdoVars(zdo);
                        data.Prefab = znetView.GetPrefabName();

                        foreach (var component in
                                 obj.GetComponents<Component>())
                        {
                            data.Components.Add(component.GetType().Name);
                        }

                        PrefabDataZDO.Add(data.Prefab, data);
                        //  GameObject.Destroy(go);
                    }
                    ;
                } else
                {
                    string       prefab = ((Object)obj).name;
                    List<string> p_list = new();
                    obj.name = obj.name.Replace(" ", "_");
                    foreach (var component in
                             obj.GetComponents<Component>())
                    {
                        p_list.Add(component.GetType().Name);
                    }
                    PrefabDataNoZDO.Add(prefab, p_list);
                }
            }
        } catch (Exception e)
        {
            System.Console.WriteLine("GetData" + e);
        }
    }*/
    public void GetData(string path) {
        var prefabs        = ZNetScene.s_instance.m_namedPrefabs;
        var Wrapper        = new GameObject();
        var SelectedPrefab = new GameObject();

        using (var initialStream = new FileStream(path, FileMode.Create,
                   FileAccess.Write, FileShare.Read))
        {
            // Optional: Schreib erste Info oder leer
            byte[] data = Encoding.UTF8.GetBytes("");
            ;
            initialStream.Write(data, 0, data.Length);
            initialStream.Flush();
            initialStream.Close();
        }
        try
        {
            
            
            using (var stream = new FileStream(path, FileMode.Append,
                       FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(stream))
            {
                SelectedPrefab.transform.SetParent(Wrapper.transform);
                SelectedPrefab.name                 = "ZDO Tester";
                SelectedPrefab.transform.localScale = Vector3.one;
                SelectedPrefab.transform.position =
                    Player.m_localPlayer.transform.position;
                var piece = SelectedPrefab.AddComponent<Piece>();
                piece.m_name = "ZDO Tester";
                Wrapper.SetActive(false);

                foreach (var pair in prefabs)
                {
                    var    zdo_key = pair.Key;
                    var    prefab  = pair.Value;
                    string text    = prefab.name.Split(' ', '(')[0];
                    writer.Write($"\"{{{prefab}\":");

                    if (prefab.TryGetComponent<ZNetView>(out var _))
                    {
                        // var go = Util.CreateDummy(zdo_key);

                        var view = prefab.GetComponent<ZNetView>();
                        //  var obj  = new GameObject();
                        GameObject? obj;
                        obj = Object.Instantiate(view.gameObject,
                            SelectedPrefab.transform.position,
                            Quaternion.identity);

                        if (!obj) { continue; }

                        if (obj.TryGetComponent<ZNetView>(out var znetView))
                        {
                            obj.SetActive(true);
                            ZDO zdo  = znetView.GetZDO();
                            var data = GetZdoVars(zdo);
                            writer.Write($"{{\"ZDOVars\":");
                            try
                            {
                                writer.Write(
                                    JsonUtility.ToJson(data));
                            } catch (NotSupportedException e)
                            {
                                continue;
                            }
                            writer.Write($",\"Components\":[\"");
                            obj.SetActive(false);
                            List<string> components = new();
                            foreach (var component in
                                     obj.GetComponents<Component>())
                            {
                                components.Add(component.GetType().Name);
                            }
                            writer.Write(string.Join("\",\"", components));
                            writer.Write($"\"]}}\n");
                            writer.Flush();
                            //Object.Destroy(obj);
                        }
                    } else
                    {
                        string       prefabName = ((Object)prefab).name;
                        List<string> p_list     = new();
                        prefabName = prefabName.Replace(" ", "_");
                        foreach (var component in
                                 prefab.GetComponents<Component>())
                        {
                            p_list.Add(component.GetType().Name);
                        }
                        PrefabDataNoZDO.Add(prefabName, p_list);
                    }
                }
            }
        } catch (Exception e)
        {
            System.Console.WriteLine("GetData" + e);
        }
    }
}
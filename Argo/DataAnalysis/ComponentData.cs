using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;
using UnityEngine;

namespace Argo.DataAnalysis;

public class ComponentData
{
    [Flags]
    [Serializable]
    [JsonConverter(typeof(JsonStringEnumConverter<ZdoVarFlag>))]
    public enum ZdoVarFlag
    {
        TUnknown    = 0,
        Tfloat      = 1 << 1,
        TVector3    = 1 << 2,
        TQuaternion = 1 << 3,
        Tint        = 1 << 4,
        Tlong       = 1 << 5,
        Tstring     = 1 << 6,
        TbyteArray  = 1 << 7,
        // this means the target type is bool but its stored as int in the ZDOvars
        TBoolAsInt = 1 << 8,
        // this means the target type is a Game but its stored as str in the ZDOvars
        // basically its a prefab name
        TGoAsStr = 1 << 9,
        // Target type is ItemdDrop, but its stored as string
        TItemAsStr = 1 << 10,
    }

    struct ComponentFieldInfo
    {
        public ZdoVarFlag ZdoVarFlag;
    }

    Dictionary<int, string> InitZdoUnhash = new Dictionary<int, string>();
    Dictionary<string, int> InitZdoHash = new Dictionary<string, int>();
    Dictionary<string, ZdoVarFlag> zdoVarType = [];
    Dictionary<string, Dictionary<string, ZdoVarFlag>> componentZDOs = [];

    /// <summary>
    /// it seems the default values of components are saved under a hash
    /// generated form "ComponentName.Fieldname". Usually the hash of a
    /// ZDOvar is just based on the Fieldname.
    /// This generates those combined hashs and saves them, for later lookup
    /// </summary>
    /// <param name="component"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    public int GetFieldHash(string component, string field) {
        int    hash;
        string name = component + "." + field;
        if (InitZdoHash.ContainsKey(name))
        {
            hash = InitZdoHash[name];
        } else
        {
            hash = name.GetStableHashCode();
            InitZdoHash.Add(name, hash);
            if (!InitZdoUnhash.ContainsKey(hash))
            {
                InitZdoUnhash.Add(hash, name);
            }
        }
        return hash;
    }

    /// <summary>
    /// zdos which contain init data seem to be a different format
    /// A hash of "HasFields" + "ComponentName" is used to test this
    /// </summary>
    /// <param name="zDO"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    public bool HasFields(ZDO zDO, string component) {
        if (!zDO.GetBool("HasFields"))
        {
            return  false;
        }
        int    hash;
        string name = "HasFields" + component;
        if (InitZdoHash.ContainsKey(name))
        {
            hash = InitZdoHash[name];
        } else
        {
            hash = name.GetStableHashCode();
            InitZdoHash.Add(name, hash);
            if (!InitZdoUnhash.ContainsKey(hash))
            {
                InitZdoUnhash.Add(hash, name);
            }
        }
        return true;
    }
  /// <summary>
  /// Adds the type of the Field to the General Fieldlist, aswell as the
  /// component specific list.
  /// </summary>
  /// <param name="fieldName"></param>
  /// <param name="flag"></param>
  /// <param name="componentVars"></param>
    public void AddInfo(string     fieldName,
                        ZdoVarFlag flag,
                        ref Dictionary<string, ZdoVarFlag>
                            componentVars) {
      
        if (zdoVarType.ContainsKey(fieldName))
        {
            zdoVarType[fieldName] |= flag;
        } else
        {
            zdoVarType.Add(fieldName, flag);
        }
        if (componentVars.ContainsKey(fieldName))
        {
            componentVars[fieldName] |= flag;
        } else
        {
            componentVars.Add(fieldName, flag);
        }
    }
/// <summary>
/// It seems the ZDO vars for the Default values of Components are stored under
/// a pretty different naming scheme. Instead of a hash of the ZDOVar name it
/// uses a hash generated from the Componets name and the Fieldnames they are
/// imported to 
/// </summary>
/// <param name="nview"></param>
    public void LoadFields(ZNetView nview) {
        ZDO zDO = nview.GetZDO();
        // in the ZNNetView component ony "HasFields" is used as hash
        if (!zDO.GetBool("HasFields"))
        {
            return;
        }
        List<MonoBehaviour> childs = new List<MonoBehaviour>();
        ((Component)nview).gameObject
                          .GetComponentsInChildren<MonoBehaviour>(
                               childs);

        foreach (MonoBehaviour tempComponent in childs)
        {
            string componentName = ((object)tempComponent).GetType().Name;
            int    hash;
            if (HasFields(zDO, componentName))
            {
                continue;
            }
            Dictionary<string, ZdoVarFlag> componentVars;
            if (componentZDOs.ContainsKey(componentName))
            {
                componentVars = componentZDOs[componentName];
            } else
            {
                componentVars = new Dictionary<string, ZdoVarFlag>();
                componentZDOs[componentName] = componentVars;
            }

            FieldInfo[] fields = ((object)tempComponent).GetType()
               .GetFields(BindingFlags.Instance | BindingFlags.Public);
            TestFields(fields, componentName, zDO, ref componentVars);
        }
    }

    /// <summary>
    /// it seems the default values of components are saved under a hash
    /// generated form "ComponentName.Fieldname". Usually the hash of a
    /// ZDOvar is just based on the Fieldname
    /// </summary>
    /// <param name="fieldInfos"></param>
    /// <param name="componentName"></param>
    /// <param name="zDo"></param>
    /// <param name="zdoVarFlags"></param>
       private void TestFields(FieldInfo[]                fieldInfos, string componentName, ZDO zDo,
                       ref Dictionary<string, ZdoVarFlag> zdoVarFlags) {
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                string fieldName = fieldInfo.Name;
                int  hash = GetFieldHash(componentName, fieldName);

                if (fieldInfo.FieldType == typeof(int) &&
                    zDo.GetInt((int)hash, out _))
                {
                    AddInfo(fieldName, ZdoVarFlag.Tint, ref zdoVarFlags);
                } else if (fieldInfo.FieldType == typeof(float) &&
                           zDo.GetFloat((int)hash, out _))
                {
                    AddInfo(fieldName, ZdoVarFlag.Tfloat, ref zdoVarFlags);
                } else if (fieldInfo.FieldType == typeof(bool) &&
                           zDo.GetBool((int)hash, out _))
                {
                    AddInfo(fieldName, ZdoVarFlag.TBoolAsInt,
                        ref zdoVarFlags);
                } else if (fieldInfo.FieldType == typeof(Vector3) &&
                           zDo.GetVec3((int)hash, out _))
                {
                    AddInfo(fieldName, ZdoVarFlag.TVector3, ref zdoVarFlags);
                } else if (fieldInfo.FieldType == typeof(string) &&
                           zDo.GetString((int)hash, out _))
                {
                    AddInfo(fieldName, ZdoVarFlag.Tstring, ref zdoVarFlags);
                } else if (fieldInfo.FieldType == typeof(GameObject) &&
                           zDo.GetString((int)hash, out _))
                {
                    AddInfo(fieldName, ZdoVarFlag.TGoAsStr, ref zdoVarFlags);
                } else if ((fieldInfo.FieldType == typeof(byte[])) &&
                           zDo.GetByteArray((int)hash, out _))
                {
                    AddInfo(fieldName, ZdoVarFlag.TbyteArray,
                        ref zdoVarFlags);
                } else if ((fieldInfo.FieldType == typeof(long)) &&
                           (zDo.GetLong(hash)   != 0))
                {
                    AddInfo(fieldName, ZdoVarFlag.Tlong, ref zdoVarFlags);
                } else if ((fieldInfo.FieldType == typeof(Quaternion)) &&
                           zDo.GetQuaternion((int)hash, out _))
                {
                    AddInfo(fieldName, ZdoVarFlag.TQuaternion,
                        ref zdoVarFlags);
                }
            
        }
    }
}
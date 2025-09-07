using System;
using System.Collections.Generic;
using System.Linq;
using Argo.Blueprint;
using Data;
using UnityEngine;
using Argo.Zdo;
using ArgoRegister = Argo.Registers.SettingsRegister;
using SaveExtraData  = Argo.Registers.SaveExtraData;

namespace InfinityHammer;

using ArgoZVars = Argo.Blueprint.ZVars;

//using static Argo.Blueprint.BpjZVars;
public static class ArgoExtensions
{
    public static readonly Dictionary<string, string> pars = new();

    public static ArgoZVars ToArgoZVars(this DataEntry? data, ArgoRegister mConfig) {
        return Convert(data, mConfig);
    }
    public static DataEntry ToDataEntry(this ArgoZVars argozdo) {
        DataEntry data = new DataEntry();
        data.Floats = argozdo.Floats?.ToDictionary(pair => pair.Key,
            pair => (IFloatValue)new SimpleFloatValue(pair.Value));
        data.Ints = argozdo.Ints?.ToDictionary((pair) => pair.Key, pair => (IIntValue)new SimpleIntValue(pair.Value));
        data.Longs = argozdo.Longs?.ToDictionary((pair) => pair.Key,
            pair => (ILongValue)new SimpleLongValue(pair.Value));
        data.Strings = argozdo.Strings?.ToDictionary((pair) => pair.Key,
            pair => (IStringValue)new SimpleStringValue(pair.Value));
        data.Vecs = argozdo.Vec3s?.ToDictionary((pair) => pair.Key,
            pair => (IVector3Value)new SimpleVector3Value(pair.Value));
        data.Quats = argozdo.Quats?.ToDictionary((pair) => pair.Key,
            pair => (IQuaternionValue)new SimpleQuaternionValue(pair.Value));
        data.ByteArrays = argozdo.ByteArrays?.ToDictionary((pair) => pair.Key, pair => pair.Value);
        return data;
    }
    public static ArgoZVars Convert(DataEntry? data, ArgoRegister mConfig) {
        var m_config = mConfig;
        if (mConfig.SaveMode != SaveExtraData.None) {
            var set = mConfig.Filter.Get();
            Func<int, bool> filter = (x) => {
                if (set.Contains(x)) return false;
                return true;
            };
            if (data != null) {
                return new ArgoZVars(
                    import<IFloatValue, float, float?>(data.Floats, filter, x => x.Get(pars)),
                    import<IIntValue, int, int?>(data.Ints, filter, x => x.Get(pars)),
                    import<ILongValue, long, long?>(data.Longs, filter, x => x.Get(pars)),
                    import<IStringValue, string, string?>(data.Strings, filter, x => x.Get(pars)),
                    import<IVector3Value, Vector3, Vector3?>(data.Vecs, filter, x => x.Get(pars)),
                    import<IQuaternionValue, Quaternion, Quaternion?>(data.Quats, filter, x => x.Get(pars)),
                    import<byte[], byte[], byte[]?>(data.ByteArrays, filter, x => x)
                );
            }
        }
        return new ArgoZVars();
    }

    static BinarySearchDictionary<int, U>? import<T, U, V>(
        IEnumerable<KeyValuePair<int, T>>? import,
        Func<int, bool> filter,
        Func<T, V> getter) {
        // var type = GetVType<T>();
        if (import != null) {
            var values = new BinarySearchDictionary<int, U>();
            import.Select(x => x).Aggregate(
                values,
                (target, pair) => {
                    if (filter(pair.Key)) {
                        var val = getter(pair.Value);
                        if (val != null) {
                            target[pair.Key] = (U)(object)val;
                        }
                    }
                    return target;
                }
            );
            if (values.Count > 0) {
                return values;
            }
        }
        return null;
    }

    public static MultiSelection CreateMultiSelection(GameObject? placementGhost_, ObjectSelection selection,
        Player player, ArgoRegister mConfig) {
        if (placementGhost_ == null) { throw new ArgumentNullException("No objects selected."); }
        var piece = placementGhost_.GetComponent<Piece>();

        string name;
        try {
            if (placementGhost_) {
                name = piece
                    ? Localization.instance.Localize(piece.m_name)
                    : Utils.GetPrefabName(placementGhost_);
            } else {
                name = Guid.NewGuid().ToString();
            }
        } catch (Exception e) {
            name = "";
            System.Console.WriteLine("ArgoWrapper: Error in GetPrefabName" + e);
        }
        var position = placementGhost_?.transform.position ?? Vector3.zero;
        var rotation = placementGhost_?.transform.rotation ?? Quaternion.identity;

        var selectionObejects = new List<SelectionObject>(selection.Objects.Count());
        selection.Objects.Select(x => x).Aggregate(
            selectionObejects,
            (target, obj) => {
                if (obj.GameObject.TryGetTarget(out var go) &&
                    ZNetScene.s_instance.m_namedPrefabs.TryGetValue(obj.Prefab, out var prefab)) {
                    string prefabName = Utility.GetPrefabName(go);
                    target.Add(new SelectionObject(prefabName,
                        obj.Data?.ToArgoZVars(mConfig) ?? null,
                        obj.Scalable,
                        go
                    ));
                }
                return target;
            }
        );
        MultiSelection multiSelection = new MultiSelection(selectionObejects,
            name,
            new SelectionHeader {
                Creator  = player.GetPlayerName(),
                Category = "InfinityHammer"
            },
            new Argo.Math.Transform(
                position,
                rotation),
            placementGhost_);
        
        return multiSelection;
    }

    public static BaseSelection ToBaseSelection(MultiSelection multiSelection) {
        throw new NotImplementedException(); // todo
    }
    /*public static BaseSelection ToBaseSelection(Terminal terminal, MultiSelection multiSelection, string name) {
        // todo maybe make interface for BaseSelection/Multiselection
        return new ObjectSelection(multiSelection.Wrapper, multiSelection.SelectedPrefab,
            multiSelection.Objects.Select(x => new SelectedObject {
                Prefab = x.Prefab,
                Data = x.data,
                Scalable = x.scalable
            }))
        multiSelection.SelectedObjects
    }*/
}
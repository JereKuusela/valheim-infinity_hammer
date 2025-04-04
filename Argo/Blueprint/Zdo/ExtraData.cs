using System.Collections.Generic;
using System.Linq;
using Argo.Blueprint;
using Argo.DataAnalysis;
using UnityEngine;

namespace Argo.Blueprint;




public abstract class AExtraData
{
    public          string m_name = "ExtraData";
    public          string Name  { get => m_name; set => m_name = value; }
    public abstract int    Count { get;            }
    public abstract bool   Remove<T>(int    hash);
    public virtual  bool   Remove<T>(string name) { return Remove<T>(name.GetStableHashCode()); }
    public abstract bool   TryAdd<T>(int    hash, T value);
    public virtual bool TryAdd<T>(string name, T value) {
        return TryAdd<T>(name.GetStableHashCode(), value);
    }
    public virtual bool TryAdd<T>(string name, T value, bool unknown) {
        if (unknown) {
            TryAdd<T>(int.Parse(name), value);
        }
        return TryAdd<T>(name.GetStableHashCode(), value);
    }
    public abstract bool TryGetValue<T>(int hash, out T value);
    public virtual bool TryGetValue<T>(string name, out T value) {
        return TryGetValue<T>(name.GetStableHashCode(), out value);
    }

    public abstract void SetValues<T>(IEnumerable<KeyValuePair<int, T>>? values);

    public abstract IEnumerable<KeyValuePair<int, T>>? GetValues<T>();

    public bool Pop<T>(int hash, out T zvalue) {
        if (GetValues<T>() != null) {
            if (this.TryGetValue<T>(hash, out zvalue)) {
                this.Remove<T>(hash);
                return true;
            }
            return false;
        }
        zvalue = default;
        return false;
    }
    public virtual bool PopFloat(int     hash, out float      zvalue) => Pop(hash, out zvalue);
    public virtual bool PopInt(int       hash, out int        zvalue) => Pop(hash, out zvalue);
    public virtual bool PopLong(int      hash, out long       zvalue) => Pop(hash, out zvalue);
    public virtual bool PopString(int    hash, out string     zvalue) => Pop(hash, out zvalue);
    public virtual bool PopVec3(int      hash, out Vector3    zvalue) => Pop(hash, out zvalue);
    public virtual bool PopQuat(int      hash, out Quaternion zvalue) => Pop(hash, out zvalue);
    public virtual bool PopByteArray(int hash, out byte[]     zvalue) => Pop(hash, out zvalue);

    public virtual bool TryGetFloat(int  hash, out float   zvalue) => TryGetValue(hash, out zvalue);
    public virtual bool TryGetInt(int    hash, out int     zvalue) => TryGetValue(hash, out zvalue);
    public virtual bool TryGetLong(int   hash, out long    zvalue) => TryGetValue(hash, out zvalue);
    public virtual bool TryGetString(int hash, out string  zvalue) => TryGetValue(hash, out zvalue);
    public virtual bool TryGetVec3(int   hash, out Vector3 zvalue) => TryGetValue(hash, out zvalue);
    public virtual bool TryGetQuat(int hash, out Quaternion zvalue) =>
        TryGetValue(hash, out zvalue);
    public virtual bool TryGetByteArray(int hash, out byte[] zvalue) =>
        TryGetValue(hash, out zvalue);

    public abstract List<int> GetKeys<T>();

    public bool RemoveAll<T>(HashSet<int> hashes, ref HashSet<int> rest) {
        bool ret = false;
        foreach (var kvp in GetValues<T>()!) {
            if (!hashes.Contains(kvp.Key)) {
                Remove<T>(kvp.Key);
                rest.Remove(kvp.Key);
                ret = true;
            }
        }
        return ret;
    }
    public HashSet<int> RemoveAll(HashSet<int> hashes) {
        var rest = new HashSet<int>();
        this.RemoveAll<float>(hashes, ref rest);
        this.RemoveAll<int>(hashes, ref rest);
        this.RemoveAll<long>(hashes, ref rest);
        this.RemoveAll<string>(hashes, ref rest);
        this.RemoveAll<Vector3>(hashes, ref rest);
        this.RemoveAll<Quaternion>(hashes, ref rest);
        this.RemoveAll<byte[]>(hashes, ref rest);
        return rest;
    }

    public HashSet<int> RemoveAll<T>(params int[] hashes) {
        var rest = new HashSet<int>();
        RemoveAll<T>(hashes.ToHashSet(), ref rest);
        return rest;
    }

    public HashSet<int> RemoveAll<T>(params string[] names) {
        var rest   = new HashSet<int>();
        var hashes = names.Select(x => x.GetStableHashCode()).ToHashSet();
        RemoveAll<T>(hashes, ref rest);
        return rest;
    }
    public abstract IEnumerable<KeyValuePair<int, T>>? PopAll<T>(
        HashSet<int> hashes);

    public virtual IEnumerable<KeyValuePair<int, T>>? PopAll<T>(
        params int[] hashes) => PopAll<T>(hashes.ToHashSet());
    public virtual IEnumerable<KeyValuePair<int, T>>? PopAll<T>(
        params string[] names) => PopAll<T>(names.Select(x => x.GetStableHashCode()).ToHashSet());

    public abstract AExtraData Create();
    public virtual AExtraData PopAll(HashSet<int> hashes) {
        var rest = new HashSet<int>();
        var data = Create();
        data.SetValues(this.PopAll<float>(hashes));
        data.SetValues(this.PopAll<int>(hashes));
        data.SetValues(this.PopAll<long>(hashes));
        data.SetValues(this.PopAll<string>(hashes));
        data.SetValues(this.PopAll<Vector3>(hashes));
        data.SetValues(this.PopAll<Quaternion>(hashes));
        data.SetValues(this.PopAll<byte[]>(hashes));
        return data;
    }
}


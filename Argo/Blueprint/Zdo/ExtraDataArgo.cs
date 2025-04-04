using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Argo.Blueprint;

public class ExtraDataArgo : AExtraData
{
    public Dictionary<int, float>?      s_floats     = null;
    public Dictionary<int, int>?        s_ints       = null;
    public Dictionary<int, long>?       s_longs      = null;
    public Dictionary<int, string>?     s_strings    = null;
    public Dictionary<int, Vector3>?    s_vec3s      = null;
    public Dictionary<int, Quaternion>? s_quats      = null;
    public Dictionary<int, byte[]>?     s_byteArrays = null;

    public override int Count {
        get {
            return s_floats?.Count ?? 0 +
                s_ints?.Count ?? 0 +
                s_longs?.Count ?? 0 +
                s_strings?.Count ?? 0 +
                s_vec3s?.Count ?? 0 +
                s_quats?.Count ?? 0 +
                s_byteArrays?.Count ?? 0;
        }
    }

    public ExtraDataArgo() { }
    public ExtraDataArgo(ref ExtraDataArgo rhs) {
        this.s_floats     = rhs.s_floats?.ToDictionary(e => e.Key, e => e.Value);
        this.s_ints       = rhs.s_ints?.ToDictionary(e => e.Key, e => e.Value);
        this.s_longs      = rhs.s_longs?.ToDictionary(e => e.Key, e => e.Value);
        this.s_strings    = rhs.s_strings?.ToDictionary(e => e.Key, e => e.Value);
        this.s_vec3s      = rhs.s_vec3s?.ToDictionary(e => e.Key, e => e.Value);
        this.s_quats      = rhs.s_quats?.ToDictionary(e => e.Key, e => e.Value);
        this.s_byteArrays = rhs.s_byteArrays?.ToDictionary(e => e.Key, e => e.Value);
    }

    public override AExtraData Create() { return (AExtraData)new ExtraDataArgo(); }

    public override IEnumerable<KeyValuePair<int, T>>? GetValues<T>() {
        if (typeof(T) == typeof(float)) { return (IEnumerable<KeyValuePair<int, T>>?)s_floats; }
        if (typeof(T) == typeof(int)) { return (IEnumerable<KeyValuePair<int, T>>?)s_ints; }
        if (typeof(T) == typeof(long)) { return (IEnumerable<KeyValuePair<int, T>>?)s_longs; }
        if (typeof(T) == typeof(string)) { return (IEnumerable<KeyValuePair<int, T>>?)s_strings; }
        if (typeof(T) == typeof(Vector3)) { return (IEnumerable<KeyValuePair<int, T>>?)s_vec3s; }
        if (typeof(T) == typeof(Quaternion)) { return (IEnumerable<KeyValuePair<int, T>>?)s_quats; }
        if (typeof(T) == typeof(byte[])) {
            return (IEnumerable<KeyValuePair<int, T>>?)s_byteArrays;
        } else throw new ArgumentException($"Unsupported data type: {typeof(T)}");
    }

    public override IEnumerable<KeyValuePair<int, T>>? PopAll<T>(
        HashSet<int> hashes) {
        Dictionary<int, T>? values = new Dictionary<int, T>();
        if (GetValues<T>() != null) {
            foreach (var kvp in GetValues<T>()!) {
                if (!hashes.Contains(kvp.Key)) {
                    Remove<T>(kvp.Key);
                    values.Add(kvp.Key, kvp.Value);
                }
            }
        }
        return values;
    }

    public override void SetValues<T>(IEnumerable<KeyValuePair<int, T>>? values) {
        if (typeof(T) == typeof(float)) {
            s_floats = (Dictionary<int, float>)values;
        } else if (typeof(T) == typeof(int)) {
            s_ints = (Dictionary<int, int>)values;
        } else if (typeof(T) == typeof(long)) {
            s_longs = (Dictionary<int, long>)values;
        } else if (typeof(T) == typeof(string)) {
            s_strings = (Dictionary<int, string>)values;
        } else if (typeof(T) == typeof(Vector3)) {
            s_vec3s = (Dictionary<int, Vector3>)values;
        } else if (typeof(T) == typeof(Quaternion)) {
            s_quats = (Dictionary<int, Quaternion>)values;
        } else if (typeof(T) == typeof(byte[])) {
            s_byteArrays = (Dictionary<int, byte[]>)values;
        } else throw new ArgumentException($"Unsupported data type: {typeof(T)}");
    }
    public override List<int> GetKeys<T>() {
        Dictionary<int, T>? values = (Dictionary<int, T>?)GetValues<T>();
        return values?.Keys.ToList() ?? new List<int>();
    }

    public override bool Remove<T>(int hash) {
        Dictionary<int, T>? values = (Dictionary<int, T>?)GetValues<T>();
        return values?.Remove(hash) ?? false;
    }

    public override bool TryAdd<T>(int hash, T value) {
        Dictionary<int, T>? values = (Dictionary<int, T>?)GetValues<T>();
        if (values == null) {
            values = new Dictionary<int, T>();
            values.Add(hash, value);
            SetValues<T>(values);
            return true;
        } else if (!values.ContainsKey(hash)) {
            values.Add(hash, value);
            return true;
        }
        return false;
    }
    public override bool TryGetValue<T>(int hash, out T value) {
        Dictionary<int, T>? values = (Dictionary<int, T>?)GetValues<T>();
        if (values != null && values.TryGetValue(hash, out value)) {
            return true;
        }
        value = default;
        return false;
    }
}
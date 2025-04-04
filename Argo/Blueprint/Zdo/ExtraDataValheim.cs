using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Argo.Blueprint;

public  class ExtraDataValheim : AExtraData
{
    public BinarySearchDictionary<int, float>?      s_floats     = null;
    public BinarySearchDictionary<int, int>?        s_ints       = null;
    public BinarySearchDictionary<int, long>?       s_longs      = null;
    public BinarySearchDictionary<int, string>?     s_strings    = null;
    public BinarySearchDictionary<int, Vector3>?    s_vec3s      = null;
    public BinarySearchDictionary<int, Quaternion>? s_quats      = null;
    public BinarySearchDictionary<int, byte[]>?     s_byteArrays = null;
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
    public ExtraDataValheim() { }
    public ExtraDataValheim(ref ExtraDataValheim rhs) {
        this.s_floats     = (BinarySearchDictionary<int, float>?) rhs.s_floats?.Clone();
        this.s_ints       = (BinarySearchDictionary<int, int>?) rhs.s_ints?.Clone();
        this.s_longs      = (BinarySearchDictionary<int, long>?) rhs.s_longs?.Clone();
        this.s_strings    = (BinarySearchDictionary<int, string>?) rhs.s_strings?.Clone();
        this.s_vec3s      = (BinarySearchDictionary<int, Vector3>?) rhs.s_vec3s?.Clone();
        this.s_quats      = (BinarySearchDictionary<int, Quaternion>? ) rhs.s_quats?.Clone();
        this.s_byteArrays = (BinarySearchDictionary<int, byte[]>?) rhs.s_byteArrays?.Clone();
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
        BinarySearchDictionary<int, T>? values = new BinarySearchDictionary<int, T>();
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
            s_floats = (BinarySearchDictionary<int, float>)values;
        } else if (typeof(T) == typeof(int)) {
            s_ints = (BinarySearchDictionary<int, int>)values;
        } else if (typeof(T) == typeof(long)) {
            s_longs = (BinarySearchDictionary<int, long>)values;
        } else if (typeof(T) == typeof(string)) {
            s_strings = (BinarySearchDictionary<int, string>)values;
        } else if (typeof(T) == typeof(Vector3)) {
            s_vec3s = (BinarySearchDictionary<int, Vector3>)values;
        } else if (typeof(T) == typeof(Quaternion)) {
            s_quats = (BinarySearchDictionary<int, Quaternion>)values;
        } else if (typeof(T) == typeof(byte[])) {
            s_byteArrays = (BinarySearchDictionary<int, byte[]>)values;
        } else throw new ArgumentException($"Unsupported data type: {typeof(T)}");
    }
    public override List<int> GetKeys<T>() {
        BinarySearchDictionary<int, T>? values = (BinarySearchDictionary<int, T>?)GetValues<T>();
        return values?.Keys.ToList() ?? new List<int>();
    }

    public override bool Remove<T>(int hash) {
        BinarySearchDictionary<int, T>? values = (BinarySearchDictionary<int, T>?)GetValues<T>();
        return values?.Remove(hash) ?? false;
    }

    public override bool TryAdd<T>(int hash, T value) {
        BinarySearchDictionary<int, T>? values = (BinarySearchDictionary<int, T>?)GetValues<T>();
        if (values == null) {
            values = new BinarySearchDictionary<int, T>();
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
        BinarySearchDictionary<int, T>? values = (BinarySearchDictionary<int, T>?)GetValues<T>();
        if (values != null && values.TryGetValue(hash, out value)) {
            return true;
        }
        value = default;
        return false;
    }
}
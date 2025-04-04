using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Argo.Blueprint.Util;
using Argo.Util;
using UnityEngine;

namespace Argo.Blueprint;

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct Tags
{
    // careful here editing this code. thie is the very ugly C# equivalent of an union,
    // never write to tags and buffer/buffersize at the same time. if count > SIZE
    // the memory of tags is used to store the reference and address of the rented buffer
    // therefor reading or writing to tags when buffer is used and vice versa will lead to bugs
    // this code can actually be written very save in c++ but its a safty mess in C#
    private const                   int   SIZE = 4;
    [FieldOffset(0)]  private       short count;
    [FieldOffset(2)]  internal      bool  isbuffer = false;
    [FieldOffset(4)]  private fixed int   tags[4];
    [FieldOffset(8)]  internal      int[] buffer;
    [FieldOffset(16)] private       int   buffersize;
    public Tags() {
        count    = 0;
        isbuffer = false;
    }
    public int Count => count;
    public int this[int index] {
        get {
            if ((index < 0) || (index >= count))
                throw new IndexOutOfRangeException();
            if (isbuffer) {
                return buffer[index];
            } else {
                return tags[index];
            }
        }
        set {
            if ((index < 0) || (index >= count))
                throw new IndexOutOfRangeException();
            if (isbuffer) {
                buffer[index] = value;
                return;
            } else {
                tags[index] = value;
                return;
            }
        }
    }
    public void Add(BpoTag tag) {
        if (!isbuffer) {
            if ((count < SIZE)) {
                tags[count] = tag;
                count++;
            } else {
                int[] array = ArrayPool<int>.Shared.Rent(8);
                for (int i = 0; i < count; i++) {
                    array[i] = tags[i];
                }
                isbuffer      = true;
                buffer        = array;
                buffersize    = 8;
                buffer[count] = tag;
                count++;
            }
        } else {
            if (count < buffersize) {
                buffer[count] = tag;
                count++;
            } else {
                int[] array = ArrayPool<int>.Shared.Rent(buffersize << 1);
                for (int i = 0; i < count; i++) {
                    array[i] = buffer[i];
                }
                ArrayPool<int>.Shared.Return(buffer);
                buffer        = array;
                buffersize    = buffersize << 1;
                buffer[count] = tag;
                count++;
            }
        }
    }
    public void Add(BpoTag[] tags) {
        if ((tags.Length + count) >= SIZE) throw new Exception("Too many tags");

        for (int i = 0; i < tags.Length; i++) {
            Add(tags[i]);
        }
    }
    public void Remove(int index) {
        if (index < 0 || index >= count) throw new IndexOutOfRangeException();
        for (int i = index; i < count - 1; i++) {
            tags[i] = tags[i + 1];
        }
        count--;
    }
}

public class BpjObjectNew
{
    // name and type will be combined to: blueptrint: name, or prefab: name
    //  if type is blueprint or prefab
    public                                           string   name = "";
    public                                           BpoType  type;
    [JsonInclude] [JsonPropertyName("flags")] public BPOFlags flags;
    [JsonInclude]
    [JsonPropertyName("p")]
    public Vector3 pos = Vector3.zero; // todo may remove unity dependency here
    [JsonInclude] [JsonPropertyName("r")]      public Quaternion rot = Quaternion.identity;
    [JsonInclude] [JsonPropertyName("s")]      public Vector3 scale = Vector3.one;
    [JsonInclude] [JsonPropertyName("chance")] public float chance = 1.0f;
    internal Tags tags = new Tags(); // todo write serializer for json
    public Dictionary<string, BpoComponent> data = new();
    ~BpjObjectNew() {
        if (tags.isbuffer) {
            // never remove this code as long as Tags is used.
            ArrayPool<int>.Shared.Return(tags.buffer);
        }
    }
}
// todo write index (relative to start of blueprint) of objects in extended blueprints or mor like 
//      full json compliant

public struct BpoTag
{
    // todo bring in line with the other registers
    public static readonly ImmutableDictionary<int, ConstStr> DefaultToName = CreateDefaultToName();
    public static readonly ImmutableDictionary<ConstStr, int>
        DefaulktToHash = CreateDefaultToHash();
    public static ImmutableDictionary<int, ConstStr> ToName = CreateDefaultToName();
    public static ImmutableDictionary<ConstStr, int> ToHash = CreateDefaultToHash();

    public readonly ConstStr name;
    public readonly int      hash;
    private BpoTag(string name) {
        this.name = name;
        this.hash = name.GetStableHashCode();
    }
    public static BpoTag Create(string name) {
        var tag = new BpoTag(name);
        ToName = ToName.Add(Variant.hash, Variant.name);
        ToHash = ToHash.Add(Negative.name, Negative.hash);
        return tag;
    }

    public static ImmutableDictionary<int, ConstStr> CreateDefaultToName() {
        var dic = ImmutableDictionary<int, ConstStr>.Empty;
        dic = dic.Add(Variant.hash, Variant.name);
        dic = dic.Add(Negative.hash, Negative.name);
        return dic;
    }
    public static ImmutableDictionary<ConstStr, int> CreateDefaultToHash() {
        var dic = ImmutableDictionary<ConstStr, int>.Empty;
        dic = dic.Add(Variant.name, Variant.hash);
        dic = dic.Add(Negative.name, Negative.hash);
        return dic;
    }
    public static BpoTag Variant  => new("Variant");
    public static BpoTag Negative => new("Negative");

    public static implicit operator int(BpoTag tag) => tag.hash;
}

public interface BpoData { }

public enum PrefabTypeEnum : byte
{
    Vanilla,
    Mod,
    Unknown
}

public enum BpoTypeEnum : ushort
{
    Unknown = 0,
    Prefab = 1,
    Blueprint = 2,
    Deleted = 3, // mainy for manually editing blueprints which dont have an idex field to preserve order
    Reference = 4,
}

[Flags]
public enum BpoFlagEnum : ushort
{
    Vanilla = 1,
    Modded = 2,
    ModdedAndVanilla = Vanilla | Modded,
    Reference = 1 << 2, // todo add something like resolved reference for pieces that where based on a reference, but are copied now
    Variant = 1 << 3, // todo use prefab/blueprint as json propertynames?
    Target = 1 << 4, // when a reference is used this should be set if its the target of the reference
    Resolved = 1 << 5, // means the content of the referenced object have been copied in place, only for internal use when creating a selection
    Partitial = 1 << 6, // means only a part of the reference will used
    InPlace = 1 << 7, // instead of creating a reference to a blueprint or variant it will be stored inplace
    Refferer = 1 << 8, // means the object is a reference to another object
    // todo simply save other stuff like negativ as tags?
    Negative, // todo a negative that removes either certain pieces or wholes groups of pieces,maybe even radius
}

public struct BpoType
{
    public BpoTypeEnum m_basetype;
    public BpoFlagEnum m_flags;
}
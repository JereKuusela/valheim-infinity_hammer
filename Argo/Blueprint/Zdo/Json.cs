using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Argo.Blueprint.Utility;
using Argo.DataAnalysis;
using Argo.Zdo;
using UnityEngine;

namespace Argo.Blueprint;

public class ExtraDataJson : JsonConverter<AExtraData>
{
    static readonly Vec3JsonConverter vec3JsonConverter = new Vec3JsonConverter();
    static readonly QuatJsonConverter quatJsonConverter = new QuatJsonConverter();
    public override void Write(
        Utf8JsonWriter        writer, AExtraData zvar,
        JsonSerializerOptions options) {
        try {
            var info = Config.GetHashLookup();
            writer.WritePropertyName(zvar.Name);
            writer.WriteStartObject();

            if (zvar.GetValues<int>() is { } ints) {
                foreach (var pair in ints) {
                    writer.WritePropertyName(info.ToJsonPropertyNameInt(pair.Key));
                    writer.WriteNumberValue(pair.Value);
                }
            }
            if (zvar.GetValues<long>() is { } longs) {
                foreach (var pair in longs) {
                    writer.WritePropertyName(info.ToJsonPropertyNameLong(pair.Key));
                    writer.WriteNumberValue(pair.Value);
                }
            }
            if (zvar.GetValues<float>() is { } floats) {
                foreach (var pair in floats) {
                    writer.WritePropertyName(info.ToJsonPropertyNameFloat(pair.Key));
                    writer.WriteNumberValue(pair.Value);
                }
            }
            if (zvar.GetValues<string>() is { } strings) {
                foreach (var pair in strings) {
                    writer.WritePropertyName(info.ToJsonPropertyNameString(pair.Key));
                    writer.WriteStringValue(pair.Value);
                }
            }
            if (zvar.GetValues<Vector3>() is { } vecs) {
                foreach (var pair in vecs) {
                    writer.WritePropertyName(info.ToJsonPropertyNameVec3(pair.Key));
                    vec3JsonConverter.Write(writer, pair.Value, options);
                }
            }
            if (zvar.GetValues<Quaternion>() is { } quats) {
                foreach (var pair in quats) {
                    writer.WritePropertyName(info.ToJsonPropertyNameQuat(pair.Key));
                    quatJsonConverter.Write(writer, pair.Value, options);
                }
            }
            if (zvar.GetValues<byte[]>() is { } bytearrays) {
                foreach (var pair in bytearrays) {
                    writer.WritePropertyName(info.ToJsonPropertyNameByteArray(pair.Key));
                    var value = Convert.ToBase64String(pair.Value);
                    writer.WriteStringValue((value));
                }
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
        } catch (Exception e) {
            System.Console.WriteLine("Error in Json Serializer BpjZVars.ZValue " + e);
            throw e;
        }
    }

    public override AExtraData Read(
        ref Utf8JsonReader    reader, Type type,
        JsonSerializerOptions options) {
        try {
            if (reader.TokenType != JsonTokenType.PropertyName)
                reader.Read();
            ExtraDataArgo                data       = new ExtraDataArgo();
            while (reader.TokenType != JsonTokenType.EndObject) {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Zvalue1: Expected Property Name.");
                string name = reader.GetString() ??
                    throw new JsonException("Propertyname is Empty");

                char prefix = name[0];
                bool unknown;
                if (prefix == 'u') {
                    prefix  = name[1];
                    name    = name.Substring(2);
                    unknown = true;
                } else {
                    name    = name.Substring(1);
                    unknown = false;
                }
                reader.Read();
             
                switch (prefix) {
                    case (char)ZDOType.Float:
                        data.TryAdd<float>(name, reader.GetSingle(), unknown);
                        break;
                    case (char)ZDOType.Vec3:
                        data.TryAdd<Vector3>(name,
                            vec3JsonConverter.Read(ref reader, typeof(Quaternion), options),
                            unknown);
                        break;
                    case (char)ZDOType.Quat:
                        data.TryAdd<Quaternion>(name,
                            quatJsonConverter.Read(ref reader, typeof(Quaternion), options),
                            unknown);
                        break;
                    case (char)ZDOType.Int:
                        data.TryAdd<int>(name, reader.GetInt32(), unknown);
                        break;
                    case (char)ZDOType.Long:
                        data.TryAdd<long>(name, reader.GetInt64(), unknown);
                        break;
                    case (char)ZDOType.String:
                        data.TryAdd<string>(name, reader.GetString() ?? "", unknown);
                        break;
                    case (char)ZDOType.ByteArray:
                        var    str   = reader.GetString() ?? "";
                        byte[] bytes = Convert.FromBase64String(str);
                        data.TryAdd<byte[]>(name, bytes, unknown);
                        break;
                    default:
                        throw new JsonException("Unknown type");
                }
                reader.Read();
            }
            return data;
        } catch (Exception e) {
            System.Console.WriteLine("Error in Json Serializer ZValue" + e);
            System.Console.WriteLine("TokenType:" + reader.TokenType.ToString());
            System.Console.WriteLine("Bytes" + reader.BytesConsumed);
            throw e;
        }
    }
}
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;

namespace Argo.Blueprint.Json;

// todo might move to its own file
public class BpjDataJson : JsonConverter<BpjObject.TData>
{
    static readonly Vec3JsonConverter vec3JsonConverter = new Vec3JsonConverter();
    static readonly QuatJsonConverter quatJsonConverter = new QuatJsonConverter();
    public override void Write(
        Utf8JsonWriter        writer, BpjObject.TData data,
        JsonSerializerOptions options) {
        // todo remove fields with default values (scale and odds)
        try {
            writer.WriteStartObject();
            writer.WritePropertyName("flags");
            writer.WriteNumberValue((uint)(data.flags));
            writer.WritePropertyName("p");
            vec3JsonConverter.Write(writer, data.pos, options);
            writer.WritePropertyName("r");
            quatJsonConverter.Write(writer, data.rot, options);
            writer.WritePropertyName("s");
            vec3JsonConverter.Write(writer, data.scale, options);
            writer.WritePropertyName("odds");
            writer.WriteNumberValue(data.chance);
            writer.WriteEndObject();
        } catch (Exception e) {
            System.Console.WriteLine("Error in Json Serializer Bpjobject " + e);
            throw;
        }
    }
    public override BpjObject.TData Read(
        ref Utf8JsonReader    reader, Type type,
        JsonSerializerOptions options) {
        reader.Read();
        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();
        reader.Read();
        BPOFlags   flags = 0;
        Vector3    p     = Vector3.zero;
        Quaternion r     = Quaternion.identity;
        Vector3    s     = Vector3.zero;
        float      odds  = 0f;
        while (reader.TokenType != JsonTokenType.EndObject) {
            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();
            string p_name = reader.GetString() ?? "";
            switch (p_name) {
                case "flags":
                    reader.Read();
                    flags = (BPOFlags)reader.GetUInt32();
                    break;
                case "p":
                    p = vec3JsonConverter.Read(ref reader, typeof(Vector3), options);
                    break;
                case "r":
                    r = quatJsonConverter.Read(ref reader, typeof(Quaternion), options);
                    break;
                case "s":
                    s = vec3JsonConverter.Read(ref reader, typeof(Vector3), options);
                    break;
                case "odds":
                    reader.Read();
                    odds = reader.GetSingle();
                    break;
                default:
                    break;
            }
            reader.Read();
        }
        return new BpjObject.TData(flags, p, r, s, odds);
    }
}

public class BpObjectJson : JsonConverter<BpjObject>
{
    private readonly static BpjDataJson DataJson = new BpjDataJson();
    private readonly static ExtraDataJson    ExtraDataJson  = new ExtraDataJson();
    public override void Write(
        Utf8JsonWriter        writer, BpjObject? Bpo,
        JsonSerializerOptions options) {
        if (Bpo == null) { return; }
        try {
            writer.WriteStartObject();
            writer.WritePropertyName(Bpo.m_prefab);

            DataJson.Write(writer, Bpo.m_baseData, options);
            //JsonSerializer.Serialize<BpjObject.TData>(writer, Bpo.m_data, options);
            if (Bpo.m_extraData.Count != 0) {
                writer.WritePropertyName("ZDOVars");

                ExtraDataJson.Write(writer, Bpo.m_extraData, options);
            }
            writer.WriteEndObject();
            writer.WriteRawValue("\n", true);
        } catch (Exception e) {
            System.Console.WriteLine("Error in Json Serializer Bpjobject " + e);
            throw e;
        }
    }

    public override BpjObject Read(
        ref Utf8JsonReader    reader, Type type,
        JsonSerializerOptions options) {
        try {
            reader.Read();
            if (reader.TokenType == JsonTokenType.StartObject) {
                reader.Read();
            }
            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("BlueprintObject3: Expected PropertyName.");
            }
            string prefab = reader.GetString();
            BpjObject.TData data
                = JsonSerializer.Deserialize<BpjObject.TData>(ref reader, options);
            BpjObject bpo = new BpjObject(prefab, data);
            reader.Read();
            if (reader.TokenType == JsonTokenType.EndObject)
                return bpo;

            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();

            string temp = reader.GetString() ?? "";
            if ((string.Equals(temp, "ZDOVars", StringComparison.InvariantCultureIgnoreCase)) ||
                (string.Equals(temp, "ExtraData", StringComparison.InvariantCultureIgnoreCase))) {
                reader.Read();

                AExtraData extraData = ExtraDataJson.Read(ref reader, typeof(AExtraData), options);

                bpo.ZVars = extraData;
                reader.Read();

            }
            if (reader.TokenType != JsonTokenType.EndObject)
                throw new JsonException("Expected end of Object.");
            reader.Read();
            return bpo;
        } catch (Exception e) {
            System.Console.WriteLine("Error in Json Serializer BpjObject" + e);
            System.Console.WriteLine("TokenType:" + reader.TokenType.ToString());
            System.Console.WriteLine("Bytes" + reader.BytesConsumed);
            throw e;
        }
    }
}
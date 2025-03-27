using UnityEngine;

namespace Argo.blueprint.Util;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Vec3JsonConverter : JsonConverter<Vector3>
{
    // Schreiben des Vector3 als Array [x,y,z]
    public override void Write(Utf8JsonWriter writer, Vector3 vec, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(vec.x);
        writer.WriteNumberValue(vec.y);
        writer.WriteNumberValue(vec.z);
        writer.WriteEndArray();
    }

    // Lesen aus einem JSON Array [x,y,z]
    public override Vector3 Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array.");

        Vector3 vec = new();
        reader.Read();
        vec.x = reader.GetSingle();
        reader.Read();
        vec.y = reader.GetSingle();
        reader.Read();
        vec.z = reader.GetSingle();

        reader.Read(); // EndArray

        if (reader.TokenType != JsonTokenType.EndArray)
            throw new JsonException("Expected end of array.");

        return vec;
    }
}
public class QuatJsonConverter : JsonConverter<Quaternion>
{
    // Schreiben des Vector3 als Array [x,y,z]
    public override void Write(Utf8JsonWriter writer, Quaternion quat, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(quat.x);
        writer.WriteNumberValue(quat.y);
        writer.WriteNumberValue(quat.z);
        writer.WriteNumberValue(quat.w);
        writer.WriteEndArray();
    }

    // Lesen aus einem JSON Array [x,y,z]
    public override Quaternion Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array.");

        Quaternion quat = new();
        reader.Read();
        quat.x = reader.GetSingle();
        reader.Read();
        quat.y = reader.GetSingle();
        reader.Read();
        quat.z = reader.GetSingle();
        reader.Read();
        quat.w = reader.GetSingle();
        reader.Read(); // EndArray

        if (reader.TokenType != JsonTokenType.EndArray)
            throw new JsonException("Expected end of array.");

        return quat;
    }
}
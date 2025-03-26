namespace Argo.blueprint.Util;

using System;
using System.Numerics; // Vector3 hier definiert, oder UnityEngine, falls in Unity.
using System.Text.Json;
using System.Text.Json.Serialization;

public class Vec3JsonConverter : JsonConverter<Vector3>
{
    // Schreiben des Vector3 als Array [x,y,z]
    public override void Write(Utf8JsonWriter writer, Vector3 vec, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(vec.X);
        writer.WriteNumberValue(vec.Y);
        writer.WriteNumberValue(vec.Z);
        writer.WriteEndArray();
    }

    // Lesen aus einem JSON Array [x,y,z]
    public override Vector3 Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array.");

        Vector3 vec = new();
        reader.Read();
        vec.X = reader.GetSingle();
        reader.Read();
        vec.Y = reader.GetSingle();
        reader.Read();
        vec.Z = reader.GetSingle();

        reader.Read(); // EndArray

        if (reader.TokenType != JsonTokenType.EndArray)
            throw new JsonException("Expected end of array.");

        return vec;
    }
}
public class QuatJsonConverter : JsonConverter<Quaternion>
{
    // Schreiben des Vector3 als Array [x,y,z]
    public override void Write(Utf8JsonWriter writer, Quaternion vec, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(vec.X);
        writer.WriteNumberValue(vec.Y);
        writer.WriteNumberValue(vec.Z);
        writer.WriteEndArray();
    }

    // Lesen aus einem JSON Array [x,y,z]
    public override Quaternion Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array.");

        Quaternion quat = new();
        reader.Read();
        quat.X = reader.GetSingle();
        reader.Read();
        quat.Y = reader.GetSingle();
        reader.Read();
        quat.Z = reader.GetSingle();
        reader.Read();
        quat.W = reader.GetSingle();
        reader.Read(); // EndArray

        if (reader.TokenType != JsonTokenType.EndArray)
            throw new JsonException("Expected end of array.");

        return quat;
    }
}
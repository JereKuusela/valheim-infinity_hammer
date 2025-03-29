using System.Collections.Generic;
using Argo.Blueprint;
using UnityEngine;

namespace Argo.blueprint.Util;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class BpHeaderConverter : JsonConverter<BlueprintHeader>
{
    static readonly         Vec3JsonConverter           vec3JsonConverter = new Vec3JsonConverter();
    private static          JsonConverter<BlueprintMod> ModConverter;
    private static readonly JsonSerializerOptions       options;
    static BpHeaderConverter() {
        options = new JsonSerializerOptions {
            WriteIndented = false
        };
        options.Converters.Add( new Vec3JsonConverter() );
        ModConverter = (JsonConverter<BlueprintMod>)options
           .GetConverter( typeof(BlueprintMod) );
    }
    public override void Write(
        Utf8JsonWriter        writer, BlueprintHeader header,
        JsonSerializerOptions options) {
        try {
            writer.WriteStartObject();

            writer.WritePropertyName( "Name" );
            writer.WriteStringValue( header.Name );
            writer.WriteRawValue( "\n", true );
            writer.WritePropertyName( "Creator" );
            writer.WriteStringValue( header.Creator );
            writer.WriteRawValue( "\n", true );
            writer.WritePropertyName( "Description" );
            writer.WriteStringValue( header.Description );
            writer.WriteRawValue( "\n", true );
            writer.WritePropertyName( "Coordinates" );
            vec3JsonConverter.Write( writer, header.Coordinates, options );
            writer.WriteRawValue( "\n", true );
            writer.WritePropertyName( "Rotation" );
            vec3JsonConverter.Write( writer, header.Rotation, options );
            writer.WriteRawValue( "\n", true );
            writer.WritePropertyName( "Category" );
            writer.WriteStringValue( header.Category );
            writer.WriteRawValue( "\n", true );
            writer.WritePropertyName( "Radius" );
            writer.WriteNumberValue( header.Radius );
            writer.WriteRawValue( "\n", true );
            writer.WritePropertyName( "Version" );
            writer.WriteStringValue( header.Version );
            writer.WriteRawValue( "\n", true );
            if (header.SnapPoints.Count > 0) {
                writer.WritePropertyName( "SnapPoints" );
                writer.WriteStartArray();
                foreach (var snapPoint in header.SnapPoints) {
                    vec3JsonConverter.Write( writer, snapPoint, options );
                }
                writer.WriteEndArray();
                writer.WriteRawValue( "\n", true );
            }
            if (header.Mods.Count > 0) {
                writer.WritePropertyName( "Mods" );
                writer.WriteStartArray();
                foreach (var mod in header.Mods) {
                    ModConverter.Write( writer, mod, options );
                }
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
            writer.WriteRawValue( "\n", true );
        } catch (Exception e) {
            System.Console.WriteLine( "Error in Json Serializer Bpjobject " + e );
            throw e;
        }
    }

    public override BlueprintHeader Read(
        ref Utf8JsonReader    reader, Type type,
        JsonSerializerOptions options) {
        try {
            reader.Read();
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException( "Header: Expected start of of object." );

            string             Name_        = "";
            string             Creator_     = "";
            string             Description_ = "";
            Vector3            Coordinates_ = new Vector3();
            Vector3            Rotation_    = new Vector3();
            string             Category_    = "";
            float              Radius_      = 0f;
            string             Version_     = "";
            List<BlueprintMod> Mods_        = [];
            List<Vector3>      Snappoints_  = [];
            while (reader.Read()) {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();
                string property = reader.GetString();
                switch (property) {
                    case ("Name"):       reader.Read(); Name_        = reader.GetString(); break;
                    case ("Creator"):    reader.Read(); Creator_     = reader.GetString(); break;
                    case ("Description"):reader.Read(); Description_ = reader.GetString(); break;
                    case ("Coordinates"):
                        Coordinates_
                            = vec3JsonConverter.Read( ref reader, typeof(Vector3), options );
                        break;
                    case ("Rotation"):
                        Rotation_ = vec3JsonConverter.Read( ref reader, typeof(Vector3), options );
                        break;
                    case ("Category"): reader.Read();  Category_ = reader.GetString(); break;
                    case ("Radius"):   reader.Read();  Radius_   = reader.GetSingle(); break;
                    case ("Version"):  reader.Read();  Version_  = reader.GetString(); break;
                    case ("Mods"):
                        reader.Read();
                        if (reader.TokenType != JsonTokenType.StartArray)
                            throw new JsonException( "Header1: Expected start of array." );
                        while (reader.Read()) {
                            if (reader.TokenType == JsonTokenType.EndArray)
                                break;
                            var mod = ModConverter.Read( ref reader, typeof(BlueprintMod),
                                options );
                            // if (mod != null){
                            Mods_.Add( mod );
                            //}
                           
                        }
                        break;
                    case ("SnapPoints"):
                        reader.Read();
                        if (reader.TokenType != JsonTokenType.StartArray)
                            throw new JsonException( "Header2: Expected start of array." );
                        while (reader.Read()) {
                                if (reader.TokenType == JsonTokenType.EndArray) {
                                    break;
                                }
                            var vec = vec3JsonConverter.Read( ref reader, typeof(Vector3),
                                options );
                            // if (mod != null){
                            Snappoints_.Add( vec );
                            //}
                            ;
                        }
                        break;
                    default:
                        while (reader.Read()) {
                            if (reader.TokenType == JsonTokenType.EndObject)
                                break;
                        }
                        break;
                }
            }
            return new BlueprintHeader {
                Name        = Name_,
                Creator     = Creator_,
                Description = Description_,
                Coordinates = Coordinates_,
                Rotation    = Rotation_,
                Category    = Category_,
                Radius      = Radius_,
                Version     = Version_,
                Mods        = Mods_,
                SnapPoints  = Snappoints_
            };
        } catch (Exception e) {
            System.Console.WriteLine( "Error in Json Serializer BlueprintHeader" + e );
            System.Console.WriteLine( "TokenType:"                         + reader.TokenType.ToString() );
            System.Console.WriteLine( "Bytes" + reader.BytesConsumed);
            throw e;
        }
    }
}

public class Vec3JsonConverter : JsonConverter<Vector3>
{
    // Schreiben des Vector3 als Array [x,y,z]
    public override void Write(
        Utf8JsonWriter writer, Vector3 vec, JsonSerializerOptions options) {
        writer.WriteStartArray();
        writer.WriteNumberValue( vec.x );
        writer.WriteNumberValue( vec.y );
        writer.WriteNumberValue( vec.z );
        writer.WriteEndArray();
    }

    // Lesen aus einem JSON Array [x,y,z]
    public override Vector3 Read(
        ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) {
        try {
            reader.Read();
            if (reader.TokenType == JsonTokenType.StartArray)
            { 
                reader.Read();
            }
            if (reader.TokenType != JsonTokenType.Number) {
                throw new JsonException( "Quat: Expected number." );
            }

            Vector3 vec = new();
            vec.x = reader.GetSingle();
            reader.Read();
            vec.y = reader.GetSingle();
            reader.Read();
            vec.z = reader.GetSingle();

            reader.Read(); // EndArray

            if (reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException( "Vec3: Expected end of array." );

            return vec;
        } catch (Exception e) {
            System.Console.WriteLine( "Error in Json Serializer Vec3JsonConverter" + e );
            System.Console.WriteLine( "TokenType:"                         + reader.TokenType.ToString() );
            System.Console.WriteLine( "Bytes" + reader.BytesConsumed);
            throw e;
        }
    }
}

public class QuatJsonConverter : JsonConverter<Quaternion>
{
    // Schreiben des Vector3 als Array [x,y,z]
    public override void Write(
        Utf8JsonWriter writer, Quaternion quat, JsonSerializerOptions options) {
        writer.WriteStartArray();
        writer.WriteNumberValue( quat.x );
        writer.WriteNumberValue( quat.y );
        writer.WriteNumberValue( quat.z );
        writer.WriteNumberValue( quat.w );
        writer.WriteEndArray();
    }

    // Lesen aus einem JSON Array [x,y,z]
    public override Quaternion Read(
        ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) {
        try {
            reader.Read();
            if (reader.TokenType == JsonTokenType.StartArray)
            { 
                reader.Read();
            }
            if (reader.TokenType != JsonTokenType.Number) {
                throw new JsonException( "Quat: Expected number." );
            }
                

            Quaternion quat = new();
            quat.x = reader.GetSingle();
            reader.Read();
            quat.y = reader.GetSingle();
            reader.Read();
            quat.z = reader.GetSingle();
            reader.Read();
            quat.w = reader.GetSingle();
            reader.Read(); // EndArray

            if (reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException( "Quat: Expected end of array." );

            return quat;
        } catch (Exception e) {
            System.Console.WriteLine( "Error in Json Serializer QuatJsonConverter" + e );
            System.Console.WriteLine( "TokenType:"                         + reader.TokenType.ToString() );
            System.Console.WriteLine( "Bytes" + reader.BytesConsumed);
            throw e;
        }
    }
}
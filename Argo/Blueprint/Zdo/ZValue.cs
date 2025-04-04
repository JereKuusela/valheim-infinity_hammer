using System;
using UnityEngine;
using System.Text.Json.Serialization;
using Argo.DataAnalysis;
using Argo.Zdo;

namespace Argo.Blueprint;



public struct ZValue

{
    [JsonIgnore]         ZDOType  m_type;
    [JsonIgnore]         string m_name;
    [JsonIgnore]         object m_value;
    [JsonIgnore]         bool   m_hashUnknown = false;
    [JsonInclude] public ZDOType  Type  { get => m_type;  set => m_type = value; }
    [JsonInclude] public string Name  { get => m_name;  set => m_name = value; }
    [JsonInclude] public object Value { get => m_value; set => m_value = value; }

    [JsonInclude] public bool UnknownHash { get => m_hashUnknown; }
    public void SetValue(int value_) {
        m_value = value_;
        m_type  = ZDOType.I;
    }
    public void SetValue(long value_) {
        m_value = value_;
        m_type  = ZDOType.L;
    }
    public void SetValue(float value_) {
        m_value = value_;
        m_type  = ZDOType.F;
    }
    public void SetValue(string value_) {
        m_value = value_;
        m_type  = ZDOType.S;
    }
    public void SetValue(Vector3 value_) {
        m_value = value_;
        m_type  = ZDOType.V;
    }
    public void SetValue(Quaternion value_) {
        m_value = value_;
        m_type  = ZDOType.Q;
    }
    public void SetValue(byte[] value_) {
        m_value = value_;
        m_type  = ZDOType.B;
    }

    private ZValue(ZDOType type_, string name_, object value_, bool unknown) {
        m_type        = type_;
        m_name        = name_;
        m_hashUnknown = unknown;
        m_value       = value_;
    }
    public static ZValue Create<T>(string name_, T value, bool unknown) {
        if (value == null) throw new InvalidOperationException();
        if (typeof(T) == typeof(int))
            return new ZValue( ZDOType.I, name_, (int)(object)value, unknown );
        if (typeof(T) == typeof(float))
            return new ZValue( ZDOType.F, name_, (float)(object)value, unknown );
        if (typeof(T) == typeof(Quaternion))
            return new ZValue( ZDOType.Q, name_, (Quaternion)(object)value, unknown );
        if (typeof(T) == typeof(Vector3))
            return new ZValue( ZDOType.V, name_, (Vector3)(object)value, unknown );
        if (typeof(T) == typeof(long))
            return new ZValue( ZDOType.L, name_, (long)(object)value, unknown );
        if (typeof(T) == typeof(string))
            return new ZValue( ZDOType.S, name_, (string)(object)value, unknown );
        if (typeof(T) == typeof(byte[]))
            return new ZValue( ZDOType.B, name_, (byte[])(object)value, unknown );
        throw new ArgumentException( $"Unsupported data type1: {typeof(T)}" );
    }
    public static ZValue Create<T>(int hash, T value_, HashRegister hashRegister) {
        var unknown = !hashRegister.GetNameOrToString( hash, out string name_ );
        if (value_ == null) throw new InvalidOperationException();
        if (typeof(T) == typeof(int))
            return new ZValue( name_, (int)(object)value_, unknown );
        if (typeof(T) == typeof(float))
            return new ZValue( name_, (float)(object)value_, unknown );
        if (typeof(T) == typeof(Quaternion))
            return new ZValue( name_, (Quaternion)(object)value_, unknown );
        if (typeof(T) == typeof(Vector3))
            return new ZValue( name_, (Vector3)(object)value_, unknown );
        if (typeof(T) == typeof(long))
            return new ZValue( name_, (long)(object)value_, unknown );
        if (typeof(T) == typeof(string))
            return new ZValue( name_, (string)(object)value_, unknown );
        if (typeof(T) == typeof(byte[]))
            return new ZValue( name_, (byte[])(object)value_, unknown );
        throw new ArgumentException( $"Unsupported data type1: {typeof(T)}" );
    }
    static T CheckValue<T>(T value) {
        if (value     == null) throw new InvalidOperationException();
        if (typeof(T) == typeof(int)) { return (T)value; }
        if (typeof(T) == typeof(float)) { return (T)value; }
        if (typeof(T) == typeof(Quaternion)) { return (T)value; }
        if (typeof(T) == typeof(Vector3)) { return (T)value; }
        if (typeof(T) == typeof(long)) { return (T)value; }
        if (typeof(T) == typeof(byte[])) { return (T)value; }
        if (typeof(T) == typeof(string)) { return (T)value; }

        throw new ArgumentException( $"Unsupported data type2: {typeof(T)}" );
    }
    public ZValue(string name_, int value_, bool unknown) :
        this( ZDOType.I, name_, value_, unknown ) { }
    public ZValue(string name_, long value_, bool unknown) :
        this( ZDOType.L, name_, value_, unknown ) { }
    public ZValue(string name_, float value_, bool unknown) :
        this( ZDOType.F, name_, value_, unknown ) { }
    public ZValue(string name_, string value_, bool unknown) :
        this( ZDOType.S, name_, value_, unknown ) { }
    public ZValue(string name_, Vector3 value_, bool unknown) :
        this( ZDOType.V, name_, value_, unknown ) { }
    public ZValue(string name_, Quaternion value_, bool unknown) :
        this( ZDOType.Q, name_, value_, unknown ) { }
    public ZValue(string name_, byte[] value_, bool unknown) :
        this( ZDOType.ByteArray, name_, value_, unknown ) { }
}
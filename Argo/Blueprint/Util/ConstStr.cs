using System;

namespace Argo.Blueprint.Util;

/// <summary>
/// Class for Immutatble strings to use in Immutable Dictionaries etc.
/// </summary>
public readonly struct ConstStr : IEquatable<ConstStr>, IComparable<ConstStr>
{
    private readonly string _value;

    public ConstStr(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value => _value;

    public static implicit operator ConstStr(string value) => new(value);

    public static implicit operator string(ConstStr  str) =>  str._value;

    public bool Equals(ConstStr other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object obj)
    {
        return obj is ConstStr other && Equals(other);
    }

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(_value);

    public static bool operator ==(ConstStr left, ConstStr right)
        => left.Equals(right);

    public static bool operator !=(ConstStr left, ConstStr right)
        => !left.Equals(right);

    public int CompareTo(ConstStr other)
        => string.Compare(_value, other._value, StringComparison.OrdinalIgnoreCase);

    public override string ToString() => _value;
}

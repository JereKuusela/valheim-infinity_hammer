using System;

namespace Argo.Zdo;

public enum ZDOType : byte
{
    Float     = (byte)'f',
    Vec3      = (byte)'v',
    Quat      = (byte)'q',
    Int       = (byte)'i',
    Long      = (byte)'l',
    String    = (byte)'s',
    ByteArray = (byte)'b',
    ConnectionsHashData = (byte)'h',
    Connections = (byte)'c',
    Owner = (byte)'o',
    
    F = (byte)'f',
    V = (byte)'v',
    Q = (byte)'q',
    I = (byte)'i',
    L = (byte)'l',
    S = (byte)'s',
    B = (byte)'b',
}

[Flags]
public enum ZDOTypeFlags : ushort
{
    Float               =  1 << 1,
    Vec3                =  1 << 2,
    Quat                =  1 << 3,
    Int                 =  1 << 4,
    Long                =  1 << 5,
    String              =  1 << 6,
    ByteArray           =  1 << 7,
    ConnectionsHashData =  1 << 8,
    Connections         =  1 << 9,
    Owner               =  1 << 10,
}
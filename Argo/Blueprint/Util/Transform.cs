using System.Text.Json.Serialization;
using UnityEngine;

namespace Argo.Blueprint.Util;

public struct Transform
{
   [JsonInclude][JsonPropertyName("p")] public  Vector3 m_Position;
   [JsonInclude][JsonPropertyName("r")] public Quaternion m_Rotation;
   [JsonInclude][JsonPropertyName("s")] public Vector3 m_Scale;
}
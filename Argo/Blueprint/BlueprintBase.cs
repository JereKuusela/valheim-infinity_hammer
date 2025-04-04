using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Argo.Blueprint;

public enum ChanceMode : byte
{
    Multiply, 
    Lowest,
    Highest,
    /// <summary>
    /// rolls chances individually and succeeds if one of them succeeds
    /// </summary>
    Advantage, 
    /// <summary>
    /// rolls chances individually and fails if one of them fails, probably the same as multiply
    /// </summary>
    Disadvantage 
    
}
#if ARGO_STANDALONE
struct BpObjectList
{
    [JsonInclude] public List<BpjObject?>   bpos;
}
#else
struct BpObjectList
{
    [JsonInclude] public List<BpjObject?>   bpos;
    [JsonIgnore]  public List<SelectionObject> gos;
}
#endif
public class BlueprintBase
{
    [JsonInclude]                              public List<BpjObject?>   objects;
    [JsonIgnore]                               public List<SelectionObject> GameObjects; //todo move out of here to selection class?
    [JsonInclude] [JsonPropertyName("chance")] public float              chance;
    [JsonInclude] [JsonPropertyName("chance")] public ChanceMode         chanceMode = ChanceMode.Multiply;


}
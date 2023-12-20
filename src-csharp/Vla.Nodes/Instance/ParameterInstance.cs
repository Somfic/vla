using Newtonsoft.Json;

namespace Vla.Nodes.Instance;

public readonly struct ParameterInstance(string id, object value)
{
    [JsonProperty("id")]
    public string Id { get; init; } = id;
    
    [JsonProperty("defaultValue")]
    public string DefaultValue { get; init; } = JsonConvert.SerializeObject(value);
}
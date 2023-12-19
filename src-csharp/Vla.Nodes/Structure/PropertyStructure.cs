using Newtonsoft.Json;

namespace Vla.Nodes.Structure;

public readonly struct PropertyStructure(string name, Type type, string defaultValue)
{
    [JsonProperty("name")]
    public string Name { get; init; } = name;

    [JsonProperty("type")]
    public Type Type { get; init; } = type;

    [JsonProperty("defaultValue")]
    public string DefaultValue { get; init; } = defaultValue;
}
using Newtonsoft.Json;

namespace Vla.Abstractions.Structure;

public readonly struct ParameterStructure(string id, string name, Type type)
{
    [JsonProperty("id")]
    public string Id { get; init; } = id;

    [JsonProperty("name")]
    public string Name { get; init; } = name;

    [JsonProperty("type")]
    public Type Type { get; init; } = type;

    [JsonProperty("defaultValue")]
    public object? DefaultValue { get; init; } = type.GetDefaultValueForType();
}
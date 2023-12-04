using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Vla.Abstractions.Instance;

public readonly struct PropertyInstance(string name, Type type, string value)
{
    [JsonProperty("name")]
    public string Name { get; init; } = name;

    [JsonProperty("type")]
    public Type Type { get; init; } = type;

    [JsonProperty("value")]
    public string Value { get; init; } = value;
}
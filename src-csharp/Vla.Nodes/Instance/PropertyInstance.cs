using Newtonsoft.Json;

namespace Vla.Nodes.Instance;

public readonly struct PropertyInstance(string name, Type type, string value)
{
    /// <summary>
    /// The name of the property.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; init; } = name;

    /// <summary>
    /// The type of the property.
    /// </summary>
    [JsonProperty("type")]
    public Type Type { get; init; } = type;

    /// <summary>
    /// The JSON encoded value of the property.
    /// </summary>
    [JsonProperty("value")]
    public string Value { get; init; } = value;
}
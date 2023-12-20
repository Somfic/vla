using Newtonsoft.Json;

namespace Vla.Nodes.Instance;

public readonly struct PropertyInstance(string id, Type type, string value)
{
    /// <summary>
    /// The id of the property.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; init; } = id;

    [JsonProperty("type")]
    public Type Type { get; init; } = type;

    [JsonProperty("value")]
    public string Value { get; init; } = value;
}
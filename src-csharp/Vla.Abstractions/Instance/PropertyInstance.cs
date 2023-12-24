using Newtonsoft.Json;

namespace Vla.Abstractions.Instance;

public readonly struct PropertyInstance(string id, Type type, string value)
{
    /// <summary>
    /// The id of the property.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; init; } = id;

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
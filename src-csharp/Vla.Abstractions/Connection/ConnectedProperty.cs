using Newtonsoft.Json;

namespace Vla.Abstractions.Connection;

public readonly struct ConnectedProperty(Guid instanceId, string propertyId)
{
    [JsonProperty("node")]
    public Guid Node { get; init; } = instanceId;

    [JsonProperty("name")]
    public string Name { get; init; } = propertyId;
}
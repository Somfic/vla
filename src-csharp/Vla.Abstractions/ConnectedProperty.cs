using Newtonsoft.Json;

namespace Vla.Abstractions;

public readonly struct ConnectedProperty(Guid instanceId, string propertyId)
{
    [JsonProperty("node")]
    public Guid NodeId { get; init; } = instanceId;

    [JsonProperty("id")]
    public string Id { get; init; } = propertyId;
}
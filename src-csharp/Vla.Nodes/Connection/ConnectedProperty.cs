using Newtonsoft.Json;

namespace Vla.Nodes.Connection;

public readonly struct ConnectedProperty(string instanceId, string propertyId)
{
    [JsonProperty("instanceId")]
    public string InstanceId { get; init; } = instanceId;

    [JsonProperty("propertyId")]
    public string PropertyId { get; init; } = propertyId;
}
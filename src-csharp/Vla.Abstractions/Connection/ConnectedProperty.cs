using Newtonsoft.Json;

namespace Vla.Abstractions.Connection;

public readonly struct ConnectedProperty(Guid instanceId, string propertyId)
{
    [JsonProperty("instanceId")]
    public Guid InstanceId { get; init; } = instanceId;

    [JsonProperty("propertyId")]
    public string PropertyId { get; init; } = propertyId;
    
    public string Id => $"{InstanceId}.{PropertyId}";
}
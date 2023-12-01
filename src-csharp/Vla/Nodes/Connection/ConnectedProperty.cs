using Newtonsoft.Json;

namespace Vla.Nodes.Connection;

public readonly struct ConnectedProperty
{
    public ConnectedProperty(string instanceId, string propertyId)
    {
        InstanceId = instanceId;
        PropertyId = propertyId;
    }

    [JsonProperty("instanceId")]
    public string InstanceId { get; init; }

    [JsonProperty("propertyId")]
    public string PropertyId { get; init; }
}
namespace Vla.Nodes.Connection;

public readonly struct ConnectedProperty
{
    public ConnectedProperty(string instanceId, string propertyId)
    {
        InstanceId = instanceId;
        PropertyId = propertyId;
    }
	
    public string InstanceId { get; init; }

    public string PropertyId { get; init; }
}
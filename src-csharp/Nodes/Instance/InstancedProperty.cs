namespace Vla.Nodes.Instance;

public readonly struct InstancedProperty
{
    public InstancedProperty(string instanceId, string propertyId)
    {
        InstanceId = instanceId;
        PropertyId = propertyId;
    }
	
    public string InstanceId { get; init; }

    public string PropertyId { get; init; }
}
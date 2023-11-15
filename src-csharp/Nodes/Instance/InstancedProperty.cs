namespace Vla.Nodes.Instance;

public readonly struct InstancedProperty
{
    public InstancedProperty(Guid instanceId, string propertyId)
    {
        InstanceId = instanceId;
        PropertyId = propertyId;
    }
	
    public Guid InstanceId { get; init; }

    public string PropertyId { get; init; }
}
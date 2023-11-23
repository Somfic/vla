namespace Vla.Nodes.Web.Result;

public readonly struct InstanceValue
{
	public InstanceValue(string id, object? value)
	{
		Id = id;
		Value = value;
	}

	public string Id { get; init; }
    
	public object? Value { get; init; }
}
using System.Collections.Immutable;

namespace Vla.Nodes.Instance;

public readonly struct NodeInstance
{
	public NodeInstance()
	{
	}

	public Guid Id { get; init; } = Guid.NewGuid();
	
	public Type Type { get; init; } = null;
	
	public ImmutableArray<Property> Properties { get; init; } = ImmutableArray<Property>.Empty;
	
	public Metadata Metadata { get; init; } = new();
}
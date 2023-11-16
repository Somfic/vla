using System.Collections.Immutable;

namespace Vla.Nodes.Instance;

public readonly struct NodeInstance
{
	public NodeInstance()
	{
	}

	public string Id { get; init; }
	
	public Type Type { get; init; }
	
	public ImmutableArray<Property> Properties { get; init; }
	
	public Metadata Metadata { get; init; }
}
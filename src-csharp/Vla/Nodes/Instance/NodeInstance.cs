using System.Collections.Immutable;

namespace Vla.Nodes.Instance;

public readonly struct NodeInstance
{
	public string Id { get; init; }
	
	public Type NodeType { get; init; }
	
	public ImmutableArray<PropertyInstance> Properties { get; init; }
	
	public Metadata Metadata { get; init; }
}
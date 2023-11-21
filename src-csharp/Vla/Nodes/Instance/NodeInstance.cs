using System;
using System.Collections.Immutable;

namespace Vla.Nodes.Instance;

public readonly struct NodeInstance
{
	public NodeInstance()
	{
		Id = Guid.NewGuid().ToString();
	}

	public string Id { get; init; }

	public Type NodeType { get; init; } = typeof(object);

	public ImmutableArray<PropertyInstance> Properties { get; init; } = ImmutableArray<PropertyInstance>.Empty;

	public Metadata Metadata { get; init; } = new();
}	
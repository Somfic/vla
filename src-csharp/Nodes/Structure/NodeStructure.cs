using System.Collections.Immutable;

namespace Vla.Nodes.Structure;

public readonly struct NodeStructure
{
	public NodeStructure()
	{
	}

	public Type Type { get; init; } = null;
	
	public ImmutableArray<PropertyStructure> Properties { get; init; } = ImmutableArray<PropertyStructure>.Empty;

	public ImmutableArray<ParameterStructure> Inputs { get; init; } = ImmutableArray<ParameterStructure>.Empty;
	
	public ImmutableArray<ParameterStructure> Outputs { get; init; } = ImmutableArray<ParameterStructure>.Empty;
	
	public string ExecuteMethod { get; init; } = null;
}
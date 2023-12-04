using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Vla.Abstractions.Structure;

public readonly struct NodeStructure
{
	public NodeStructure()
	{
	}

	[JsonProperty("nodeType")]
	public Type NodeType { get; init; } = typeof(object);

	[JsonProperty("properties")]
	public ImmutableArray<PropertyStructure> Properties { get; init; } = ImmutableArray<PropertyStructure>.Empty;

	[JsonProperty("inputs")]
	public ImmutableArray<ParameterStructure> Inputs { get; init; } = ImmutableArray<ParameterStructure>.Empty;

	[JsonProperty("outputs")]
	public ImmutableArray<ParameterStructure> Outputs { get; init; } = ImmutableArray<ParameterStructure>.Empty;

	[JsonProperty("executeMethod")]
	public string ExecuteMethod { get; init; } = string.Empty;
}
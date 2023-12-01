using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Vla.Nodes.Instance;

public readonly struct NodeInstance()
{
	[JsonProperty("id")]
	public string Id { get; init; } = Guid.NewGuid().ToString();

	[JsonProperty("nodeType")]
	public Type NodeType { get; init; } = typeof(object);

	[JsonProperty("properties")]
	public ImmutableArray<PropertyInstance> Properties { get; init; } = ImmutableArray<PropertyInstance>.Empty;

	[JsonProperty("metadata")]
	public Metadata Metadata { get; init; } = new();
}
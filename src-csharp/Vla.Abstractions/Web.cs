using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Vla.Abstractions;

public readonly record struct Web(string Name)
{
	[JsonProperty("name")]
	public string Name { get; init; } = Name;

	[JsonProperty("instances")]
	public ImmutableArray<NodeInstance> Instances { get; init; }

	[JsonProperty("connections")]
	public ImmutableArray<NodeConnection> Connections { get; init; }
}
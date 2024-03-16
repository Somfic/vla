using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Vla.Abstractions;

public readonly record struct Web
{
	public Web(string name)
	{
		Name = name;
		Instances = ImmutableArray<NodeInstance>.Empty;
		Connections = ImmutableArray<NodeConnection>.Empty;
	}
	
	[JsonProperty("name")]
	public string Name { get; init; }

	[JsonProperty("instances")]
	public ImmutableArray<NodeInstance> Instances { get; init; }

	[JsonProperty("connections")]
	public ImmutableArray<NodeConnection> Connections { get; init; }
	
}
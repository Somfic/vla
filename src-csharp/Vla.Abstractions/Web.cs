using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Vla.Abstractions;

public readonly record struct Web
{
	public Web(string workspacePath, string name)
	{
		Name = name;
		WorkspacePath = workspacePath;
		Instances = ImmutableArray<NodeInstance>.Empty;
		Connections = ImmutableArray<NodeConnection>.Empty;
	}
	
	[JsonProperty("workspacePath")]
	public string WorkspacePath { get; init; }
	
	[JsonProperty("name")]
	public string Name { get; init; }

	[JsonProperty("instances")]
	public ImmutableArray<NodeInstance> Instances { get; init; }

	[JsonProperty("connections")]
	public ImmutableArray<NodeConnection> Connections { get; init; }
	
}
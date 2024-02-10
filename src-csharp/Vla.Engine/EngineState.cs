using System.Collections.Immutable;
using Newtonsoft.Json;
using Vla.Abstractions.Connection;

namespace Vla.Engine;

public record EngineState
{
	[JsonProperty("instances")]
	public ImmutableArray<NodeInstance> Instances { get; init; }
	
	[JsonProperty("connections")]
	public ImmutableArray<NodeConnection> Connections { get; init; }
}
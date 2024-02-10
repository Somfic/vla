using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Vla.Engine;

public record NodeInstance
{

	[JsonProperty("node")]
	public Type Type { get; init; } = null!;

	[JsonProperty("id")] 
	public Guid? Guid { get; init; } = null;
	
	[JsonProperty("properties")]
	public ImmutableDictionary<string, dynamic?> Properties { get; init; } = ImmutableDictionary<string, dynamic?>.Empty;
	
	[JsonProperty("inputs")]
	public ImmutableDictionary<string, dynamic?> Inputs { get; init; } = ImmutableDictionary<string, dynamic?>.Empty;
	
	[JsonProperty("outputs")]
	public ImmutableDictionary<string, dynamic?> Outputs { get; init; } = ImmutableDictionary<string, dynamic?>.Empty;
}
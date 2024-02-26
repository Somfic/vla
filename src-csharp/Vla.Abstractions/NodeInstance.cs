using System.Collections.Immutable;
using Newtonsoft.Json;
using Vla.Addon;

namespace Vla.Abstractions;

public record struct NodeInstance()
{
	[JsonProperty("node")]
	public Type Type { get; init; } 

	[JsonProperty("id")] 
	public Guid? Guid { get; init; }
	
	[JsonProperty("properties")]
	public ImmutableDictionary<string, dynamic?> Properties { get; init; } 
	
	[JsonProperty("inputs")]
	public ImmutableDictionary<string, dynamic?> Inputs { get; init; }
	
	[JsonProperty("outputs")]
	public ImmutableDictionary<string, dynamic?> Outputs { get; init; }

	public static implicit operator NodeInstance(Node node) => new(node);
	private NodeInstance(Node node) : this()
	{
		Type = node.GetType();
		Guid = node.Id;
		Properties = node.Properties;
		Inputs = node.Inputs;
		Outputs = node.Outputs;
	}
}

public record NodeInstanceOptions()
{
	[JsonProperty("node")]
	public Type Type { get; init; } = null!;
	
	[JsonProperty("id")]
	public Guid? Id { get; init; } = null;
	
	[JsonProperty("properties")]
	public ImmutableDictionary<string, dynamic?>? Properties { get; init; } = ImmutableDictionary<string, dynamic?>.Empty; 
	
	[JsonProperty("inputs")]
	public ImmutableDictionary<string, dynamic?>? Inputs { get; init; } = ImmutableDictionary<string, dynamic?>.Empty;
	
	[JsonProperty("outputs")]
	public ImmutableDictionary<string, dynamic?>? Outputs { get; init; } = ImmutableDictionary<string, dynamic?>.Empty;

	public static implicit operator NodeInstanceOptions(NodeInstance instance) => new(instance);
	private NodeInstanceOptions(NodeInstance instance) : this()
	{
		Type = instance.Type;
		Id = instance.Guid;
		Properties = instance.Properties;
		Inputs = instance.Inputs;
		Outputs = instance.Outputs;
	}
}
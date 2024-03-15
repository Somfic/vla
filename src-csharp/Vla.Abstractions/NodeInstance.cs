using System.Collections.Immutable;
using Newtonsoft.Json;
using Vla.Addon;

namespace Vla.Abstractions;

public record struct NodeInstance()
{
	private NodeInstance(Node node) : this()
	{
		Type = node.GetType();
		Guid = node.Id;
		Properties = node.Properties.Select(x => new NamedValue(x.Key, x.Key, x.Value)).ToImmutableArray();
		Inputs = node.Inputs.Select(x =>
				new NamedValue(x.Key, node.InputLabels.FirstOrDefault(y => y.Key == x.Key).Value ?? x.Key, x.Value))
			.ToImmutableArray();
		Outputs = node.Outputs.Select(x =>
				new NamedValue(x.Key, node.OutputLabels.FirstOrDefault(y => y.Key == x.Key).Value ?? x.Key, x.Value))
			.ToImmutableArray();
	}

	[JsonProperty("node")]
	public Type Type { get; init; }

	[JsonProperty("id")]
	public Guid? Guid { get; init; } = null;

	[JsonProperty("properties")]
	public ImmutableArray<NamedValue> Properties { get; init; } = ImmutableArray<NamedValue>.Empty;

	[JsonProperty("inputs")]
	public ImmutableArray<NamedValue> Inputs { get; init; } = ImmutableArray<NamedValue>.Empty;

	[JsonProperty("outputs")]
	public ImmutableArray<NamedValue> Outputs { get; init; } = ImmutableArray<NamedValue>.Empty;

	public static implicit operator NodeInstance(Node node)
	{
		return new NodeInstance(node);
	}
}

public record struct NamedValue
{
	public NamedValue(string id, string label, dynamic? value)
	{
		Id = id;
		Label = label;
		Value = value;
	}

	[JsonProperty("id")]
	public string Id { get; init; }

	[JsonProperty("label")]
	public string Label { get; init; }

	[JsonProperty("value")]
	public dynamic? Value { get; init; }
}
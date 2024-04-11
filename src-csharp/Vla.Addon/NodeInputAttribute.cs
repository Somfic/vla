using Newtonsoft.Json;

namespace Vla.Addon;

public readonly struct NodeInput(string id, string label, dynamic? value) : INodeValue
{
	[JsonProperty("id")]
	public string Id { get; init; } = id;

	[JsonProperty("label")]
	public string Label { get; init; } = label;

	[JsonProperty("value")]
	public dynamic? Value { get; init; } = value;
}

public readonly struct NodeOutput(string id, string label, dynamic? value) : INodeValue
{
	[JsonProperty("id")]
	public string Id { get; init; } = id;

	[JsonProperty("label")]
	public string Label { get; init; } = label;

	[JsonProperty("value")]
	public dynamic? Value { get; init; } = value;
}

public interface INodeValue
{
	[JsonProperty("id")]
	public string Id { get; }

	[JsonProperty("label")]
	public string Label { get; }

	[JsonProperty("value")]
	public dynamic? Value { get; }
}
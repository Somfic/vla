using Newtonsoft.Json;

namespace Vla.Nodes.Structure;

public readonly struct OutputParameterStructure(string id, string name, Type type) : IParameterStructure
{
	[JsonProperty("id")]
	public string Id { get; init; } = id;

	[JsonProperty("name")]
	public string Name { get; init; } = name;

	[JsonProperty("type")]
	public Type Type { get; init; } = type;
}
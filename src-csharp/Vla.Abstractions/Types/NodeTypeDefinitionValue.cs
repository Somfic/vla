using Newtonsoft.Json;

namespace Vla.Abstractions.Types;

public readonly struct NodeTypeDefinitionValue(string name, object? value)
{
	[JsonProperty("name")]
	public string Name { get; init; } = name;

	[JsonProperty("value")]
	public object? Value { get; init; } = value;
}
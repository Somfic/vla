using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Vla.Abstractions;

public record struct TypeDefinition
{
	[JsonProperty("type")]
	public Type Type { get; init; }
    
	[JsonProperty("name")]
	public string Name { get; init; }
    
	[JsonProperty("description")]
	public string? Description { get; init; }
    
	[JsonProperty("possibleValues")]
	public ImmutableArray<PossibleValue> PossibleValues { get; init; }
    
	public readonly struct PossibleValue(string label, dynamic? value)
	{
		[JsonProperty("label")]
		public string Label { get; init; } = label;

		[JsonProperty("value")]
		public dynamic? Value { get; init; } = value;
	}
}
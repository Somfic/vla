using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Vla.Abstractions;

public record struct TypeDefinition
{
	public TypeDefinition(System.Type type, HtmlType htmlType, params LabeledValue[] possibleValue)
	{
		Type = type;
		Html = htmlType;
		PossibleValues = possibleValue.ToImmutableArray();
	}
	
	
	[JsonProperty("type")]
	public System.Type Type { get; init; }
	
	[JsonProperty("htmlType")]
	public HtmlType Html { get; init; }
	
	[JsonProperty("possibleValues")]
	public ImmutableArray<LabeledValue> PossibleValues { get; init; }
	
	public enum HtmlType
	{
		Text,
		Number,
		Range,
		Checkbox,
		Select,
	}
	
	public record struct LabeledValue
	{
		public LabeledValue(dynamic value, string label)
		{
			Value = value;
			Label = label;
		}

		[JsonProperty("value")]
		public dynamic Value { get; init; }
		
		[JsonProperty("label")]
		public string Label { get; init; }

		public readonly void Deconstruct(out dynamic Value, out string Label)
		{
			Value = this.Value;
			Label = this.Label;
		}
	}
}
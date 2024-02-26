using System.Collections.Immutable;
using Newtonsoft.Json;
using Vla.Addon;

namespace Vla.Abstractions;

public record struct NodeStructure
{
	[JsonProperty("name")]
	public string Name { get; init; }
    
	[JsonProperty("category")]
	public string? Category { get; init; }
    
	[JsonProperty("description")]
	public string? Description { get; init; }
    
	[JsonProperty("searchTerms")]
	public ImmutableArray<string> SearchTerms { get; init; }
    
	[JsonProperty("purity")]
	public NodePurity Purity { get; init; }
    
	[JsonProperty("properties")]
	public ImmutableArray<Property> Properties { get; init; }
    
	public record struct Property
	{
		[JsonProperty("name")]
		public string Name { get; init; }
        
		[JsonProperty("type")]
		public Type Type { get; init; }
        
		[JsonProperty("description")]
		public string? Description { get; init; }
	}
}
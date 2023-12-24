using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Vla.Abstractions.Structure;

public readonly struct NodeStructure
{
    public NodeStructure()
    {
    }

    [JsonProperty("nodeType")]
    public Type NodeType { get; init; } = typeof(object);

    [JsonProperty("name")]
    public string Name { get; init; } = string.Empty;
    
    [JsonProperty("description")]
    public string Description { get; init; } = string.Empty;

    [JsonProperty("category")]
    public string? Category { get; init; } = string.Empty;

    [JsonProperty("searchTerms")]
    public ImmutableArray<string> SearchTerms { get; init; } = ImmutableArray<string>.Empty;

    [JsonProperty("properties")]
    public ImmutableArray<PropertyStructure> Properties { get; init; } = ImmutableArray<PropertyStructure>.Empty;

    [JsonProperty("inputs")]
    public ImmutableArray<InputParameterStructure> Inputs { get; init; } = ImmutableArray<InputParameterStructure>.Empty;

    [JsonProperty("outputs")]
    public ImmutableArray<OutputParameterStructure> Outputs { get; init; } = ImmutableArray<OutputParameterStructure>.Empty;

    [JsonProperty("executeMethod")]
    public string ExecuteMethod { get; init; } = string.Empty;
}
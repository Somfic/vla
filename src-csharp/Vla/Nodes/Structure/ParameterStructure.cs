using Newtonsoft.Json;

namespace Vla.Nodes.Structure;

public readonly struct ParameterStructure
{
    public ParameterStructure(string id, string name, Type type)
    {
        Id = id;
        Name = name;
        Type = type;
    }

    [JsonProperty("id")]
    public string Id { get; init; }

    [JsonProperty("name")]
    public string Name { get; init; }

    [JsonProperty("type")]
    public Type Type { get; init; }
}
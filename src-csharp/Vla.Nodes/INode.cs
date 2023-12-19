using Newtonsoft.Json;

namespace Vla.Nodes;

public interface INode
{
    [JsonProperty("name")]
    public string Name { get; }
}
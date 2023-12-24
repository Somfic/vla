using Newtonsoft.Json;

namespace Vla.Addon;

public interface INode
{
    [JsonProperty("name")]
    public string Name { get; }
}
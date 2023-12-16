using Newtonsoft.Json;

namespace Vla.Abstractions;

public interface INode
{
	[JsonProperty("name")]
	public string Name { get; }
}
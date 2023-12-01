using Newtonsoft.Json;
using Vla.Nodes.Connection;
using Vla.Nodes.Instance;

namespace Vla.Nodes.Web;

public readonly struct Web
{
	[JsonProperty("instances")]
	public NodeInstance[] Instances { get; init; }
	
	[JsonProperty("connections")]
	public NodeConnection[] Connections { get; init; }
}
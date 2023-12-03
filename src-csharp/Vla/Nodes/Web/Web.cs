using Newtonsoft.Json;
using Vla.Nodes.Connection;
using Vla.Nodes.Instance;

namespace Vla.Nodes.Web;

public readonly struct Web
{
	public Web()
	{
	}

	[JsonProperty("instances")]
	public NodeInstance[] Instances { get; init; } = Array.Empty<NodeInstance>();
	
	[JsonProperty("connections")]
	public NodeConnection[] Connections { get; init; } = Array.Empty<NodeConnection>();
}
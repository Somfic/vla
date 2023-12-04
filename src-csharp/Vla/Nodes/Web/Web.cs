using Newtonsoft.Json;
using Vla.Abstractions.Connection;
using Vla.Abstractions.Instance;

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
using Vla.Nodes.Connection;
using Vla.Nodes.Instance;

namespace Vla.Nodes.Web;

public readonly struct Web
{
	public NodeInstance[] Instances { get; init; }
	
	public NodeConnection[] Connections { get; init; }
}
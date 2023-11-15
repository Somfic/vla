using Vla.Nodes.Instance;

namespace Vla.Nodes.Connection;

public static class NodeConnectionExtensions
{
    public static NodeConnection From(this NodeConnection connection, NodeInstance node, string outputId)
    {
        return connection with { From = new InstancedProperty(node.Id, outputId) };
    }
	
    public static NodeConnection To(this NodeConnection connection, NodeInstance node, string inputId)
    {
        return connection with { To = new InstancedProperty(node.Id, inputId) };
    }
}
using Vla.Nodes.Instance;

namespace Vla.Nodes.Connection;

public static class NodeConnectionExtensions
{
    public static NodeConnection WithSource(this NodeConnection connection, NodeInstance node, string outputId)
    {
        return connection with { Source = new ConnectedProperty(node.Id, outputId) };
    }

    public static NodeConnection WithTarget(this NodeConnection connection, NodeInstance node, string inputId)
    {
        return connection with { Target = new ConnectedProperty(node.Id, inputId) };
    }
}
using Vla.Nodes.Structure;

namespace Vla.Nodes.Instance;

public static class NodeInstanceBuilderExtensions
{
    public static NodeInstance From(this NodeInstance node, NodeStructure structure)
    {
        return node with { NodeType = structure.NodeType };
    }
    
    public static NodeInstance From<TNode>(this NodeInstance node) where TNode : INode
    {
        return node with { NodeType = typeof(TNode) };
    }
	
    public static NodeInstance WithProperty(this NodeInstance node, string name, string value)
    {
        return node with { Properties = node.Properties.Add(new PropertyInstance(name, value)) };
    }
}
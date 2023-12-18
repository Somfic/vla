using Newtonsoft.Json;
using Vla.Abstractions.Structure;

namespace Vla.Abstractions.Instance;

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

    public static NodeInstance WithProperty<T>(this NodeInstance node, string name, T value)
    {
        return node with { Properties = node.Properties.Add(new PropertyInstance(name, typeof(T), JsonConvert.SerializeObject(value))) };
    }

    public static NodeInstance WithPosition(this NodeInstance node, int x, int y)
    {
        return node with { Metadata = node.Metadata with { Position = new Position { X = x, Y = y } } };
    }
}
using System.Collections.Immutable;
using Newtonsoft.Json;
using Vla.Nodes.Structure;

namespace Vla.Nodes.Instance;

public static class NodeInstanceBuilderExtensions
{
    public static NodeInstance From(this NodeInstance node, NodeStructure structure)
    {
        node = node with { NodeType = structure.NodeType };
        node = node with { Properties = structure.Properties.Select(p => new PropertyInstance(p.Name, p.Type, p.DefaultValue)).ToImmutableArray() };
        node = node with { Inputs = structure.Inputs.Select(p => new ParameterInstance(p.Name, p.Type)).ToImmutableArray() };
        return node;
    }

    public static NodeInstance From<TNode>(this NodeInstance node) where TNode : INode
    {
        return node with { NodeType = typeof(TNode) };
    }

    public static NodeInstance WithProperty<T>(this NodeInstance node, string name, T value)
    {
        // Replace existing property if it exists
        if (node.Properties.Any(p => p.Name == name))
        {
            return node with { Properties = node.Properties.Replace(node.Properties.First(p => p.Name == name), new PropertyInstance(name, typeof(T), JsonConvert.SerializeObject(value))) };
        }

        return node with
        {
            Properties =
            node.Properties.Add(new PropertyInstance(name, typeof(T), JsonConvert.SerializeObject(value)))
        };
    }

    public static NodeInstance WithPosition(this NodeInstance node, int x, int y)
    {
        return node with { Metadata = node.Metadata with { Position = new Position { X = x, Y = y } } };
    }
}
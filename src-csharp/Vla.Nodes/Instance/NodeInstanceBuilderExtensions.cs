using System.Collections.Immutable;
using Newtonsoft.Json;
using Vla.Nodes.Structure;

namespace Vla.Nodes.Instance;

public static class NodeInstanceBuilderExtensions
{
    public static NodeInstance From(this NodeInstance node, NodeStructure structure)
    {
        node = node with
        {
            NodeType = structure.NodeType,
            Properties = structure.Properties
                .Select(p => new PropertyInstance(p.Id, p.Type, p.DefaultValue))
                .ToImmutableArray()
        };
        return node;
    }

    public static NodeInstance WithProperty<T>(this NodeInstance node, string id, T value)
    {
        if (node.Properties.Any(p => p.Id == id))
        {
            return node with
            {
                Properties = node.Properties.Replace(node.Properties.First(p => p.Id == id),
                    new PropertyInstance(id, typeof(T), JsonConvert.SerializeObject(value)))
            };
        }

        return node with
        {
            Properties =
            node.Properties.Add(new PropertyInstance(id, typeof(T), JsonConvert.SerializeObject(value)))
        };
    }
    
    public static NodeInstance WithInput(this NodeInstance node, string id, object value)
    {
        if (node.Inputs.Any(p => p.Id == id))
        {
            return node with
            {
                Inputs = node.Inputs.Replace(node.Inputs.First(p => p.Id == id),
                    new ParameterInstance(id, value))
            };
        }

        return node with
        {
            Inputs = node.Inputs.Add(new ParameterInstance(id, value))
        };
    }

    public static NodeInstance WithPosition(this NodeInstance node, int x, int y)
    {
        return node with { Metadata = node.Metadata with { Position = new Position { X = x, Y = y } } };
    }
}
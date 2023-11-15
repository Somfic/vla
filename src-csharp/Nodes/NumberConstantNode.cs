using Vla.Nodes.Attributes;

namespace Vla.Nodes;

[Node]
public class NumberConstantNode : INode
{
    [NodeProperty]
    public double Value { get; set; }

    public void Execute([NodeOutput("Value")] out double value)
    {
        value = Value;
    }
}
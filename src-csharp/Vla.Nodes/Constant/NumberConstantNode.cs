using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Nodes.Constant;

[Node("Number constant")]
[NodeCategory("Constant")]
[NodeTags("Number", "Constant", "Value", "Double", "Float", "Decimal", "Integer")]
public class NumberConstantNode : INode
{
    public string Name => "Number constant";

    [NodeProperty]
    public double Value { get; set; }

    public void Execute([NodeOutput("Value")] out double value)
    {
        value = Value;
    }
}
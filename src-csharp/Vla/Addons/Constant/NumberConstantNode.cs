namespace Vla.Addon.Core.Constant;

[Node(Purity.Deterministic)]
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
namespace Vla.Addon.Core.Math;

[Node]
[NodeCategory("Math")]
[NodeTags("Exponent", "Exp", "e^", "e", "Exponential")]
public class ExponentMathNode : INode
{
    public string Name => "Exponent";

    public void Execute([NodeInput("Value")] double value, [NodeOutput("Result")] out double result)
    {
        result = System.Math.Exp(value);
    }
}
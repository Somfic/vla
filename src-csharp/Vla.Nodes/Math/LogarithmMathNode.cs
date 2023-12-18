using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Nodes.Math;

[Node("Logarithm")]
[NodeCategory("Math")]
[NodeTags("Logarithm", "Log", "Ln", "Logarithmic")]
public class LogarithmMathNode : INode
{
    public string Name => Mode.GetValueName();

    [NodeProperty]
    public MathMode Mode { get; set; } = MathMode.Logarithm;

    public void Execute([NodeInput("Value")] double value, [NodeOutput("Result")] out double result)
    {
        result = Mode switch
        {
            MathMode.Logarithm => System.Math.Log10(value),
            MathMode.NaturalLogarithm => System.Math.Log(value),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public enum MathMode
    {
        [NodeEnumValue("Logarithm")]
        Logarithm,

        [NodeEnumValue("Natural logarithm")]
        NaturalLogarithm
    }
}
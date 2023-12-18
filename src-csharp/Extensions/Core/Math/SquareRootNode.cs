using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Extensions.Core.Math;

[Node("Square root")]
[NodeCategory("Math")]
[NodeTags("Square root", "Inverse square root", "Sqrt", "InvSqrt", "√", "1/√")]
public class SquareRootMathNode : INode
{
    public string Name => Mode.GetValueName();

    [NodeProperty]
    public MathMode Mode { get; set; } = MathMode.SquareRoot;

    public void Execute([NodeInput("Value")] double value, [NodeOutput("Result")] out double result)
    {
        result = Mode switch
        {
            MathMode.SquareRoot => System.Math.Sqrt(value),
            MathMode.InverseSquareRoot => 1 / System.Math.Sqrt(value),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public enum MathMode
    {
        [NodeEnumValue("Square root")]
        SquareRoot,

        [NodeEnumValue("Inverse square root")]
        InverseSquareRoot,
    }
}
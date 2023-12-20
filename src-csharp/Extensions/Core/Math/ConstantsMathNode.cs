using Vla.Abstractions;
using Vla.Nodes;
using Vla.Nodes.Attributes;

namespace Vla.Extensions.Core.Math;

[Node("Math constants")]
[NodeCategory("Math")]
[NodeTags("Math", "Constants", "Pi", "E", "Tau", "π", "τ")]
public class ConstantsMathNode : INode
{
    public string Name => Mode.GetValueName();

    [NodeProperty]
    public MathMode Mode { get; set; } = MathMode.Pi;

    public void Execute([NodeOutput("Constant")] out double value)
    {
        value = Mode switch
        {
            MathMode.Pi => System.Math.PI,
            MathMode.E => System.Math.E,
            MathMode.Tau => System.Math.Tau,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public enum MathMode
    {
        [NodeEnumValue("π")]
        Pi,
        [NodeEnumValue("e")]
        E,
        [NodeEnumValue("τ")]
        Tau
    }
}
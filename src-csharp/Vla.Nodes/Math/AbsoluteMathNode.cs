using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Nodes.Math;

[Node("Absolute")]
[NodeCategory("Math")]
[NodeTags("Absolute", "Abs", "Magnitude")]
public class AbsoluteMathNode : INode
{
    public string Name => "Absolute";

    public void Execute([NodeInput("Value")] double value, [NodeOutput("Result")] out double result)
    {
        result = System.Math.Abs(value);
    }
}
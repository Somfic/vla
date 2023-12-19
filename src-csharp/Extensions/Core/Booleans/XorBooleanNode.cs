using Vla.Abstractions;
using Vla.Abstractions.Attributes;
using Vla.Nodes;

namespace Vla.Extensions.Core.Booleans;

[Node("XOR operator")]
[NodeCategory("Booleans")]
[NodeTags("Xor", "Operator", "^", "Logic", "Gate")]
public class XorBooleanNode : INode
{
    public string Name => "XOR";

    public void Execute([NodeInput("Left")] bool left, [NodeInput("Right")] bool right, [NodeOutput("Result")] out bool result)
    {
        result = left ^ right;
    }
}
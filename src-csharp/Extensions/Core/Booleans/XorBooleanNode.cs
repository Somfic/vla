using Vla.Abstractions;
using Vla.Nodes;
using Vla.Nodes.Attributes;

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
using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Extensions.Core.Booleans;

[Node("XNOR operator")]
[NodeCategory("Booleans")]
[NodeTags("Xnor", "Operator", "!^", "Logic", "Gate")]
public class XnorBooleanNode : INode
{
    public string Name => "XNOR";

    public void Execute([NodeInput("Left")] bool left, [NodeInput("Right")] bool right, [NodeOutput("Result")] out bool result)
    {
        result = !(left ^ right);
    }
}
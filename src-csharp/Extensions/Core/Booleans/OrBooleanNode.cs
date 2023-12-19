using Vla.Abstractions;
using Vla.Abstractions.Attributes;
using Vla.Nodes;

namespace Vla.Extensions.Core.Booleans;

[Node("OR operator")]
[NodeCategory("Booleans")]
[NodeTags("Or", "Operator", "||", "Logic", "Gate")]
public class OrBooleanNode : INode
{
    public string Name => "OR";

    public void Execute([NodeInput("Left")] bool left, [NodeInput("Right")] bool right, [NodeOutput("Result")] out bool result)
    {
        result = left || right;
    }
}
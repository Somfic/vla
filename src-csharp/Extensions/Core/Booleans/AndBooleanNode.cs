using Vla.Abstractions;
using Vla.Nodes;
using Vla.Nodes.Attributes;

namespace Vla.Extensions.Core.Booleans;

[Node("AND operator")]
[NodeCategory("Booleans")]
[NodeTags("And", "Operator", "&&", "Logic", "Gate")]
public class AndBooleanNode : INode
{
    public string Name => "AND";

    public void Execute([NodeInput("Left")] bool left, [NodeInput("Right")] bool right, [NodeOutput("Result")] out bool result)
    {
        result = left && right;
    }
}
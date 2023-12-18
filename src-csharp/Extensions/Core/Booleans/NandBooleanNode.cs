using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Extensions.Core.Booleans;

[Node("NAND operator")]
[NodeCategory("Booleans")]
[NodeTags("Nand", "Operator", "!&", "Logic", "Gate")]
public class NandBooleanNode : INode
{
    public string Name => "NAND";

    public void Execute([NodeInput("Left")] bool left, [NodeInput("Right")] bool right, [NodeOutput("Result")] out bool result)
    {
        result = !(left && right);
    }
}
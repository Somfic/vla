namespace Vla.Addon.Core.Booleans;

[Node]
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
namespace Vla.Addon.Core.Booleans;

[Node]
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
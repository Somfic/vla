namespace Vla.Addon.Core.Booleans;

[Node]
[NodeCategory("Booleans")]
[NodeTags("Nor", "Operator", "!|", "Logic", "Gate")]
public class NorBooleanNode : INode
{
    public string Name => "NOR";

    public void Execute([NodeInput("Left")] bool left, [NodeInput("Right")] bool right, [NodeOutput("Result")] out bool result)
    {
        result = !(left || right);
    }
}
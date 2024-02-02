namespace Vla.Addon.Core.Booleans;

[Node]
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
namespace Vla.Addon.Core.Constant;

[Node(Purity.Deterministic)]
[NodeCategory("Constant")]
[NodeTags("Boolean", "Constant", "Value", "True", "False")]
public class BooleanConstantNode : INode
{
    public string Name => "Boolean constant";

    [NodeProperty]
    public bool Value { get; set; }

    public void Execute([NodeOutput("Value")] out bool value)
    {
        value = Value;
    }
}
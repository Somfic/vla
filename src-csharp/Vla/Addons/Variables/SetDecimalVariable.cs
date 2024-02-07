using Vla.Addon.Services;

namespace Vla.Addon.Core.Variables;

[Node(Purity.Deterministic)]
[NodeCategory("Variables")]
[NodeTags("Set", "Decimal", "Float", "Double", "Save")]
public class SetDecimalVariable(IVariableManager variableManager) : INode
{
    public string Name => "Set decimal variable";

    public void Execute([NodeInput("Variable")] string variable, [NodeInput("Value")] double value)
    {
        variableManager.SetVariable(variable, value);
    }
}
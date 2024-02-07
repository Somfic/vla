using Vla.Addon.Services;

namespace Vla.Addon.Core.Variables;

[Node(Purity.Deterministic)]
[NodeCategory("Variables")]
[NodeTags("Set", "Int", "Integer", "Save")]
public class SetIntVariable(IVariableManager variableManager) : INode
{
    public string Name => "Set int variable";

    public void Execute([NodeInput("Variable")] string variable, [NodeInput("Value")] int value)
    {
        variableManager.SetVariable(variable, value);
    }
}
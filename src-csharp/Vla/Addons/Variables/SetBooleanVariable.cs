using Vla.Addon.Services;

namespace Vla.Addon.Core.Variables;

[Node]
[NodeCategory("Variables")]
[NodeTags("Set", "Boolean", "Bool", "Save")]
public class SetBooleanVariable(IVariableManager variableManager) : INode
{
    public string Name => "Set boolean variable";

    public void Execute([NodeInput("Variable")] string variable, [NodeInput("Value")] bool value)
    {
        variableManager.SetVariable(variable, value);
    }
}
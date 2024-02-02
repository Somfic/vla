using Vla.Addon.Services;

namespace Vla.Addon.Core.Variables;

[Node]
[NodeCategory("Variables")]
[NodeTags("Set", "String", "Save")]
public class SetStringVariable(IVariableManager variableManager) : INode
{
    public string Name => "Set string variable";

    public void Execute([NodeInput("Variable")] string variable, [NodeInput("Value")] string value)
    {
        variableManager.SetVariable(variable, value);
    }
}
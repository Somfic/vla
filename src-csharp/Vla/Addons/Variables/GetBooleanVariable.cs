using Vla.Addon.Services;

namespace Vla.Addon.Core.Variables;

[Node]
[NodeCategory("Variables")]
[NodeTags("Get", "Boolean", "Bool", "Load")]
public class GetBooleanVariable(IVariableManager variableManager) : INode
{
    public string Name => "Get boolean variable";

    public void Execute([NodeInput("Variable")] string variable, [NodeOutput("Value")] out bool value)
    {
        value = variableManager.GetVariable<bool>(variable);
    }
}
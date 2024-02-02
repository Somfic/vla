using Vla.Addon.Services;

namespace Vla.Addon.Core.Variables;

[Node]
[NodeCategory("Variables")]
[NodeTags("Get", "Int", "Integer", "Load")]
public class GetIntVariable(IVariableManager variableManager) : INode
{
    public string Name => "Get int variable";

    public void Execute([NodeInput("Variable")] string variable, [NodeOutput("Value")] out int value)
    {
        value = variableManager.GetVariable<int>(variable);
    }
}
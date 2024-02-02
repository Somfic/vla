using Vla.Addon.Services;

namespace Vla.Addon.Core.Variables;

[Node]
[NodeCategory("Variables")]
[NodeTags("Get", "String", "Load")]
public class GetStringVariable(IVariableManager variableManager) : INode
{
    public string Name => "Get string variable";

    public void Execute([NodeInput("Variable")] string variable, [NodeOutput("Value")] out string value)
    {
        value = variableManager.GetVariable<string>(variable);
    }
}
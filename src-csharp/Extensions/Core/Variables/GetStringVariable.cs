using Vla.Abstractions;
using Vla.Abstractions.Attributes;
using Vla.Nodes;

namespace Vla.Extensions.Core.Variables;

[Node("Get string variable")]
[NodeCategory("Variables")]
[NodeTags("Get", "String", "Load")]
public class GetStringVariable(VariableManager variableManager) : INode
{
    public string Name => "Get string variable";

    public void Execute([NodeInput("Variable")] string variable, [NodeOutput("Value")] out string value)
    {
        value = variableManager.GetVariable<string>(variable);
    }
}
using Vla.Abstractions;
using Vla.Nodes;
using Vla.Nodes.Attributes;

namespace Vla.Extensions.Core.Variables;

[Node("Set string variable")]
[NodeCategory("Variables")]
[NodeTags("Set", "String", "Save")]
public class SetStringVariable(VariableManager variableManager) : INode
{
    public string Name => "Set string variable";

    public void Execute([NodeInput("Variable")] string variable, [NodeInput("Value")] string value)
    {
        variableManager.SetVariable(variable, value);
    }
}
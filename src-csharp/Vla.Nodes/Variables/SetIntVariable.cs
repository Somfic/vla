using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Nodes.Variables;

[Node("Set int variable")]
[NodeCategory("Variables")]
[NodeTags("Set", "Int", "Integer", "Save")]
public class SetIntVariable(VariableManager variableManager) : INode
{
    public string Name => "Set int variable";

    public void Execute([NodeInput("Variable")] string variable, [NodeInput("Value")] int value)
    {
        variableManager.SetVariable(variable, value);
    }
}
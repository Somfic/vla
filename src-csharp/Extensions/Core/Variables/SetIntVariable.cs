using Vla.Abstractions;
using Vla.Abstractions.Attributes;
using Vla.Nodes;

namespace Vla.Extensions.Core.Variables;

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
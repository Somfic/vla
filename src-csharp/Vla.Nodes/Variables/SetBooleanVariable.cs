using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Nodes.Variables;

[Node("Set boolean variable")]
[NodeCategory("Variables")]
[NodeTags("Set", "Boolean", "Bool", "Save")]
public class SetBooleanVariable(VariableManager variableManager) : INode
{
    public string Name => "Set boolean variable";

    public void Execute([NodeInput("Variable")] string variable, [NodeInput("Value")] bool value)
    {
        variableManager.SetVariable(variable, value);
    }
}
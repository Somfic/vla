using Vla.Abstractions;
using Vla.Nodes;
using Vla.Nodes.Attributes;

namespace Vla.Extensions.Core.Variables;

[Node("Set decimal variable")]
[NodeCategory("Variables")]
[NodeTags("Set", "Decimal", "Float", "Double", "Save")]
public class SetDecimalVariable(VariableManager variableManager) : INode
{
    public string Name => "Set decimal variable";

    public void Execute([NodeInput("Variable")] string variable, [NodeInput("Value")] double value)
    {
        variableManager.SetVariable(variable, value);
    }
}
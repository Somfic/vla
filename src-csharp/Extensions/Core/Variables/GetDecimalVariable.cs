using Vla.Abstractions;
using Vla.Nodes;
using Vla.Nodes.Attributes;

namespace Vla.Extensions.Core.Variables;

[Node("Get decimal variable")]
[NodeCategory("Variables")]
[NodeTags("Get", "Decimal", "Float", "Double", "Load")]
public class GetDecimalVariable(VariableManager variableManager) : INode
{
    public string Name => "Get decimal variable";

    public void Execute([NodeInput("Variable")] string variable, [NodeOutput("Value")] out double value)
    {
        value = variableManager.GetVariable<double>(variable);
    }
}
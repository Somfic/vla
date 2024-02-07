using Vla.Addon.Services;

namespace Vla.Addon.Core.Variables;

[Node(Purity.Deterministic)]
[NodeCategory("Variables")]
[NodeTags("Get", "Decimal", "Float", "Double", "Load")]
public class GetDecimalVariable(IVariableManager variableManager) : INode
{
    public string Name => "Get decimal variable";

    public void Execute([NodeInput("Variable")] string variable, [NodeOutput("Value")] out double value)
    {
        value = variableManager.GetVariable<double>(variable);
    }
}
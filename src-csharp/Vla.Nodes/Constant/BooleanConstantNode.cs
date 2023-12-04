using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Nodes.Constant;

[Node]
public class BooleanConstantNode : INode
{
	public string Name => "Boolean constant";
    
	[NodeProperty]
	public bool Value { get; set; }

	public void Execute([NodeOutput("Value")] out bool value)
	{
		value = Value;
	}
}
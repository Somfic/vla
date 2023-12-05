using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Nodes.Math;

[Node("Clamp math")]
[NodeCategory("Math")]
[NodeTags("Math", "Clamp", "Clamp01", "Clamp 0-1", "Min", "Max")]
public class ClampMathNode : INode
{
	public string Name => "Clamp";
	
	public void Execute([NodeInput("Value")] double value, [NodeInput("Min")] double min, [NodeInput("Max")] double max, [NodeOutput("Result")] out double result)
	{
		result = System.Math.Clamp(value, min, max);
	}
}
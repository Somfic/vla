using Vla.Nodes.Attributes;

namespace Vla.Nodes;

[Node]
public class MathNode : INode
{
	[NodeProperty]
	public MathMode Mode { get; set; } = MathMode.Addition;

	public void Execute([NodeInput("A")]double a, [NodeInput("B")]double b, [NodeOutput("Result")]out double result)
	{
		result = Mode switch
		{
			MathMode.Addition => a + b,
			MathMode.Subtraction => a - b,
			_ => throw new ArgumentOutOfRangeException(nameof(Mode), Mode, null)
		};
	}
	
	public enum MathMode
	{
		Addition,
		Subtraction
	}
}
using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Nodes.Math;

[Node]
public class BasicMathNode : INode
{
	public string Name => $"Math {Mode.ToString().ToLower()}";

	[NodeProperty]
	public MathMode Mode { get; set; } = MathMode.Add;

	public void Execute([NodeInput("A")] double a, [NodeInput("B")] double b, [NodeOutput("Result")] out double result)
	{
		result = Mode switch
		{
			MathMode.Add => a + b,
			MathMode.Subtract => a - b,
			MathMode.Multiply => a * b,
			MathMode.Divide => a / b,
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	public enum MathMode
	{
		Add,
		Subtract,
		Multiply,
		Divide
	}
}
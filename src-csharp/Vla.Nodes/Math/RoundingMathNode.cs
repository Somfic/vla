using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Nodes.Math;

[Node]
public class RoundingMathNode : INode
{
	public string Name => $"Math {Mode.ToString().ToLower()}";

	[NodeProperty]
	public MathMode Mode { get; set; } = MathMode.Round;

	public void Execute([NodeInput("Value")] double value, [NodeOutput("Result")] out double result)
	{
		result = Mode switch
		{
			MathMode.Round => System.Math.Round(value),
			MathMode.Floor => System.Math.Floor(value),
			MathMode.Ceil => System.Math.Ceiling(value),
			MathMode.Truncate => System.Math.Truncate(value),
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	public enum MathMode
	{
		Round,
		Floor,
		Ceil,
		Truncate
	}
}
using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Nodes.Math;

[Node("Rounding math")]
[NodeCategory("Math")]
[NodeTags("Math", "Rounding", "Round", "Floor", "Ceil", "Truncate")]
public class RoundingMathNode : INode
{
	public string Name => $"Math {Mode.GetValueName()}";

	[NodeProperty]
	public MathMode Mode { get; set; } = MathMode.Round;

	public void Execute([NodeInput("Value")] double value, [NodeOutput("Result")] out int result)
	{
		result = Mode switch
		{
			MathMode.Round => (int)System.Math.Round(value),
			MathMode.Floor => (int)System.Math.Floor(value),
			MathMode.Ceil => (int)System.Math.Ceiling(value),
			MathMode.Truncate => (int)System.Math.Truncate(value),
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
using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Nodes.Math;

[Node]
public class TrigonometryNode : INode
{
	public string Name => $"Math {Mode.ToString().ToLower()}";

	[NodeProperty]
	public MathMode Mode { get; set; } = MathMode.Sin;

	public void Execute([NodeInput("Value")] double value, [NodeOutput("Result")] out double result)
	{
		result = Mode switch
		{
			MathMode.Sin => System.Math.Sin(value),
			MathMode.Cos => System.Math.Cos(value),
			MathMode.Tan => System.Math.Tan(value),
			MathMode.Asin => System.Math.Asin(value),
			MathMode.Acos => System.Math.Acos(value),
			MathMode.Atan => System.Math.Atan(value),
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	public enum MathMode
	{
		Sin,
		Cos,
		Tan,
		Asin,
		Acos,
		Atan
	}
}
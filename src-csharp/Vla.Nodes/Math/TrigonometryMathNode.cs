using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Nodes.Math;

[Node("Trigonometry math")]
[NodeCategory("Math")]
[NodeTags("Math", "Sin", "Cos", "Tan", "Asin", "Acos", "Atan", "Sine", "Cosine", "Tangent", "Arcsine", "Arccosine", "Arctangent", "Trigonometry", "Hyperbolic", "Hyperbolic sine", "Hyperbolic cosine", "Hyperbolic tangent", "Hyperbolic arcsine", "Hyperbolic arccosine", "Hyperbolic arctangent", "Sinh", "Cosh", "Tanh", "Asinh", "Acosh", "Atanh")]
public class TrigonometryMathNode : INode
{
	public string Name => $"Math {Mode.GetValueName()}{(Hyperbolic ? " (hyperbolic)" : "")}";

	[NodeProperty]
	public MathMode Mode { get; set; } = MathMode.Sin;
	
	[NodeProperty]
	public bool Hyperbolic { get; set; } = false;

	public void Execute([NodeInput("Value")] double value, [NodeOutput("Result")] out double result)
	{
		result = Mode switch
		{
			MathMode.Sin => Hyperbolic ? System.Math.Sinh(value) : System.Math.Sin(value),
			MathMode.Cos => Hyperbolic ? System.Math.Cosh(value) : System.Math.Cos(value),
			MathMode.Tan => Hyperbolic ? System.Math.Tanh(value) : System.Math.Tan(value),
			MathMode.Asin => Hyperbolic ? System.Math.Asinh(value) : System.Math.Asin(value),
			MathMode.Acos => Hyperbolic ? System.Math.Acosh(value) : System.Math.Acos(value),
			MathMode.Atan => Hyperbolic ? System.Math.Atanh(value) : System.Math.Atan(value),
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
		Atan,
	}
}
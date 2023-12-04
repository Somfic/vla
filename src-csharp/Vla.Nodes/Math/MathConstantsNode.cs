using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Nodes.Math;

[Node("Math constants")]
[NodeCategory("Math")]
[NodeTags("Math", "Constants", "Pi", "E", "Tau", "π", "τ")]
public class MathConstantsNode : INode
{
	public string Name => $"Math {Mode.GetValueName()}";

	[NodeProperty]
	public MathMode Mode { get; set; } = MathMode.Pi;

	public void Execute([NodeOutput("Constant")] out double value)
	{
		value = Mode switch
		{
			MathMode.Pi => System.Math.PI,
			MathMode.E => System.Math.E,
			MathMode.Tau => System.Math.Tau,
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	public enum MathMode
	{
		Pi,
		E,
		Tau
	}
}
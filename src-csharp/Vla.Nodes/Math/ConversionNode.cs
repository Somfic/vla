using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Nodes.Math;

[Node]
public class ConversionNode : INode
{
	public string Name => $"Math {Mode.ToString().ToLower()}";

	[NodeProperty]
	public ConversionMode Mode { get; set; } = ConversionMode.ToRadians;

	public void Execute([NodeInput("Value")] double value, [NodeOutput("Result")] out double result)
	{
		result = Mode switch
		{
			ConversionMode.ToRadians => value * System.Math.PI / 180,
			ConversionMode.ToDegrees => value * 180 / System.Math.PI,
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	public enum ConversionMode
	{
		ToRadians,
		ToDegrees
	}
}
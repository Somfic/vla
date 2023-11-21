using System;
using Vla.Nodes.Attributes;

namespace Vla.Nodes;

[Node]
public class MathNode : INode
{
	[NodeProperty]
	public MathMode Mode { get; set; } = MathMode.Addition;

	public void Execute([NodeInput("A")]int a, [NodeInput("B")]int b, [NodeOutput("Result")]out int result)
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

[Node]
public class MathModulo : INode
{
	public void Execute(
		[NodeInput("Value")] double value, 
		[NodeInput("Modulo")] double modulo, 
		[NodeOutput("Result")] out int result, 
		[NodeOutput("Rest")]out int rest, 
		[NodeOutput("Has rest")]out bool hasRest)
	{
		result = (int) (value % modulo);
		rest = (int) (value - result);
		hasRest = result != 0;
	}
}
using Vla.Input;
using Vla.Nodes.Attributes;

namespace Vla.Nodes;

[Node]
public class MathNode : INode
{
	public string Name => Mode.ToString();

	[NodeProperty]
	public MathMode Mode { get; set; } = MathMode.Addition;

	public void Execute([NodeInput("A")] int a, [NodeInput("B")] int b, [NodeOutput("Result")] out int result)
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
	public string Name => "Modulo";

	public void Execute(
		[NodeInput("Value")] double value,
		[NodeInput("Modulo")] double modulo,
		[NodeOutput("Result")] out int result,
		[NodeOutput("Rest")] out int rest,
		[NodeOutput("Has rest")] out bool hasRest)
	{
		result = (int)(value % modulo);
		rest = (int)(value - result);
		hasRest = result != 0;
	}
}

[Node]
public class PressKeyNode(InputService input) : INode
{
	public string Name => "Press key";

	public void Execute([NodeInput("Key")] string key)
	{
		input.Press(key);
	}
}
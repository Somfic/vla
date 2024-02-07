namespace Vla.Addon.Core.Math;

[Node(Purity.Deterministic)]
[NodeCategory("Math")]
[NodeTags("Add", "Subtract", "Multiply", "Divide", "+", "-", "*", "/", "Plus", "Minus", "Times")]
public class BasicMathNode : INode
{
    public string Name => Mode.GetValueName();

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
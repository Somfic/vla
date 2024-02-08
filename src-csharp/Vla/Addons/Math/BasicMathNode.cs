using System.Collections.Immutable;

namespace Vla.Addon.Core.Math;

[Node]
[NodeCategory("Math")]
[NodeTags("Add", "Subtract", "Multiply", "Divide", "+", "-", "*", "/", "Plus", "Minus", "Times")]
public class BasicMathNode : Node
{
    public override string Name => Mode.GetValueName();
    
    [NodeProperty]
    public MathMode Mode { get; set; } = MathMode.Add;

    public override ImmutableArray<NodeOutput> Execute()
    {
        var a = Input("A", 0d);
        var b = Input("B", 0d);

        var result = Mode switch
        {
            MathMode.Add => a + b,
            MathMode.Subtract => a - b,
            MathMode.Multiply => a * b,
            MathMode.Divide => a / b,
            _ => throw new ArgumentOutOfRangeException()
        };

        return [Output("Result", result)];
    }
    
    public enum MathMode
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }
}
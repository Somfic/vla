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

    public override Task Execute()
    {
        var a = Input<double>("A", 0);
        var b = Input<double>("B", 0);

        var result = Mode switch
        {
            MathMode.Add => a + b,
            MathMode.Subtract => a - b,
            MathMode.Multiply => a * b,
            MathMode.Divide => a / b,
            _ => throw new ArgumentOutOfRangeException()
        };


        Output("Result", result);

        return Task.CompletedTask;
    }
    
    public enum MathMode
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }
}
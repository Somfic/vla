namespace Vla.Addon.Core.Math;

[Node(Purity.Deterministic)]
[NodeCategory("Math")]
[NodeTags("Math", "Conversion", "Radians", "Degrees")]
public class ConversionMathNode : INode
{
    public string Name => Mode.GetValueName();

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
        [NodeEnumValue("To radians")]
        ToRadians,

        [NodeEnumValue("To degrees")]
        ToDegrees
    }
}
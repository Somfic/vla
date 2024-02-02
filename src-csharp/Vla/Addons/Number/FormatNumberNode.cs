namespace Vla.Addon.Core.Number;

[Node]
[NodeCategory("Numbers")]
[NodeTags("Format", "Pretty")]
public class FormatNumberNode : INode
{
    public string Name => $"{Mode.GetValueName()} format";

    [NodeProperty]
    public FormatMode Mode { get; set; } = FormatMode.General;

    private static readonly Dictionary<FormatMode, string> FormatStrings = new() {
        { FormatMode.Scientific, "E" },
        { FormatMode.FixedPoint, "F" },
        { FormatMode.General, "G" },
        { FormatMode.Number, "N" },
        { FormatMode.Currency, "C" },
        { FormatMode.Percentage, "P" },
        { FormatMode.Exponential, "E" },
        { FormatMode.RoundTrip, "R" }
    };

    public void Execute([NodeInput("Number")] double number, [NodeOutput("Formatted number")] out string formattedNumber)
    {
        formattedNumber = number.ToString(FormatStrings[Mode]);
    }

    public enum FormatMode
    {
        [NodeEnumValue("Scientific notation")]
        Scientific,
        [NodeEnumValue("Fixed-point notation")]
        FixedPoint,
        [NodeEnumValue("General notation")]
        General,
        [NodeEnumValue("Number")]
        Number,
        [NodeEnumValue("Currency")]
        Currency,
        [NodeEnumValue("Percentage")]
        Percentage,
        [NodeEnumValue("Exponential")]
        Exponential,
        [NodeEnumValue("Round-trip")]
        RoundTrip,
    }
}


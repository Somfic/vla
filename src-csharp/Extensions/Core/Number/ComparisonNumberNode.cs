using Vla.Abstractions;
using Vla.Nodes;
using Vla.Nodes.Attributes;

namespace Vla.Extensions.Core.Number;

[Node("Compare numbers")]
[NodeCategory("Number")]
[NodeTags("Compare", "Number", "Double", "Float", "Decimal", "Integer", "Equal", "Not equal", "Greater than", "Greater than or equal to", "Less than", "Less than or equal to", "Bigger", "Smaller", "==", "!=", ">", ">=", "<", "<=")]
public class ComparisonNumberNode : INode
{
    public string Name => Mode.GetValueName();

    [NodeProperty]
    public ComparisonMode Mode { get; set; }

    public void Execute([NodeInput("Value")] double a, [NodeInput("Compare to")] double b, [NodeOutput("Result")] out bool result)
    {
        result = Mode switch
        {
            ComparisonMode.Equal => System.Math.Abs(a - b) < 0.000000001,
            ComparisonMode.NotEqual => System.Math.Abs(a - b) > 0.000000001,
            ComparisonMode.GreaterThan => a > b,
            ComparisonMode.GreaterThanOrEqual => a >= b,
            ComparisonMode.LessThan => a < b,
            ComparisonMode.LessThanOrEqual => a <= b,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public enum ComparisonMode
    {
        [NodeEnumValue("Equal to")]
        Equal,
        [NodeEnumValue("Not equal to")]
        NotEqual,
        [NodeEnumValue("Greater than")]
        GreaterThan,
        [NodeEnumValue("Greater than or equal to")]
        GreaterThanOrEqual,
        [NodeEnumValue("Less than")]
        LessThan,
        [NodeEnumValue("Less than or equal to")]
        LessThanOrEqual
    }
}
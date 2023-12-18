using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Extensions.Core.Booleans;

[Node("Compare booleans")]
[NodeCategory("Booleans")]
[NodeTags("Compare", "Operator", "Equals", "==", "!=", "Comparison")]
public class ComparisonBooleanNode : INode
{
    public string Name { get; }

    [NodeProperty]
    public ComparisonMode Mode { get; set; }

    public void Execute([NodeInput("Value")] bool a, [NodeInput("Compare to")] bool b, [NodeOutput("Result")] out bool result)
    {
        result = Mode switch
        {
            ComparisonMode.Equal => a == b,
            ComparisonMode.NotEqual => a != b,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public enum ComparisonMode
    {
        [NodeEnumValue("Equal to")]
        Equal,
        [NodeEnumValue("Not equal to")]
        NotEqual
    }
}
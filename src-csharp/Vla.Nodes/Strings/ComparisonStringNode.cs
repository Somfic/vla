using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Nodes.Strings;

[Node("Compare strings")]
[NodeCategory("Strings")]
[NodeTags("Compare", "Strings", "Equal", "Not equal", "Includes", "Starts with", "Ends with", "Contains", "Comparison", "==", "!=")]

public class ComparisonStringNode : INode
{
	public string Name { get; }
	
	[NodeProperty]
	public ComparisonMode Mode { get; set; }
	
	public void Execute([NodeInput("Value")] string a, [NodeInput("Compare to")] string b, [NodeOutput("Result")] out bool result)
	{
		result = Mode switch
		{
			ComparisonMode.Equal => a == b,
			ComparisonMode.NotEqual => a != b,
			ComparisonMode.StartsWith => a.StartsWith(b),
			ComparisonMode.EndsWith => a.EndsWith(b),
			ComparisonMode.Contains => a.Contains(b),
			_ => throw new ArgumentOutOfRangeException()
		};
	}
	
	public enum ComparisonMode
	{
		[NodeEnumValue("Equal to")]
		Equal,
		[NodeEnumValue("Not equal to")]
		NotEqual,
		[NodeEnumValue("Starts with")]
		StartsWith, 
		[NodeEnumValue("Ends with")]
		EndsWith, 
		[NodeEnumValue("Contains")]
		Contains
	}
}
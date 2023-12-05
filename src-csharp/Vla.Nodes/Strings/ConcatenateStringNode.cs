using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Nodes.Strings;

[Node("Concatenate string")]
[NodeCategory("Strings")]
[NodeTags("Strings", "Concatenate", "Concat", "Join", "Append", "Prepend")]
public class ConcatenateStringNode : INode
{
    public string Name => "Concatenate string";
    
    [NodeProperty]
    public bool AddSpace { get; set; } = true;
    
    public void Execute([NodeInput("A")] string a, [NodeInput("B")] string b, [NodeOutput("Result")] out string result)
    {
        result = AddSpace ? $"{a} {b}" : $"{a}{b}";
    }
}

[Node("Regex find string")]
[NodeCategory("Strings")]
[NodeTags("Strings", "Regex", "Find", "Search", "Match")]
public class RegexFindStringNode : INode {
    public string Name => "Regex find string";
    
    public void Execute([NodeInput("Input")] string input, [NodeInput("Pattern")] string pattern, [NodeOutput("Result")] out string result, [NodeOutput("Success")] out bool success)
    {
        var match = System.Text.RegularExpressions.Regex.Match(input, pattern);
        result = match.Value;
        success = match.Success;
    }
} 
namespace Vla.Addon.Core.Strings;

[Node(Purity.Deterministic)]
[NodeCategory("Strings")]
[NodeTags("Strings", "Regex", "Find", "Search", "Match")]
public class RegexFindStringNode : INode
{
    public string Name => "Find in string";

    public void Execute([NodeInput("Input")] string input, [NodeInput("Pattern")] string pattern, [NodeOutput("Result")] out string result, [NodeOutput("Success")] out bool success)
    {
        var match = System.Text.RegularExpressions.Regex.Match(input, pattern);
        result = match.Value;
        success = match.Success;
    }
}
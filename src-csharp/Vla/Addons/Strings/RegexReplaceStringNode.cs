namespace Vla.Addon.Core.Strings;

[Node(Purity.Deterministic)]
[NodeCategory("Strings")]
[NodeTags("Regex", "Replace")]
public class RegexReplaceStringNode : INode
{
    public string Name => "Replace in string";

    public void Execute([NodeInput("Input")] string input, [NodeInput("Pattern")] string pattern, [NodeInput("Replacement")] string replacement, [NodeOutput("Result")] out string result, [NodeOutput("Success")] out bool success)
    {
        result = System.Text.RegularExpressions.Regex.Replace(input, pattern, replacement);
        success = System.Text.RegularExpressions.Regex.IsMatch(input, pattern);
    }
}
namespace Vla.Addon.Core.Strings;

[Node]
[NodeCategory("Strings")]
[NodeTags("Strings", "Concatenate", "Concat", "Join", "Append", "Prepend")]
public class ConcatenateStringNode : INode
{
    public string Name => "Concatenate strings";

    [NodeProperty]
    public bool AddSpace { get; set; } = true;

    public void Execute([NodeInput("A")] string a, [NodeInput("B")] string b, [NodeOutput("Result")] out string result)
    {
        result = AddSpace ? $"{a} {b}" : $"{a}{b}";
    }
}
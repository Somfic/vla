using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Nodes.Strings;

[Node("Concatenate string")]
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
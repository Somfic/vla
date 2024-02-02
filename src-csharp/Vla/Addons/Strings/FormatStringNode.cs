namespace Vla.Addon.Core.Strings;

[Node]
[NodeCategory("Strings")]
[NodeTags("Format", "Pretty")]
public class FormatStringNode : INode
{
    public string Name => "Format string";

    public void Execute([NodeInput("Value")] object value, [NodeInput("Template")] string template, [NodeOutput("Formatted string")] out string formattedString)
    {
        formattedString = string.Format(template, value);
    }
}
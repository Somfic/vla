namespace Vla.Nodes.Structure;

public readonly struct PropertyStructure
{
    public PropertyStructure(string name, Type type, string defaultValue)
    {
        Name = name;
        Type = type;
        DefaultValue = defaultValue;
        HtmlType = type.Name switch
        {
            "Int32" => "number",
            "Double" => "number",
            "String" => "text",
            "Boolean" => "checkbox",
            _ => "text"
        };
    }

    public string Name { get; init; }
	
    public Type Type { get; init; }
    
    public string HtmlType { get; init; }
	
    public string DefaultValue { get; init; }
}
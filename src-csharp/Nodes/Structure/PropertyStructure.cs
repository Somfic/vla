namespace Vla.Nodes.Structure;

public readonly struct PropertyStructure
{
    public PropertyStructure(string name, string type, string defaultValue)
    {
        Name = name;
        Type = type;
        DefaultValue = defaultValue;
    }

    public string Name { get; init; }
	
    public string Type { get; init; }
	
    public string DefaultValue { get; init; }
}
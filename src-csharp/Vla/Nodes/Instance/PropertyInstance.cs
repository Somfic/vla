namespace Vla.Nodes.Instance;

public readonly struct PropertyInstance
{
    public PropertyInstance(string name, Type type, string value)
    {
        Name = name;
        Type = type;
        Value = value;
    }

    public string Name { get; init; }
    
    public Type Type { get; init; }
    
    public string Value { get; init; }
}
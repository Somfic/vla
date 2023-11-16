namespace Vla.Nodes.Instance;

public readonly struct PropertyInstance
{
    public PropertyInstance(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; init; }
		
    public string Value { get; init; }
}
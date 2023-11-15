namespace Vla.Nodes.Instance;

public readonly struct Property
{
    public Property(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; init; }
		
    public string Value { get; init; }
}
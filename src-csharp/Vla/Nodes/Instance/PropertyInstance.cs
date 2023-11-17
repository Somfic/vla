namespace Vla.Nodes.Instance;

public readonly struct PropertyInstance
{
    {
        Name = name;
        Value = value;
    }

    public string Name { get; init; }
    public string Value { get; init; }
}
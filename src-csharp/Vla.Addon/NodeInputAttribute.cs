namespace Vla.Addon;

[AttributeUsage(AttributeTargets.Property)]
public class NodeInputAttribute(string? name = null) : Attribute
{
    public string? Name { get; } = name;
}

public readonly struct NodeInput(string name, dynamic value) : INodeValue
{
    public string Name { get; init; } = name;

    public dynamic Value { get; init; } = value;
}

public readonly struct NodeOutput(string name, dynamic value) : INodeValue
{
    public string Name { get; init; } = name;

    public dynamic Value { get; init; } = value;
}

public interface INodeValue
{
    public string Name { get; }
    
    public dynamic Value { get; }
}
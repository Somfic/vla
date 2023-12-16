namespace Vla.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class NodeOutputAttribute : Attribute
{
    public NodeOutputAttribute(string name = "")
    {
        Name = name;
    }
	
    public string Name { get; }
}
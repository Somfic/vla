namespace Vla.Nodes.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class NodeEnumValueAttribute(string name) : Attribute
{
	public string Name { get; } = name;
}
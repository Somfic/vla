namespace Vla.Addon;

[AttributeUsage(AttributeTargets.Field)]
public class NodeEnumValueAttribute(string name, string category = "") : Attribute
{
	public string Name { get; } = name;
	
	public string Category { get; } = category;
}
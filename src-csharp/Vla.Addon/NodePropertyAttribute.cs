namespace Vla.Addon;

[AttributeUsage(AttributeTargets.Property)]
public class NodePropertyAttribute(string? name = null) : Attribute
{
	public string? Name { get; } = name;
}
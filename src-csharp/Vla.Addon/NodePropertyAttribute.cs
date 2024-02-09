namespace Vla.Addon;

/// <summary>
/// This attribute is used to mark a property as a node property.
/// </summary>
/// <param name="name">The display name of the property. If not set, the property name is used.</param>
[AttributeUsage(AttributeTargets.Property)]
public class NodePropertyAttribute(string? name = null) : Attribute
{
	public string? Name { get; } = name;
}
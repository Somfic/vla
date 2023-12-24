namespace Vla.Addon;

[AttributeUsage(AttributeTargets.Class)]
public class NodeTagsAttribute(params string[] tags) : Attribute
{
	public string[] Tags { get; } = tags;
}
namespace Vla.Addon;

/// <summary>
///		This attribute is used to mark an outgoing branch of a node.
/// </summary>
/// <param name="name">The identifier of the branch.</param>
/// <param name="label">The display name of the branch.</param>
[AttributeUsage(AttributeTargets.Property)]
public class OutgoingBranchAttribute(string name, string label) : Attribute
{
	public string Name { get; } = name;
	
	public string Label { get; } = label;
}
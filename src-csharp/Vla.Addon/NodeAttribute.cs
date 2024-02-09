namespace Vla.Addon;

/// <summary>
/// This attribute is used to mark a class as a node.
/// </summary>
/// <param name="purity">The purity of the node. Defaults to <see cref="NodePurity.Deterministic"/>.</param>
[AttributeUsage(AttributeTargets.Class)]
public class NodeAttribute(NodePurity purity = NodePurity.Deterministic) : Attribute
{
	public NodePurity Purity { get; } = purity;
}
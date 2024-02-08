namespace Vla.Addon;

[AttributeUsage(AttributeTargets.Class)]
public class NodeAttribute(NodePurity purity = NodePurity.Deterministic) : Attribute
{
	public NodePurity Purity { get; } = purity;
}
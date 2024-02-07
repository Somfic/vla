namespace Vla.Addon;

[AttributeUsage(AttributeTargets.Class)]
public class NodeAttribute(Purity Purity) : Attribute
{
	public Purity Purity { get; } = Purity;
}

public enum Purity
{
	/// <summary>
	/// A pure function. The result will always be the same, given the same inputs.
	/// </summary>
	Deterministic,
	
	/// <summary>
	/// An impure function. The result may vary, given the same inputs.
	/// </summary>
	Probabilistic
}
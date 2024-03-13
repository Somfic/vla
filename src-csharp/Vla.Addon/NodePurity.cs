namespace Vla.Addon;

public enum NodePurity
{
	/// <summary>
	///     A pure node.
	///     The node will always produce the same outputs given the same inputs.
	/// </summary>
	Deterministic,

	/// <summary>
	///     An impure node.
	///     The node may not always produce the same outputs given the same inputs.
	/// </summary>
	Probabilistic
}
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("Vla")]

namespace Vla.Addon;

/// <summary>
///     Base class for all nodes.
/// </summary>
public abstract class Node
{
	/// <summary>
	///     The name of the node. This value can be modified mid-execution.
	/// </summary>
	public abstract string Name { get; }

	/// <summary>
	///     The unique identifier of the node.
	///     This value is automatically set by the engine when initialising the node.
	/// </summary>
	public Guid Id { get; internal set; }

	/// <summary>
	///     The purity of the node.
	///     Deterministic nodes always produce the same output for the same inputs, while non-deterministic nodes may produce
	///     different outputs for the same inputs.
	///     This value is automatically set by the engine when initialising the node.
	/// </summary>
	public NodePurity Purity { get; internal set; } = NodePurity.Deterministic;

	/// <summary>
	///     A dictionary of all inputs. This value is automatically set by the engine when executing the node.
	/// </summary>
	public ImmutableDictionary<string, dynamic> Inputs { get; internal set; } =
		ImmutableDictionary<string, dynamic>.Empty;

	/// <summary>
	///     A dictionary of all input labels. This value is automatically set by the engine when executing the node.
	/// </summary>
	public ImmutableDictionary<string, string> InputLabels { get; internal set; } =
		ImmutableDictionary<string, string>.Empty;

	/// <summary>
	///     A dictionary of all outputs. This value is automatically set by the engine when executing the node.
	/// </summary>
	public ImmutableDictionary<string, dynamic> Outputs { get; internal set; } =
		ImmutableDictionary<string, dynamic>.Empty;

	/// <summary>
	///     A dictionary of all output labels. This value is automatically set by the engine when executing the node.
	/// </summary>
	public ImmutableDictionary<string, string> OutputLabels { get; internal set; } =
		ImmutableDictionary<string, string>.Empty;

	/// <summary>
	///     A dictionary of all properties marked with <see cref="NodePropertyAttribute" />. This value is computed at runtime.
	/// </summary>
	public ImmutableDictionary<string, dynamic?> Properties => GetType().GetProperties()
		.Where(x => x.GetCustomAttribute<NodePropertyAttribute>() != null)
		.ToImmutableDictionary(p => p.Name, p => p.GetValue(this));
	
	/// <summary>
	///		A dictionary of all incoming branches marked with <see cref="IncomingBranchAttribute" />. This value is computed at runtime.
	/// </summary>
	public ImmutableDictionary<string, Branch> IncomingBranches => GetType().GetProperties()
		.Where(x => x.PropertyType == typeof(Branch) && x.GetCustomAttribute<IncomingBranchAttribute>() != null)
		.ToImmutableDictionary(p => p.Name, p => (Branch)p.GetValue(this)!);
	
	/// <summary>
	///		 A dictionary of all outgoing branches marked with <see cref="OutgoingBranchAttribute" />. This value is computed at runtime.
	/// </summary>
	public ImmutableDictionary<string, Branch> OutgoingBranches => GetType().GetProperties()
		.Where(x => x.PropertyType == typeof(Branch) && x.GetCustomAttribute<OutgoingBranchAttribute>() != null)
		.ToImmutableDictionary(p => p.Name, p => (Branch)p.GetValue(this)!);

	/// <summary>
	///     The asynchronous execution method of the node.
	///     This method is called when the node is executed.
	/// </summary>
	public abstract Task Execute();

	/// <summary>
	///     Pulls a value as input by name.
	///     If the input is not connected, the default value is used.
	///     Otherwise, the value of the connected output is converted to the target type and used.
	/// </summary>
	/// <param name="id">The id of the input. This value should be unique for each possible input in the node.</param>
	/// <param name="label">The label of the input.</param>
	/// <param name="defaultValue">The default value to use if the input is not connected.</param>
	/// <typeparam name="T">The target input type</typeparam>
	protected T? Input<T>(string id, string label, T defaultValue) 
	{
		if (typeof(T) == typeof(Branch))
			throw new InvalidOperationException("Cannot have a branch as an input. Branches should be registered as properties.");
		
		InputLabels = InputLabels.SetItem(id, label);

		// Check if the input is already set
		if (Inputs.TryGetValue(id, out var value))
			// TODO: Do we want to check if the value could be converted?
			//       This might give unexpected results if we're suddenly using the default value.
			try
			{
				// Try to convert the value to the desired type
				return SetInput(id, (T?)Convert.ChangeType(value, typeof(T)));
			}
			catch
			{
				// If the conversion fails, return the default value
				return SetInput(id, defaultValue);
			}

		// If this is the first time the input is accessed, return the default value
		return SetInput(id, defaultValue);
	}

	/// <summary>
	///     Pushes a value as output by name.
	/// </summary>
	/// <param name="id">The id of the input. This value should be unique for each possible input in the node.</param>
	/// <param name="label">The label of the output.</param>
	/// <param name="value">The value to output.</param>
	/// <typeparam name="T">The type of the output.</typeparam>
	protected void Output<T>(string id, string label, T value)
	{
		if (typeof(T) == typeof(Branch))
			throw new InvalidOperationException("Cannot have a branch as an output. Branches should be registered as properties.");
        
		OutputLabels = OutputLabels.SetItem(id, label);
		Outputs = Outputs.SetItem(id, value!);
	}

	internal T SetInput<T>(string id, T value)
	{
		Inputs = Inputs.SetItem(id, value!);
		return value;
	}
}

public struct Branch
{ 
	public bool Hit { get; private set; }
	
	public void Hits()
	{
		Hit = true;
	}

	internal void UnHit()
	{
		Hit = false;
	}
}
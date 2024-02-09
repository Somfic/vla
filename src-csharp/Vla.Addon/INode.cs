using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("Vla.Engine")]

namespace Vla.Addon;

/// <summary>
/// Base class for all nodes.
/// </summary>
public abstract class Node
{
	/// <summary>
	/// The name of the node. This value can be modified mid-execution.
	/// </summary>
	[JsonProperty("name")]
	public abstract string Name { get; }
	
	/// <summary>
	/// The unique identifier of the node.
	/// This value is automatically set by the engine when initialising the node.
	/// </summary>
	public Guid Id { get; internal set; }
    
    /// <summary>
    /// The purity of the node.
    /// Deterministic nodes always produce the same output for the same inputs, while non-deterministic nodes may produce different outputs for the same inputs.
    /// This value is automatically set by the engine when initialising the node.
    /// </summary>
    public NodePurity Purity { get; internal set; } = NodePurity.Deterministic;

    /// <summary>
    /// The asynchronous execution method of the node.
    /// This method is called when the node is executed.
    /// </summary>
    public abstract Task Execute();

    /// <summary>
    /// A dictionary of all inputs. This value is automatically set by the engine when executing the node.
    /// </summary>
    public ImmutableDictionary<string, dynamic?> Inputs { get; internal set; } = ImmutableDictionary<string, dynamic?>.Empty;
    
    /// <summary>
    /// A dictionary of all outputs. This value is automatically set by the engine when executing the node.
    /// </summary>
    public ImmutableDictionary<string, dynamic?> Outputs { get; internal set; } = ImmutableDictionary<string, dynamic?>.Empty;
    
    /// <summary>
    /// A dictionary of all properties marked with <see cref="NodePropertyAttribute"/>. This value is computed at runtime.
    /// </summary>
    public ImmutableDictionary<string, dynamic?> Properties => GetType().GetProperties().Where(x => x.GetCustomAttribute<NodePropertyAttribute>() != null).ToImmutableDictionary(p => p.Name, p => p.GetValue(this));
    
    /// <summary>
    /// Pulls a value as input by name.
    /// If the input is not connected, the default value is used.
    /// Otherwise, the value of the connected output is converted to the target type and used.
    /// </summary>
    /// <param name="name">The name of the input.</param>
    /// <param name="defaultValue">The default value to use if the input is not connected.</param>
    /// <typeparam name="T">The target input type</typeparam>
    protected T? Input<T>(string name, T defaultValue)
    {
	    // Check if the input is already set
	    if (Inputs.TryGetValue(name, out var value))
	    {
		    // TODO: Do we want to check if the value could be converted?
		    //       This might give unexpected results if we're suddenly using the default value.
		    try
		    {
			    // Try to convert the value to the desired type
			    return SetInput(name, (T?)Convert.ChangeType(value, typeof(T)));
		    }
		    catch
		    {
			    // If the conversion fails, return the default value
			    return SetInput(name, defaultValue);
		    }
	    }
	    
	    // If this is the first time the input is accessed, return the default value
	    return SetInput(name, defaultValue);
    }
    
    /// <summary>
    /// Pushes a value as output by name.
    /// </summary>
    /// <param name="name">The name of the output.</param>
    /// <param name="value">The value to output.</param>
    /// <typeparam name="T">The type of the output.</typeparam>
    protected void Output<T>(string name, T? value)
    {
	    Outputs = Outputs.SetItem(name, value);
    }
    
    internal T SetInput<T>(string name, T value)
    {
	    Inputs = Inputs.SetItem(name, value);
	    return value;
    } 
}
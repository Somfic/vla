using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("Vla.Engine")]

namespace Vla.Addon;

public abstract class Node
{
	public Guid Id { get; internal set; }
	
    [JsonProperty("name")]
    public abstract string Name { get; }

    public abstract Task<ImmutableArray<NodeOutput>> Execute();

    public ImmutableDictionary<string, dynamic?> Inputs { get; internal set; } = ImmutableDictionary<string, dynamic?>.Empty;
    
    public ImmutableDictionary<string, dynamic?> Outputs { get; private set; } = ImmutableDictionary<string, dynamic?>.Empty;
    
    protected T? Input<T>(string name, T defaultValue)
    {
	    // Check if the input is already set
	    if (Inputs.TryGetValue(name, out var value))
	    {
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
    
    protected NodeOutput Output<T>(string name, T? value)
    {
	    Outputs = Outputs.SetItem(name, value);
	    
	    return new NodeOutput(name, value);
    }
    
    internal T SetInput<T>(string name, T value)
    {
	    Inputs = Inputs.SetItem(name, value);
	    return value;
    } 
}
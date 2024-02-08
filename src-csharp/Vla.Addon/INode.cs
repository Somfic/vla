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
	    if (Inputs.TryGetValue(name, out var value))
		    return value;

	    Inputs = Inputs.SetItem(name, defaultValue);
        
        return defaultValue;
    }
    
    protected NodeOutput Output<T>(string name, T? value)
    {
	    Outputs = Outputs.SetItem(name, value);
	    
	    return new NodeOutput(name, value);
    }
}
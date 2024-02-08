using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Vla.Addon;

public abstract class Node
{
    [JsonProperty("name")]
    public abstract string Name { get; }

    public abstract ImmutableArray<NodeOutput> Execute();

    internal ImmutableDictionary<string, dynamic> Inputs = ImmutableDictionary<string, dynamic>.Empty;
    
    protected T Input<T>(string name, T defaultValue)
    {
	    if (Inputs.TryGetValue(name, out var value))
		    return value;

	    Inputs = Inputs.Add(name, defaultValue);
        
        return defaultValue;
    }
    
    protected NodeOutput Output<T>(string name, T value)
    {
	    return new NodeOutput(name, value);
    }
}
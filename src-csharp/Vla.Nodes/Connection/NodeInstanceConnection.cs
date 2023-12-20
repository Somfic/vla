using Newtonsoft.Json;

namespace Vla.Nodes.Connection;

public readonly struct NodeConnection(ConnectedProperty source, ConnectedProperty target)
{
    
    [JsonProperty("from")]
    public ConnectedProperty Source { get; init; } = source;

    [JsonProperty("to")]
    public ConnectedProperty Target { get; init; } = target;
}
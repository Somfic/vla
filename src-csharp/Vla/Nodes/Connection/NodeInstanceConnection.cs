using Newtonsoft.Json;

namespace Vla.Nodes.Connection;

public readonly struct NodeConnection
{

    [JsonProperty("from")]
    public ConnectedProperty From { get; init; }

    [JsonProperty("to")]
    public ConnectedProperty To { get; init; }
}
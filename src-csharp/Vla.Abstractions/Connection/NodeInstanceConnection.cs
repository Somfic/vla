using Newtonsoft.Json;
using Vla.Addon;

namespace Vla.Abstractions.Connection;

public readonly struct NodeConnection(Node sourceNode, string sourceOutput, Node targetNode, string targetInput)
{
    [JsonProperty("from")]
    public ConnectedProperty Source { get; init; } = new(sourceNode.Id, sourceOutput);

    [JsonProperty("to")]
    public ConnectedProperty Target { get; init; } = new(targetNode.Id, targetInput);
}
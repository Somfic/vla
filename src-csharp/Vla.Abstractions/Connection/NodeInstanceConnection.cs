using Newtonsoft.Json;
using Vla.Addon;

namespace Vla.Abstractions.Connection;

public readonly struct NodeConnection(Node sourceNode, string outputId, Node targetNode, string inputId)
{
    [JsonProperty("from")]
    public ConnectedProperty Source { get; init; } = new(sourceNode.Id, outputId);

    [JsonProperty("to")]
    public ConnectedProperty Target { get; init; } = new(targetNode.Id, inputId);
}
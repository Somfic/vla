using Newtonsoft.Json;
using Vla.Abstractions.Types;
using Vla.Nodes.Structure;

namespace Vla.Server.Messages;

public readonly struct NodesStructureMessage(IEnumerable<NodeStructure> nodes, IEnumerable<NodeTypeDefinition> types) : ISocketMessage
{
    [JsonProperty("nodes")]
    public IEnumerable<NodeStructure> Nodes { get; init; } = nodes;

    [JsonProperty("types")]
    public IEnumerable<NodeTypeDefinition> Types { get; init; } = types;
}
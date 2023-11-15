using Newtonsoft.Json;
using Vla.Nodes.Structure;

namespace Vla.Server.Messages;

public record SocketMessage
{
    public string Id => GetType().Name;
}

public record NodesStructureMessage(IEnumerable<NodeStructure> Nodes) : SocketMessage;
public record RecogniserRecognisedPartialMessage(string Json) : SocketMessage;
public record RecogniserRecognisedMessage(string Json) : SocketMessage;
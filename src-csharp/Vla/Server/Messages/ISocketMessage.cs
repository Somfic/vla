using System.Collections.Generic;
using Newtonsoft.Json;
using Vla.Nodes.Connection;
using Vla.Nodes.Instance;
using Vla.Nodes.Structure;
using Vla.Nodes.Types;
using Vla.Nodes.Web;
using Vla.Nodes.Web.Result;

namespace Vla.Server.Messages;

public record SocketMessage
{
    public string Id => GetType().Name.Replace("Message", string.Empty);
}

public record NodesStructureMessage(IEnumerable<NodeStructure> Nodes, IEnumerable<NodeTypeDefinition> Types) : SocketMessage;

public record RunWebMessage(Web Web) : SocketMessage;
public record WebMessage(Web Web) : SocketMessage;

public record WebResultMessage(WebResult Result) : SocketMessage;
public record RecogniserRecognisedPartialMessage(string Json) : SocketMessage;
public record RecogniserRecognisedMessage(string Json) : SocketMessage;
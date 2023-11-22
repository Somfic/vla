using Catalyst;
using Vla.Nodes.Structure;
using Vla.Nodes.Types;
using Vla.Nodes.Web;
using Vla.Nodes.Web.Result;
using Whisper.net;

namespace Vla.Server.Messages;

public record SocketMessage
{
    public string Id => GetType().Name.Replace("Message", string.Empty);
}

public record NodesStructureMessage(IEnumerable<NodeStructure> Nodes, IEnumerable<NodeTypeDefinition> Types) : SocketMessage;

public record RunWebMessage(Web Web) : SocketMessage;
public record WebMessage(Web Web) : SocketMessage;

public record WebResultMessage(WebResult Result) : SocketMessage;
public record RecogniserRecognisedPartial(SegmentData Speech) : SocketMessage;
public record RecogniserRecognised(IReadOnlyCollection<IToken> SpeechTokens) : SocketMessage;

public record Progress(float Percentage, string Label) : SocketMessage;
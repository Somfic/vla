using Catalyst;
using Newtonsoft.Json;

namespace Vla.Server.Messages;

public readonly struct RecogniserRecognisedMessage(IEnumerable<IToken> speechTokens) : ISocketMessage
{
	[JsonProperty("speechTokens")]
	public IEnumerable<IToken> SpeechTokens { get; init; } = speechTokens;
}
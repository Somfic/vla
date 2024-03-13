using Newtonsoft.Json;

namespace Vla.Server.Messages.Messages;

public readonly struct ReadyStateMessage(bool ready) : ISocketMessage
{
	[JsonProperty("ready")]
	public bool Ready { get; init; } = ready;
}
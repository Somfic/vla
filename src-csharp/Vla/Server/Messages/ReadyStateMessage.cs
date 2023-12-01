using Newtonsoft.Json;

namespace Vla.Server.Messages;

public readonly struct ReadyStateMessage(bool ready) : ISocketMessage
{
	[JsonProperty("ready")]
	public bool Ready { get; init; } = ready;
}
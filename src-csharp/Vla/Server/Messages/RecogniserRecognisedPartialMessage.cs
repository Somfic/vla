using Newtonsoft.Json;
using Whisper.net;

namespace Vla.Server.Messages;

public readonly struct RecogniserRecognisedPartialMessage(SegmentData speech) : ISocketMessage
{
	public static implicit operator RecogniserRecognisedPartialMessage(SegmentData speech) => new(speech);

	[JsonProperty("speech")]
	public SegmentData Speech { get; init; } = speech;
}
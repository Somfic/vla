using Newtonsoft.Json;

namespace Vla.Server.Messages;

public readonly struct ProgressMessage(float percentage, string label) : ISocketMessage
{
	[JsonProperty("percentage")]
	public float Percentage { get; init; } = percentage;

	[JsonProperty("label")]
	public string Label { get; init; } = label;
}
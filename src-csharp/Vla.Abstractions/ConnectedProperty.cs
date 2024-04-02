using Newtonsoft.Json;

namespace Vla.Abstractions;

public readonly struct ConnectedProperty(string instanceId, string propertyId)
{
	[JsonProperty("node")]
	public string NodeId { get; init; } = instanceId;

	[JsonProperty("id")]
	public string Id { get; init; } = propertyId;
}
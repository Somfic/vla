using Newtonsoft.Json;

namespace Vla.Abstractions;

public readonly struct NodeConnection(Guid fromNodeId, string fromOutputId, Guid toNodeId, string toInputId)
{
	[JsonProperty("from")]
	public ConnectedProperty Source { get; init; } = new(fromNodeId, fromOutputId);

	[JsonProperty("to")]
	public ConnectedProperty Target { get; init; } = new(toNodeId, toInputId);
}
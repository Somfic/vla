using Newtonsoft.Json;

namespace Vla.Abstractions;

public readonly struct NodeConnection(string fromNodeId, string fromOutputId, string toNodeId, string toInputId)
{
	[JsonProperty("from")]
	public ConnectedProperty Source { get; init; } = new(fromNodeId, fromOutputId);
	
	[JsonProperty("to")]
	public ConnectedProperty Target { get; init; } = new(toNodeId, toInputId);
}
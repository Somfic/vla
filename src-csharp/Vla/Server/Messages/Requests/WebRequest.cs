using Newtonsoft.Json;

namespace Vla.Server.Messages.Requests;

public readonly struct WebRequest(Abstractions.Web web) : ISocketRequest
{
	[JsonProperty("web")]
	public Abstractions.Web Web { get; init; } = web;
}
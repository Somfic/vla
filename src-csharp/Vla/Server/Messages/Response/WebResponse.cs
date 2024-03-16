using Newtonsoft.Json;

namespace Vla.Server.Messages.Response;

public readonly struct WebResponse(Abstractions.Web web) : ISocketResponse
{
	[JsonProperty("web")]
	public Abstractions.Web Web { get; init; } = web;
}
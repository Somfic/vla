using Newtonsoft.Json;
using Vla.Nodes.Web;

namespace Vla.Server.Messages;

public readonly struct WebMessage(Web web) : ISocketMessage
{
	public static implicit operator WebMessage(Web web) => new(web);

	[JsonProperty("web")]
	public Web Web { get; init; } = web;
}
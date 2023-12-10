using Newtonsoft.Json;
using Vla.Abstractions.Web;
using Vla.Nodes.Web;

namespace Vla.Server.Messages;

public readonly struct RunWebMessage(Web web) : ISocketMessage
{
	public static implicit operator RunWebMessage(Web web) => new(web);

	[JsonProperty("web")]
	public Web Web { get; init; } = web;
}
using Newtonsoft.Json;

namespace Vla.Server.Messages;

public readonly struct RunWebMessage(Abstractions.Web.Web web) : ISocketMessage
{
    public static implicit operator RunWebMessage(Abstractions.Web.Web web) => new(web);

    [JsonProperty("web")]
    public Abstractions.Web.Web Web { get; init; } = web;
}

public readonly struct UpdateWebMessage(Abstractions.Web.Web web) : ISocketMessage
{
    [JsonProperty("web")]
    public Abstractions.Web.Web Web { get; init; } = web;
    
    [JsonProperty("workspace")]
    public string WorkspacePath { get; init; } = string.Empty;
}
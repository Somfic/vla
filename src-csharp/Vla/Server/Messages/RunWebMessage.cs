using Newtonsoft.Json;
using Vla.Abstractions.Web;

namespace Vla.Server.Messages;

public readonly struct RunWebMessage(Abstractions.Web.Web web) : ISocketMessage
{
    public static implicit operator RunWebMessage(Abstractions.Web.Web web) => new(web);

    [JsonProperty("web")]
    public Abstractions.Web.Web Web { get; init; } = web;
}

public readonly struct WorkspaceChangedMessage(Abstractions.Web.Workspace workspace) : ISocketMessage
{
    public static implicit operator WorkspaceChangedMessage(Abstractions.Web.Workspace workspace) => new(workspace);

    [JsonProperty("workspace")]
    public Abstractions.Web.Workspace Workspace { get; init; } = workspace;
}
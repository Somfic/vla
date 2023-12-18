using Newtonsoft.Json;

namespace Vla.Server.Messages;

public readonly struct WorkspacesMessage(IEnumerable<Abstractions.Web.Workspace> workspaces) : ISocketMessage
{
    [JsonProperty("workspaces")]
    public IEnumerable<Abstractions.Web.Workspace> Workspaces { get; init; } = workspaces;
}
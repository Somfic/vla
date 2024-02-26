using Newtonsoft.Json;

namespace Vla.Server.Messages;

public readonly struct WorkspacesMessage(IEnumerable<Abstractions.Workspace> workspaces) : ISocketMessage
{
    [JsonProperty("workspaces")]
    public IEnumerable<Abstractions.Workspace> Workspaces { get; init; } = workspaces;
}
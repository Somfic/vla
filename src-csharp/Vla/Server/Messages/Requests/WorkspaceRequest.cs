using Newtonsoft.Json;

namespace Vla.Server.Messages.Requests;

public readonly struct WorkspaceRequest(Abstractions.Workspace workspace) : ISocketRequest
{
	[JsonProperty("workspace")]
	public Abstractions.Workspace Workspace { get; init; } = workspace;
}
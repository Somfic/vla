using Newtonsoft.Json;

namespace Vla.Server.Messages.Response;

public readonly struct WorkspaceResponse(Abstractions.Workspace workspace) : ISocketResponse
{
	[JsonProperty("workspace")]
	public Abstractions.Workspace Workspace { get; init; } = workspace;
}
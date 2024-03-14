using Newtonsoft.Json;

namespace Vla.Server.Messages.Requests;

public readonly struct CreateWorkspaceRequest(string name, string path) : ISocketRequest
{
	[JsonProperty("name")]
	public string Name { get; init; } = name;

	[JsonProperty("path")]
	public string Path { get; init; } = path;
}

public readonly struct WorkspaceRequest(Abstractions.Workspace workspace) : ISocketRequest
{
	[JsonProperty("workspace")]
	public Abstractions.Workspace Workspace { get; init; } = workspace;
}
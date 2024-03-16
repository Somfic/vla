using Newtonsoft.Json;

namespace Vla.Server.Messages.Requests;

public readonly struct WebByNameRequest(string name, string workspacePath) : ISocketRequest
{
	[JsonProperty("name")]
	public string Name { get; init; } = name;

	[JsonProperty("workspacePath")]
	public string WorkspacePath { get; init; } = workspacePath;
}
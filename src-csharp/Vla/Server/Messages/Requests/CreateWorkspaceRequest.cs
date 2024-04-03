using Newtonsoft.Json;

namespace Vla.Server.Messages.Requests;

public readonly struct CreateWorkspaceRequest(string name, string path) : ISocketRequest
{
	[JsonProperty("name")]
	public string Name { get; init; } = name;

	[JsonProperty("path")]
	public string Path { get; init; } = path;
}

public record struct GenerateTypeRequest(Type type) : ISocketRequest
{
	[JsonProperty("type")]
	public Type Type { get; init; } = type;
}